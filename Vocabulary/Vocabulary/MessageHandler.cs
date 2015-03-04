using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary
{
    public enum MessageType
    {
        TransferRequest = 1,
        TransferConfirm = 2,
        Temp = 3,
        Data = 4,
        DataACK = 5,
        OutRequest = 6,
        OutConfirm = 7,
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

    public abstract class Message
    {
        MessageType messageType;
        Sense messageSense;
        Content content;
    }

    public class TransferRequest : Message
    {
        private MessageType _messageType = MessageType.TransferRequest;
        private Sense _messageSense = Sense.SenderToReceiver;
        private Content _content = Content.Request;
        private String _command = "PUT ";

        public TransferRequest(String file)
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

    public class TransferConfirm : Message
    {
        private MessageType _messageType = MessageType.TransferConfirm;
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

    public class Data : Message
    {
        private MessageType _messageType = MessageType.Data;
        private Sense _messageSense = Sense.SenderToReceiver;
        private Content _content = Content.Data;
        private int _nSec = 0;
        private byte[] _nBytes = new byte[512];

        public Data(byte[] nbytes)
        {
            _nBytes = nbytes;
        }
    }

    public class DataACK : Message
    {
        private MessageType _messageType = MessageType.DataACK;
        private Sense _messageSense = Sense.ReceiverToSender;
        private Content _content = Content.Request;
        private int _nSec;

        public DataACK(int nSec)
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

    public class OutRequest : Message
    {
        private MessageType _messageType = MessageType.OutRequest;
        private Sense _messageSense = Sense.SenderToReceiver;
        private Content _content = Content.Request;
    }

    public class OutConfirm : Message
    {
        private MessageType _messageType = MessageType.OutConfirm;
        private Sense _messageSense = Sense.ReceiverToSender;
        private Content _content = Content.Confirmation;
    }

    public abstract class State
    {
        protected delegate void MessageHandler(MessageType type);
        private Dictionary<MessageType, MessageHandler> _map = new Dictionary<MessageType, MessageHandler>();

        protected void RegisterHandler(MessageType type, MessageHandler handler)
        {
            _map.Add(type, handler);
        }

        public void HandleMessage(MessageType type)
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
            RegisterHandler(MessageType.Temp, SendRequest);
        }

        protected void SendRequest(MessageType type)
        {

        }
    }
}
