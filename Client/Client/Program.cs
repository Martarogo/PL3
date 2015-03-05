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
        protected Client _context;

        protected delegate void MessageHandler(PacketBodyType packetType);
        private Dictionary<PacketBodyType, MessageHandler> _map = new Dictionary<PacketBodyType, MessageHandler>();

        public State(Client context)
        {
            _context = context;
        }

        protected void RegisterHandler(PacketBodyType packetType, MessageHandler handler)
        {
            _map.Add(packetType, handler);
        }

        public void HandleMessage(PacketBodyType packetType)
        {
            try
            {
                MessageHandler handler = _map[packetType];
                handler.Invoke(packetType);
            }
            catch (KeyNotFoundException)
            {
                //OnUnknownMessage(type);
            }
        }
    }


    class WaitConfirmation : State
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


    class SendData : State
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
        private int sec = 1;
        private String strRec;
        private byte[] bFile, bSent, bReceived;
        private readonly String fichName = "fichero.txt";
        private State _state;

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

        public PacketBodyType ReceivePacket()
        {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

            bReceived = client.Receive(ref remoteIPEndPoint);

            return encoding.Decode(bReceived).Type;
        }

        public void Send()
        {
            int length = ReadFile();

            Packet data = new Data((int)PacketBodyType.Data, length, bFile);

            bSent = encoding.Encode(data);

            client.Send(bSent, bSent.Length, SERVER, SERVERPORT);

            if (length < 512)
            {
                Packet discon = new Discon((int)PacketBodyType.Discon,0,null);
                ChangeState(new WaitConfirmation(this));
            }
        }

        private int ReadFile()
        {
            FileStream fs = new FileStream(fichName, FileMode.Open, FileAccess.Read);
            
            return fs.Read(bFile, 0, 512);
        }

        private void Close()

        public void ChangeState(State state)
        {
            _state = state;
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
