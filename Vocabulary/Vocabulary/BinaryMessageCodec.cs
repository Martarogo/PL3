using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vocabulary;

namespace Vocabulary
{
    abstract class ICodec<T>
    {
        public abstract byte[] Encode(T message);
        public abstract T Decode(Stream source);

        public T Decode(byte[] packet)
        {
            using (Stream payload = new MemoryStream(packet, 0, packet.Length, false))
                return Decode(payload);
        }
    }


    abstract class BinaryCodec<T> : ICodec<T>
    {
        public abstract void WriteBinaryData(BinaryWriter writer, T message);
        public abstract T ReadBinaryData(BinaryReader reader);

        public override byte[] Encode(T message)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                WriteBinaryData(writer, message);

                writer.Flush();
                return ms.ToArray();
            }
        }

        public override T Decode(Stream source)
        {
            using (BinaryReader reader = new BinaryReader(source))
            {
                return ReadBinaryData(reader);
            }
        }
    }


    class PacketBinaryCodec : BinaryCodec<Packet>
    {
        public override void WriteBinaryData(BinaryWriter writer, Packet message)
        {
            writer.Write((int)message.Type);
            writer.Write(message.BodyLength);
            writer.Write(message.Body);
        }

        public override Packet ReadBinaryData(BinaryReader reader)
        {
            int typeRaw = reader.ReadInt32();
            int bodyLength = reader.ReadInt32();

            byte[] body = new byte[bodyLength];
            reader.Read(body, 0, bodyLength);

            return new Packet(typeRaw, bodyLength, body);
        }
    }
}
