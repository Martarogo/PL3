using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vocabulary;
using System.Net.Sockets;
using System.IO;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte[] bFile = null;
            UdpClient client = null;
            
            PacketBinaryCodec encoding = new PacketBinaryCodec();
            try
            {
                client = new UdpClient();

                bFile = new byte[new FileInfo("fichero.txt").Length];

                FileStream fs = new FileStream("fichero.txt", FileMode.Open, FileAccess.Read);

                for (int i = 0; i < bFile.Length; i++)
                {
                    bFile[i] = (byte)fs.ReadByte();
                }

                Packet newFile = new NewFile((int)PacketBodyType.NewFile, bFile.Length, bFile);


                /*
                Console.WriteLine("Type: " + newFile.Type);
                Console.WriteLine("BodyLength: " + newFile.BodyLength);
                Console.WriteLine("Body: " + newFile.Body);
                */
                //client.Send(sent, sent.Length, SERVER, SERVERPORT);
                Assert.AreEqual(3, bFile.Length);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}
