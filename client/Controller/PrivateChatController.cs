using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

using client.Model;

namespace client.Controller
{
    class PrivateChatController
    {
        private TcpClient client;
        private string hostname;

        public PrivateChatController(string host)
        {
            hostname = host;
        }

        public bool connectToPrivateChatQueue(User curUser, SEX LookingForSex)
        {
            bool success = false;
            client = new TcpClient(hostname, PortManager.instance().Matchport);
            NetworkStream stream = client.GetStream();

            string msg = curUser.Username + "|" + (int)curUser.AgeCategory + "|" + (int)curUser.Gender + "|" + (int)LookingForSex;
            Trace.WriteLine(msg);

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);            
            stream.Write(data, 0, data.Length);

            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data);
            Trace.WriteLine("Received from matchserver: {0}", responseData);

            success = (responseData == "ok");

            return success;
        }

        public void handeMessaging()
        {

        }
    }
}
