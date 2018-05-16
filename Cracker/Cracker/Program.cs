using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using CrackMe;

namespace Cracker
{
    public class Program
    {
        static void Main(string[] args)
        {
            var key = GenerateKeyForCrackMe();
            Console.WriteLine("Key for crack me is: " + key + ". Please run the CrackMeForm and try to pass.");
            Console.ReadLine();
        }

        public static string GenerateKeyForCrackMe()
        {
            var allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            var networkInterface = allNetworkInterfaces.FirstOrDefault();

            if (networkInterface != null)
            {
                var addressBytes = networkInterface.GetPhysicalAddress().GetAddressBytes();
                var dateBytes = BitConverter.GetBytes(DateTime.Now.Date.ToBinary());
                var list = new List<int>();

                for (var i = 0; i < addressBytes.Length; i++)
                {
                    var value = (dateBytes[i] ^ addressBytes[i]) * 10;
                    list.Add(value);
                }

                var key = string.Join("-", list);
                return key;
            }
            return null;
        }

    }
}
