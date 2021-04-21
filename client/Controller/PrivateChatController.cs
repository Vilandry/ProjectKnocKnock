using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading;

using client.Model;

namespace client.Controller
{
    public class PrivateChatController
    {
        private TcpClient client;
        private bool ongoingChat;


        public event EventHandler<MessageArrivedEventArgs> MessageArrived;


        public PrivateChatController()
        {
            
        }

        public bool HandlePrivateChatting(User curUser, GENDER LookingForSex)
        {
            bool success = false;
            client = new TcpClient(PortManager.instance().Host, PortManager.instance().Matchport);
            NetworkStream stream = client.GetStream();

            if ( !connectToPrivateChatQueue(curUser, LookingForSex))
            {
                Trace.WriteLine("Chatconnecting error at phase 1");
                return false;
            }

            try
            {
                while(true)
                {
                    Thread.Sleep(300);
                    Trace.WriteLine("PrivateChat: get matchport...");
                    string matchinfo = Utility.ReadFromNetworkStream(stream);
                    int chatport = int.Parse(matchinfo);

                    Trace.WriteLine("PrivateChat: trying to verify...");
                    string verify = Utility.ReadFromNetworkStream(stream);
                    if (verify == "OK")
                    {
                        Trace.WriteLine("PrivateChat: verified!");
                        curUser.HasOngoingChat = true;
                        ongoingChat = true;

                        client = new TcpClient(PortManager.instance().Host, chatport);
                        Console.Beep();                        
                        Thread t = new Thread(handleReading);
                        t.Start();
                        break;
                    }
                    else
                    {
                        Trace.WriteLine("Failed matchmaking attempt.");
                    }
                }                
            }
            catch(Exception e)
            {
                Trace.WriteLine("Chatconnecting error at phase 2. Error message: " + e.Message);
            }

            return success;
        }

        private bool connectToPrivateChatQueue(User curUser, GENDER LookingForSex)
        {
            bool success = false;
            try
            {
                NetworkStream stream = client.GetStream();
                string msg = curUser.Username + "|" + (int)curUser.AgeCategory + "|" + (int)curUser.Gender + "|" + (int)LookingForSex;

                byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                string responseData = Utility.ReadFromNetworkStream(stream);

                Trace.WriteLine("\nReceived from matchserver: " + responseData + "\n");

                success = (responseData == "OK");
            }
            catch(Exception e)
            {
                Trace.WriteLine("Error during privatechat connection: " + e.Message);
            }




            return success;
        }

        public void handeMessaging(string username, string message)
        {
            if(ongoingChat)
            {
                if(message == "")
                {
                    Trace.WriteLine("PrivateChat Warning: empty message");
                    return;
                }
                NetworkStream stream = client.GetStream();
                string toBeSend = username + "|" + message;
                Trace.WriteLine("Privatechat message sent: " + message);

                byte[] buffer = System.Text.Encoding.ASCII.GetBytes(toBeSend);
                stream.Write(buffer);
            }
            else
            {
                Trace.WriteLine("PrivateChat Warning: no ongoing chat!");
            }

        }

        private void handleReading()
        {
            while(ongoingChat)
            {
                NetworkStream stream = client.GetStream();

                try
                {
                    string raw_info = Utility.ReadFromNetworkStream(stream);

                    string sender = raw_info.Split("|", 2)[0];
                    string msg = raw_info.Split("|", 2)[1];
                    MessageArrivedEventArgs e = new MessageArrivedEventArgs();
                    e.MessageSender = sender;
                    e.Message = msg;
                    OnMessageArrived(e);
                }
                catch(Exception e)
                {
                    Trace.WriteLine("Smth shit happened in private chat, error message: " + e.Message);
                }
            }
        }

        protected virtual void OnMessageArrived(MessageArrivedEventArgs e)
        {
            MessageArrived?.Invoke(this, e);
        }

        public void ExitChat(User curUser)
        {
            if(ongoingChat)
            {
                string msg = "!LEAVE";
                ongoingChat = false;
                client.Close();

                curUser.HasOngoingChat = false;
            }

        }



    }
}
