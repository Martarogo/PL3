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
            UdpClient client = null;
            
            try
            {
                client = new UdpClient();

                FileStream fs = new FileStream("fichero.txt", FileMode.Open, FileAccess.Read);
                
                
                //client.Send(sent, sent.Length, SERVER, SERVERPORT);

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
