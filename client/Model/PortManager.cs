using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client.Model
{
    public class PortManager
    {
        private static PortManager manager;
        private static readonly object llock = new object();
        private int matchport;
        private int loginport;
        private int infoport;
        private string host;

        public static PortManager instance()
        {
            if (manager == null)
            {
                manager = new PortManager();

            }

            return manager;
        }

        private PortManager()
        {
            matchport = 9900;
            loginport = 11000;

            infoport = 9000; ///this should be const

            host = "localhost";
        }


        public int Matchport { get { return matchport; } }
        public int Loginport { get { return loginport; } }
        public int Infoport { get { return infoport; } }
        public string Host { get { return host; } }
    }
}

