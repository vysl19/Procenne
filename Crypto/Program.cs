using Business;
using System;

namespace Crypto
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] key = { 69, 109, 191, 233, 151, 137, 88, 7, 158, 70, 234, 137, 84, 49, 55, 1 };
            byte[] iv = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            var rpcServer = new RpcServer(new TripleDESCryptoService(key, iv));
            rpcServer.InitializeAndRun();
            Console.Read();
        }
    }
}
