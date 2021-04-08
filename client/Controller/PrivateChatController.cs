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

        public bool connectToPrivateChatSearcher(User user)
        {
            return false;
        }

        public void handeMessaging()
        {

        }
    }
}
