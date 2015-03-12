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

namespace Servidor
{
    abstract class State
    {
        protected Server _context;

        protected delegate void MessageHandler(PacketBodyType packetType);
        private Dictionary<PacketBodyType, MessageHandler> _map = new Dictionary<PacketBodyType, MessageHandler>();

        public State(Server context)
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


    class Wait : State
    {
        public Wait(Server context) : base(context)
        {
            RegisterHandler(PacketBodyType.NewFile, OnNewFile);
            RegisterHandler(PacketBodyType.Data, OnData);
            RegisterHandler(PacketBodyType.Discon, OnDiscon);
        }

        protected void OnNewFile(PacketBodyType packetType)
        {
            _context.GetName();
            _context.SendConfirmation((int)PacketBodyType.AckNewFile);
        }

        protected void OnData(PacketBodyType packetType)
        {
            _context.CheckData();
        }

        protected void OnDiscon(PacketBodyType packetType)
        {
            _context.SendConfirmation((int)PacketBodyType.AckDiscon);
        }
    }

    class Server
    {
        private static int PORT = 23456;
        private byte[] bRec = new byte[128], bACK;
        private String strRec;
        private String[] separador = { " | " }, mRec;
        private byte[] bSent, bReceived;
        private int nRec, sec = 1;
        private State _state;
        private Packet receivedPacket;
        private String fichName;

        private UdpClient client = null;
        PacketBinaryCodec encoding = new PacketBinaryCodec();
        IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);

        public void Run()
        {
            Console.WriteLine("Servidor en ejecución...");

            try
            {
                client = new UdpClient(PORT);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ErrorCode + ": " + se.Message);
                return;
            }

            ChangeState(new Wait(this));
            // El servidor se ejecuta infinitamente
            for (; ; )
            {
                try
                {
                    PacketBodyType packetType = ReceivePacket();
                    _state.HandleMessage(packetType);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: {0}", e.Message);
                }

            }
        }

        public void GetName()
        {
            fichName = Encoding.UTF8.GetString(receivedPacket.Body, 0, receivedPacket.BodyLength);
        }

        public PacketBodyType ReceivePacket()
        {
            bReceived = client.Receive(ref remoteIPEndPoint);

            receivedPacket = encoding.Decode(bReceived);

            return receivedPacket.Type;
        }

        public void SendConfirmation(int packetType)
        {
            byte[] body = Encoding.UTF8.GetBytes("OK");
            int bodyLength = body.Length;

            Packet newFileAck = new AckNewFile(packetType, bodyLength, body);

            bSent = encoding.Encode(newFileAck);

            client.Send(bSent, bSent.Length, remoteIPEndPoint);          
        }

        public void CheckData()
        {
            Data recData = (Data)receivedPacket;

            if (recData.NSec == sec)
            {
                WriteFile();
                SendAck();
                sec++;
            }
            else
            {
                Console.WriteLine("Se esperaba ACK " + sec + " y se recibió " + recData.NSec);
            }
        }

        private void WriteFile()
        {
            FileStream fs = new FileStream(fichName, FileMode.Create, FileAccess.Write);

            fs.Write()
        }

        private void SendAck()
        {
            Packet dataAck = new AckData((int)PacketBodyType.AckData, 0, null);

            bSent = encoding.Encode(dataAck);

            client.Send(bSent, bSent.Length, remoteIPEndPoint); 
        }

        public void ChangeState(State state)
        {
            _state = state;
        }

    }



    class Program
    {
        static void Main(string[] args)
        {
            Server serv = new Server();
            Thread hilo = new Thread(new ThreadStart(serv.Run));
            hilo.Start();
            hilo.Join();
        }
    }
}


