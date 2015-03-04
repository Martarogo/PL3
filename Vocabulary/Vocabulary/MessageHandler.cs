using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary
{
    public enum PacketBodyType
    {
        NewFile = 1,
        AckNewFile = 10,
        Data = 2,
        AckData = 20,
        Discon = 3,
        AckDiscon = 30
    };

    public enum Sense
    {
        SenderToReceiver = 1,
        ReceiverToSender = 2,
    };

    public enum Content
    {
        Request = 1,
        Confirmation = 2,
        Data = 3,
    };

    public class Packet
    {
        private PacketBodyType _type;
        private int _bodyLength;
        private byte[] _body;

        public Packet(int typeRaw, int bodyLength, byte[] body) {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
        }

        public PacketBodyType Type
        {
            get
            {
                return _type;
            }
        }
        public int BodyLength
        {
            get
            {
                return _bodyLength;
            }
        }
        public byte[] Body
        {
            get
            {
                return _body;
            }
        }
    }

    public class NewFile : Packet
    {
        private PacketBodyType _messageType = PacketBodyType.NewFile;
        private Sense _messageSense = Sense.SenderToReceiver;
        private Content _content = Content.Request;
        private String _command = "PUT ";

        public NewFile(String file)
        {
            _command += file;
        }

        public String Command
        {
            get
            {
                return _command;
            }
        }
    }

    public class AckNewFile : Packet
    {
        private PacketBodyType _messageType = PacketBodyType.AckNewFile;
        private Sense _messageSense = Sense.ReceiverToSender;
        private Content _content = Content.Confirmation;
        private String _command = "OK";

        public String Command
        {
            get
            {
                return _command;
            }
        }
    }

    public class Data : Packet
    {
        private PacketBodyType _messageType = PacketBodyType.Data;
        private Sense _messageSense = Sense.SenderToReceiver;
        private Content _content = Content.Data;
        private int _nSec = 0;
        private byte[] _nBytes = new byte[512];

        public Data(byte[] nbytes)
        {
            _nBytes = nbytes;
        }
    }

    public class AckData : Packet
    {
        private PacketBodyType _messageType = PacketBodyType.AckData;
        private Sense _messageSense = Sense.ReceiverToSender;
        private Content _content = Content.Request;
        private int _nSec;

        public AckData(int nSec)
        {
            _nSec = nSec;
        }

        public int NSec
        {
            get
            {
                return _nSec;
            }
        }
    }

    public class Discon : Packet
    {
        private PacketBodyType _messageType = PacketBodyType.Discon;
        private Sense _messageSense = Sense.SenderToReceiver;
        private Content _content = Content.Request;
    }

    public class AckDiscon : Packet
    {
        private PacketBodyType _messageType = PacketBodyType.AckDiscon;
        private Sense _messageSense = Sense.ReceiverToSender;
        private Content _content = Content.Confirmation;
    }

    public abstract class State
    {
        protected delegate void MessageHandler(PacketBodyType type);
        private Dictionary<PacketBodyType, MessageHandler> _map = new Dictionary<PacketBodyType, MessageHandler>();

        protected void RegisterHandler(PacketBodyType type, MessageHandler handler)
        {
            _map.Add(type, handler);
        }

        public void HandleMessage(PacketBodyType type)
        {
            try
            {
                MessageHandler handler = _map[type];
                handler.Invoke(type);
            }
            catch (KeyNotFoundException)
            {
                //OnUnknownMessage(type);
            }
        }
    }


    public class WaitConfirmation: State
    {
        public WaitConfirmation()
        {
            //(PacketBodyType.Temp, SendRequest);
        }

        protected void SendRequest(PacketBodyType type)
        {

        }
    }
}
