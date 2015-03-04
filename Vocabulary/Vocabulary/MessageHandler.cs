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
        protected PacketBodyType _type;
        protected int _bodyLength;
        protected byte[] _body;

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
        private String _command = "PUT ";

        public NewFile(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
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
        private String _command = "OK";

        public AckNewFile(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
        }

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
        private int _nSec = 0;
        private byte[] _nBytes = new byte[512];

        public Data(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
        }
    }

    public class AckData : Packet
    {
        private int _nSec;

        public AckData(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
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
        public Discon(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
        }
    }

    public class AckDiscon : Packet
    {
        public AckDiscon(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = BodyLength;
            _body = body;
        }
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
