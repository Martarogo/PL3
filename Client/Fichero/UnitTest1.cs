using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Fichero
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte[] bFile = new byte[512];
            FileStream fs = new FileStream("fichero.txt", FileMode.Open, FileAccess.Read);

            int len = fs.Read(bFile, 0, 512);

            len = fs.Read(bFile, 0, 512);
            /*for (int i = 0; i < 70; i++)
            {
                bFile[i] = (byte)fs.ReadByte();
            }*/

            Assert.AreEqual(512, len);
        }
    }
}
