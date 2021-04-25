using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

using client.Model;

namespace client.Controller
{
    class MiscController
    {
        private TcpClient client;
        private static readonly object llock = new object();

        public MiscController()
        {

        }

        public bool sendPrivateChatHistory(string conversationID, string history)
        {
            bool success = true;
            history.Replace("<e>", "<e><e>");
            history.Replace("<f>", "<e><f>");
            history.Replace("\n", "<f>");
            Trace.WriteLine(history);

            lock(llock)
            {
                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    return false;
                }

                NetworkStream stream = client.GetStream();
                byte[] idData = Encoding.Unicode.GetBytes("CONVSAVE|" + conversationID);

                try
                {
                    stream.Write(idData);
                }
                catch(Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt send privatechat history id, error message: " + e.Message);
                    client.Close();
                    return false;
                }

                try
                {
                    Thread.Sleep(100);
                    string attemptResult = Utility.ReadFromNetworkStream(stream);
                    if(attemptResult == "OK")
                    {
                        return true;
                    }
                    else if(attemptResult == "INSERT")
                    {
                        byte[] data = Encoding.Unicode.GetBytes(history);

                        StreamWriter writer = new StreamWriter(stream);
                        writer.Write(data);
                    }
                }
                catch(Exception e)
                {
                    client.Close();
                    return false;
                }                
            }
            return success;
            
        }


    }
}
