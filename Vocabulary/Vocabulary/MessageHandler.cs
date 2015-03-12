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
            _bodyLength = bodyLength;
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
        public NewFile(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = bodyLength;
            _body = body;
        }
    }

    public class AckNewFile : Packet
    {
        public AckNewFile(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = bodyLength;
            _body = body;
        }
    }

    public class Data : Packet
    {
        private int _nSec;

        public Data(int nSec, int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _nSec = nSec;
            _type = (PacketBodyType)typeRaw;
            _bodyLength = bodyLength;
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

    public class AckData : Packet
    {
        private int _nSec;

        public AckData(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = bodyLength;
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
            _bodyLength = bodyLength;
            _body = body;
        }
    }

    public class AckDiscon : Packet
    {
        public AckDiscon(int typeRaw, int bodyLength, byte[] body) : base(typeRaw, bodyLength, body)
        {
            _type = (PacketBodyType)typeRaw;
            _bodyLength = bodyLength;
            _body = body;
        }
    }

}
