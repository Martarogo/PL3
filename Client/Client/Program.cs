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
        protected delegate void MessageHandler(PacketBodyType packetType);
        private Dictionary<PacketBodyType, MessageHandler> _map = new Dictionary<PacketBodyType, MessageHandler>();

        public abstract void ChangeState();

        public abstract void Send();

        public abstract void Close();

        public abstract void ReceivePacket();

        public virtual void RegisterHandler(PacketBodyType packetType, MessageHandler handler)
        {
            _map.Add(packetType, handler);
        }

        public virtual void HandleMessage()
        {
            try
            {
                Packet packet = Receive();

                MessageHandler handler = _map[packet.Type];

                if (handler == null)
                {
                    //OnUnknownMessage(type);
                }
                else
                {
                    handler.Invoke(packet.Type);
                }
                
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.TimedOut) {
                    OnTimeout();
                }
                else if (e.SocketErrorCode == SocketError.ConnectionReset) {
                    OnSocketClosed();
                }
                else {
                    OnSocketException(e);
                }
            }
            catch (PacketException e)
            {
                OnCorruptPacket(e);
            }
            catch (EndOfStreamException e)
            {
                OnCorruptPacket(e);
            }
            catch (Exception e)
            {
                OnUnknownException(e);
            }
        }
    }

    class SenderState : State
    {
        protected Client _context;
        protected State _state;

        public SenderState(Client context)
        {
            _context = context;
        }

        public override void ChangeState(State state)
        {
            _context.ChangeState(state);
        }

        public override void Send()
        {
            _context.Send();
        }

        public override PacketBodyType ReceivePacket()
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

        protected void OnAckNewFile(PacketBodyType packetType)
        {
            _context.Send();
            _context.ChangeState(new SendData(_context));
        }

        protected void OnAckDiscon(PacketBodyType packetType)
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

        protected void OnAckData(PacketBodyType packetType)
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
                    PacketBodyType packetType = ReceivePacket();
                    _state.HandleMessage(packetType);
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

            bFile = new byte[512];

            return fs.Read(bFile, 0, 512);
        }

        public void ChangeState(State state)
        {
            _state = state;
        }

        public void Send()
        {
            int length = ReadFile();

            Packet data = new Data(nSec, (int)PacketBodyType.Data, length, bFile);

            nSec++;

            bSent = encoding.Encode(data);

            client.Send(bSent, bSent.Length, SERVER, SERVERPORT);

            if (length < 512)
            {
                Packet discon = new Discon((int)PacketBodyType.Discon, 0, null);
                ChangeState(new WaitConfirmation(this));
            }
        }

        public PacketBodyType ReceivePacket()
        {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            bReceived = client.Receive(ref remoteIPEndPoint);

            return encoding.Decode(bReceived).Type;
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
