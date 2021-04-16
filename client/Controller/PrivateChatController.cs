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
        private int port;

        public PrivateChatController(int portnum, string host)
        {
            port = portnum;
            hostname = host;
        }

        public void connectToPrivateChatQueue(User curUser)
        {
            client = new TcpClient(hostname, port);

            string msg =""; 
        }

        public void handeMessaging()
        {

        }
    }
}
