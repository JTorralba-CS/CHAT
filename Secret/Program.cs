using System;

using Standard.Functions;

namespace Secret
{
    public class Program
    {
        static void Main(string[] args)
        {
            //string Message = "Data Source = EPTBCAD01; " +
            //    "Initial Catalog = Training_System; " +
            //    "User ID = AIDA; " +
            //    "Password = @ida123X; " +
            //    "Trust Server Certificate = True; " +
            //    "Connect Timeout = 1";

            string Message = "HelloWorld";

            RSAKeys RSAKeys = new RSAKeys();

            Console.WriteLine($"{Core.RSAEncrypt(RSAKeys.Public, Message)}");

            Console.WriteLine();

            Console.WriteLine($"{RSAKeys.Private}");

            Console.WriteLine();

            Console.WriteLine($"{Core.RSADecrypt(RSAKeys.Private, Core.RSAEncrypt(RSAKeys.Public, Message))}");

            Console.WriteLine();

            Console.WriteLine();
        }
    }
}
