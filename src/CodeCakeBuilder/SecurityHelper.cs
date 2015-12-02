using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace CodeCakeBuilder
{
    public class DecryptHelper : IDisposable
    {
        StreamReader StreamReader { get; }
        CryptoStream crStream { get; }

        public DecryptHelper(string path) : this(new FileStream(path, FileMode.Open, FileAccess.Read)) { }

        public DecryptHelper(FileStream fileStream)
        {
            DESCryptoServiceProvider cryptic = new DESCryptoServiceProvider();

            cryptic.Key = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");
            cryptic.IV = ASCIIEncoding.ASCII.GetBytes("ABCDEFGH");

            CryptoStream crStream = new CryptoStream(fileStream, cryptic.CreateDecryptor(), CryptoStreamMode.Read);

            StreamReader = new StreamReader(crStream);
        }



        public void Dispose()
        {
            crStream.Close();
            StreamReader.Close();
        }

        public void Decrypt()
        {


            string data = reader.ReadToEnd();


        }
    }
}
