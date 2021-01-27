using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;
using BC = BCrypt.Net.BCrypt;

namespace SecureDesk_WCF_Service.Algorithms
{
    public class Password
    {
        //this method returns the hash_password
        /*Path ti be followed for the hashing the password
         * 1:Create the salt 
         * 2:Hash the password using the SHA512
         * 3:Hash the password using the Bcrypt library
         */
        public static string [] giveHashPassword(string password)
        {
            //this array the contains the hashed password and the salt for the user
            string[] passwordHash_Salt = new string[2];
            string salt = generateSalt(7);                                           //get the salt for adding it to the password
            string pepper = "VIa2MxZu9A==";                                          //pepper used for adding to to the password+salt for the seceracy
            string hashed_password = generateSHA512Hash(password, salt, pepper);     //hashing the password by using the SHA512 algorithm
            string bcrypt_hash_password = Bcrpyt_Password(hashed_password);          //hashing the password using the Bcrypt 
            passwordHash_Salt[0] = salt;                                         
            passwordHash_Salt[1] = bcrypt_hash_password;

            //return the salt and the hashed password to the array return to the register service method
            return passwordHash_Salt;
        }

        //this method returns the slat for adding to the original password
        private static string generateSalt(int size)
        {
            //create the instance of the RNGCryptoServiceProvider for generating the random slat
            var random_number_generator = new RNGCryptoServiceProvider();
            //define byte array to hold the random data obtinaed by the RNGCryptoServiceProvider
            var buffer = new byte[size];
            random_number_generator.GetBytes(buffer);
            //converting to the BASE64 string and returing the string to the calling method
            return Convert.ToBase64String(buffer);
        }//method ends

        //This method converts the ByteArray to  the String
        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            //retunrns the hexadecimal string to the calling method
            return hex.ToString();
        }
        //method ends

        //This method hashed the password by adding the salt and pepper to the original password 
        //This method uses the SHA-512 algorihtm for hashing the password
        public static string generateSHA512Hash(string password, string salt, string pepper)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt + pepper);
            byte[] hash512;
            using (SHA512 shaM = new SHA512Managed())
            {
                hash512 = shaM.ComputeHash(bytes);
            }

            return ByteArrayToString(hash512);
        }
        //method ends

        //This method will returns the Bcrypted password 
        public static string Bcrpyt_Password(string hash)
        {
            var cost = 14;
            var timeTarget = 100; // Milliseconds
            long timeTaken;
            string bcrypt_hash = "";
            do
            {
                var sw = Stopwatch.StartNew();
                bcrypt_hash = BC.HashPassword(hash, workFactor: cost);
                sw.Stop();
                timeTaken = sw.ElapsedMilliseconds;
                cost -= 1;
            }
            while ((timeTaken) >= timeTarget);
            Console.WriteLine("Appropriate Cost Found: " + (cost + 1));
            return bcrypt_hash;
        }
    }
}