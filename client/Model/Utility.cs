using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.Model
{
    public static class Utility
    {
        public static string CreateMD5(string input) ///https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }

    public class MessageArrivedEventArgs : EventArgs
    {
        private string msg;
        private string msgsender;

        /// <summary>
        /// The text of the message
        /// </summary>
        public string Message { get { return msg; } set { msg = value; } }

        /// <summary>
        /// The username of the message's sender
        /// </summary>
        public string MessageSender { get { return msgsender; } set { msgsender = value; } }
    }

    public enum AGECATEGORY
    {
        SIXTEEN,
        TWENTY,
        TWENTYFIVEPLUS
    }


    public enum SEX
    {
        FEMALE,
        MALE,
        OTHER
    }
}
