using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;

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

        public static string MessageFormatter(string username, string message)
        {
            if(username == "SERVER")
            {
                string[] commandargs = message.Split("|");
                if(commandargs[0] == "LEFT")
                {
                    return commandargs[1] + " has disconnected!";
                }

                return "error?";
            }
            else
            {
                return username + ": " + message;
            }          
        }

        public static string ReadFromNetworkStream(NetworkStream stream)
        {
            byte[] bytes = new Byte[256];
            string message = null;
            int i = 0, byteCount = 0;
            do
            {
                i = stream.Read(bytes, 0, bytes.Length);
                Console.WriteLine(i);
                // Translate data bytes to a ASCII string.
                message = Encoding.Unicode.GetString(bytes, byteCount, i);
                byteCount += i;
            } while (stream.DataAvailable);
            Trace.WriteLine("\nUTILITY: read from networkstream: " + message + "\n");
            

            return message;
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
        YOUNG = 0,
        SEMI = 1,
        ADULT = 2
    }


    public enum GENDER
    {
        FEMALE = 0,
        MALE = 1,
        OTHER = 2,
        ANY = 3
    }

    public enum CHATTYPE
    {
        PRIVATECHAT = 0,
        GROUPCHAT = 1,
        FRIENDLYCHAT = 2
    }
}
