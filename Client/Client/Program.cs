using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            //(PacketBodyType.Temp, SendRequest);
        }

        protected void OnAckNewFile(PacketBodyType packetType)
        {

        }
    }


    class Client
    {
        private readonly String SERVER = "localhost";
        private readonly int SERVERPORT = 23456;
        private int sec = 1;
        private String strRec;
        private byte[] bFile, bSent;
        private State _state;

        UdpClient client = null;
        PacketBinaryCodec encoding = new PacketBinaryCodec();

        public void Run()
        {
            try
            {
                client = new UdpClient();

                readFile();

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
            return (PacketBodyType)4;
        }

        public void ChangeState(State state)
        {
            _state = state;
        }

        private void readFile()
        {
            bFile = new byte[new FileInfo("fichero.txt").Length];

            Console.WriteLine("BodyLength: " + bFile.Length);

            FileStream fs = new FileStream("fichero.txt", FileMode.Open, FileAccess.Read);

            for (int i = 0; i < bFile.Length; i++)
            {
                bFile[i] = (byte)fs.ReadByte();
            }
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
