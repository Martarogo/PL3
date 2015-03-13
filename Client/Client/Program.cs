using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vocabulary;

namespace Client
{

    abstract class State
    {
        protected delegate void MessageHandler(Packet packet);
        private Dictionary<PacketBodyType, MessageHandler> _map = new Dictionary<PacketBodyType, MessageHandler>();

        public abstract void ChangeState(SenderState state);

        public abstract void Send();

        public abstract void Close();

        public abstract Packet ReceivePacket();

        protected virtual void RegisterHandler(PacketBodyType packetType, MessageHandler handler)
        {
            _map.Add(packetType, handler);
        }

        public virtual void HandleMessage()
        {
            try
            {
                Packet packet = ReceivePacket();

                MessageHandler handler = _map[packet.Type];

                if (handler == null)
                {
                    OnUnknownMessage(packet);
                }
                else
                {
                    handler.Invoke(packet);
                }
                
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.TimedOut)
                {
                    OnTimeout();
                }
                else if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    OnSocketClosed();
                }
                else
                {
                    OnSocketException(e);
                }
            }
            /*catch (MessageException e)
            {
                OnCorruptPacket(e);
            }*/
            catch (EndOfStreamException e)
            {
                OnCorruptPacket(e);
            }
            catch (Exception e)
            {
                OnUnknownException(e);
            }
        }

        public void OnUnknownMessage(Packet packet)
        {
            Console.WriteLine("Unknown Message:\n" +
                                    "Type: " + packet.Type);
        }

        public void OnTimeout()
        {
            Console.WriteLine("TImeout");
        }

        public void OnSocketClosed()
        {
            Console.WriteLine("Error: closed socket");
        }

        public void OnSocketException(Exception e)
        {
            Console.WriteLine("Socket exception: " + e.Message);
        }

        public void OnCorruptPacket(Exception e)
        {
            Console.WriteLine("Corrupt packet: " + e.Message);
        }

        public void OnUnknownException(Exception e)
        {
            Console.WriteLine("Unknown exception: " + e.Message);
        }
    }

    class SenderState : State
    {
        protected Client _context;

        public SenderState(Client context)
        {
            _context = context;
        }

        public override void ChangeState(SenderState state)
        {
            _context.ChangeState(state);
        }

        public override void Send()
        {
            _context.Send();
        }

        public override Packet ReceivePacket()
        {
            return _context.ReceivePacket();
        }

        public override void Close()
        {
            _context.Close();
        }
    }

    class WaitConfirmation : SenderState
    {
        public WaitConfirmation(Client context) : base(context)
        {
            RegisterHandler(PacketBodyType.AckNewFile, OnAckNewFile);
            RegisterHandler(PacketBodyType.AckDiscon, OnAckDiscon);
        }

        protected void OnAckNewFile(Packet packet)
        {
            _context.Send();
            _context.ChangeState(new SendData(_context));
        }

        protected void OnAckDiscon(Packet packet)
        {
            _context.Close();
        }
    }


    class SendData : SenderState
    {
        public SendData(Client context) : base(context)
        {
            RegisterHandler(PacketBodyType.AckData, OnAckData);
        }

        protected void OnAckData(Packet packet)
        {
            _context.Send();
        }
    }


    class Client
    {
        private readonly String SERVER = "localhost";
        private readonly int SERVERPORT = 23456;
        private int nSec = 1;
        private String strRec;
        private byte[] bFile, bSent, bReceived;
        private readonly String fichName = "P3.jpg";
        private SenderState _state;

        UdpClient client = null;
        PacketBinaryCodec encoding = new PacketBinaryCodec();

        public void Run()
        {
            try
            {
                client = new UdpClient();

                bFile = Encoding.UTF8.GetBytes(fichName);

                Packet newFile = new NewFile((int)PacketBodyType.NewFile, bFile.Length, bFile);

                bSent = encoding.Encode(newFile);

                client.Send(bSent, bSent.Length, SERVER, SERVERPORT);

                ChangeState(new WaitConfirmation(this));

                for (; ; )
                {
                    //PacketBodyType packetType = ReceivePacket();
                    _state.HandleMessage();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            finally
            {
                client.Close();
            }
            Console.ReadKey();
        }

        private int ReadFile()
        {
            FileStream fs = new FileStream(fichName, FileMode.Open, FileAccess.Read);

            byte[] by = BitConverter.GetBytes(nSec);

            bFile = new byte[512];

            fs.Read(bFile, 0, 512);

            byte[] c = by.Concat(bFile).ToArray();

            Console.WriteLine("asdfas: " + c.Length);

            return fs.Read(bFile, 0, 512);
        }

        public void ChangeState(SenderState state)
        {
            _state = state;
        }

        public int GetFileBytes()
        {
            int length = ReadFile();

            Packet data = new Data(nSec, (int)PacketBodyType.Data, length, bFile);

            nSec++;

            bSent = encoding.Encode(data);

            return length;
        }

        public void Send()
        {
            int length = GetFileBytes();

            client.Send(bSent, bSent.Length, SERVER, SERVERPORT);

            SetTimer();

            if (length < 512)
            {
                Packet discon = new Discon((int)PacketBodyType.Discon, 0, null);
                ChangeState(new WaitConfirmation(this));
            }
        }

        public void SetTimer()
        {
            client.Client.ReceiveTimeout = 1000;
        }

        public Packet ReceivePacket()
        {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            bReceived = client.Receive(ref remoteIPEndPoint);

            return encoding.Decode(bReceived);
        }

        public void Close()
        {
            client.Close();
        }
        
        private void processException(Exception e)
        {
            if (e.InnerException != null)
            {
                if (e.InnerException is SocketException)
                {
                    SocketException se = (SocketException)e.InnerException;
                    if (se.SocketErrorCode == SocketError.TimedOut)
                    {
                        Console.WriteLine("Ha expirado el temporizador, se reenvia el mensaje");
                    }
                    else
                    {
                        Console.WriteLine("Error: " + se.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            const int N = 1;

            Thread[] threads = new Thread[N];
            for (int i = 0; i < N; i++)
            {
                Client client = new Client();
                threads[i] = new Thread(new ThreadStart(client.Run));
                threads[i].Start();
            }

            for (int i = 0; i < N; i++)
            {
                threads[i].Join();
            }

        }
    }
}
