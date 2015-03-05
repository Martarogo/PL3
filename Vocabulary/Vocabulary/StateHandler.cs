using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vocabulary;

namespace Vocabulary
{
    public abstract class State
    {
        protected delegate void MessageHandler(PacketBodyType packetType);
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


    public class WaitConfirmation : State
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
