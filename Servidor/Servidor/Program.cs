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
        protected delegate void MessageHandler(Packet packet);
        private Dictionary<PacketBodyType, MessageHandler> _map = new Dictionary<PacketBodyType, MessageHandler>();

        public abstract void ChangeState(ReceiverState state);

        public abstract void SendConfirmation(int packetType);

        public abstract Packet ReceivePacket();

        protected void RegisterHandler(PacketBodyType packetType, MessageHandler handler)
        {
            _map.Add(packetType, handler);
        }

        public void HandleMessage()
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


    class ReceiverState : State
    {
        protected Server _context;
        protected State _state;

        public ReceiverState(Server context)
        {
            _context = context;
        }

        public override void ChangeState(ReceiverState state)
        {
            _context.ChangeState(state);
        }

        public override void SendConfirmation(int packetType)
        {
            _context.SendConfirmation(packetType);
        }

        public override Packet ReceivePacket()
        {
            return _context.ReceivePacket();
        }
    }

    class Wait : ReceiverState
    {
        public Wait(Server context) : base(context)
        {
            RegisterHandler(PacketBodyType.NewFile, OnNewFile);
            RegisterHandler(PacketBodyType.Data, OnData);
            RegisterHandler(PacketBodyType.Discon, OnDiscon);
        }

        protected void OnNewFile(Packet packet)
        {
            _context.GetName();
            _context.SendConfirmation((int)PacketBodyType.AckNewFile);
        }

        protected void OnData(Packet packet)
        {
            _context.CheckData();
        }

        protected void OnDiscon(Packet packet)
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
                    _state.HandleMessage();
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

        public Packet ReceivePacket()
        {
            bReceived = client.Receive(ref remoteIPEndPoint);

            receivedPacket = encoding.Decode(bReceived);

            return receivedPacket;
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
            Packet recData = receivedPacket;

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


