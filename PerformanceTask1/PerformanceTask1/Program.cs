using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceTask1
{
    class Program
    {
        static void Main(string[] args)
        {
            var salt = Encoding.UTF8.GetBytes("sudsdfdfhghjdfsdfgffghghjjhkjk");
            var code = GeneratePasswordHashUsingSalt("aikasdsfsdgdfgdfh", salt);
            Console.ReadLine();
        }

        public static string GeneratePasswordHashUsingSalt(string passwordText, byte[] salt)
        {

            var iterate = 10000;
            var pbkdf2 = new Rfc2898DeriveBytes(passwordText, salt, iterate);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            for (int i = 0; i < 36; i++)
            {
                if (i < 16)
                {
                    hashBytes[i] = salt[i];
                }
                else
                {
                    hashBytes[i] = hash[i - 16];
                }
            }
            //Array.Copy(salt, 0, hashBytes, 0, 16);
            //Array.Copy(hash, 0, hashBytes, 16, 20);

            var passwordHash = Convert.ToBase64String(hashBytes);

            return passwordHash;
        }
    }
}
