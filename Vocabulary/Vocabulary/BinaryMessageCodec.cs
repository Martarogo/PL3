using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vocabulary
{
    public interface ICodec
    {
        byte[] Encode(Message message);
        String Decode(byte[] array);
    }

    namespace Vocabulary
    {
        public class BinaryMessageCodec : ICodec
        {
            public byte[] Encode(Message message)
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    //writer.Write(message.packet);

                    writer.Flush();
                    return ms.ToArray();
                }
            }

            public String Decode(byte[] array)
            {
                using (MemoryStream ms = new MemoryStream(array))
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    return reader.ReadString();
                }
            }
        }
    }
}
