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
        volatile User curUser;

        public event EventHandler<MessageArrivedEventArgs> MessageArrived;
        public event EventHandler chatEnded;
        public event EventHandler chatBegins;

        public PrivateChatController(User cu)
        {
            curUser = cu;
        }

        public bool HandlePrivateChatting(GENDER LookingForSex)
        {
            bool success = false;
            client = new TcpClient(PortManager.instance().Host, PortManager.instance().Matchport);
            NetworkStream stream = client.GetStream();

            if (!connectToPrivateChatQueue(LookingForSex))
            {
                Trace.WriteLine("Chatconnecting error at phase 1");
                return false;
            }

            try
            {
                while (true)
                {
                    Trace.WriteLine("PrivateChat: get matchport...");
                    string matchinfo = Utility.ReadFromNetworkStream(stream);
                    int chatport = int.Parse(matchinfo);

                    if (!curUser.HasOngoingChatSearch)
                    {
                        client.GetStream().Close();
                        client.Close();
                        return false;
                    }

                    Trace.WriteLine("PrivateChat: trying to verify...");
                    string verify = Utility.ReadFromNetworkStream(stream);
                    string[] verifyparams = verify.Split("|");

                    if (verifyparams[0] == "OK")
                    {
                        curUser.LastPrivateChatConversationId = verifyparams[1];
                        Trace.WriteLine("PrivateChat: verified! Conversationid: " + verifyparams[1]);
                        curUser.HasOngoingChat = true;

                        client = new TcpClient(PortManager.instance().Host, chatport);


                        EventArgs e = new EventArgs();
                        OnChatBegins(e);

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
            catch (Exception e)
            {
                Trace.WriteLine("Chatconnecting error at phase 2. Error message: " + e.Message);
            }

            return success;
        }

        private bool connectToPrivateChatQueue(GENDER LookingForSex)
        {
            bool success = false;
            try
            {
                NetworkStream stream = client.GetStream();
                string msg = curUser.Username + "|" + (int)curUser.AgeCategory + "|" + (int)curUser.Gender + "|" + (int)LookingForSex;
                Trace.WriteLine(msg);

                if (!curUser.HasOngoingChatSearch)
                {
                    client.GetStream().Close();
                    client.Close();
                    return false;
                }

                byte[] data = Encoding.Unicode.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                string responseData = Utility.ReadFromNetworkStream(stream);

                Trace.WriteLine("\nReceived from matchserver: " + responseData + "\n");

                success = (responseData == "OK");
            }
            catch (Exception e)
            {
                Trace.WriteLine("Error during privatechat connection: " + e.Message);
            }




            return success;
        }

        public void handeMessaging(string username, string message)
        {
            if (curUser.HasOngoingChat)
            {
                if (message == "")
                {
                    Trace.WriteLine("PrivateChat Warning: empty message");
                    return;
                }
                NetworkStream stream = client.GetStream();
                string toBeSend = username + "|" + message;
                Trace.WriteLine("Privatechat message sent: " + message);

                byte[] buffer = Encoding.Unicode.GetBytes(toBeSend);
                stream.Write(buffer);
            }
            else
            {
                Trace.WriteLine("PrivateChat Warning: no ongoing chat!");
            }

        }

        private void handleReading()
        {
            NetworkStream stream = client.GetStream();
            while (curUser.HasOngoingChat)
            {                
                try
                {
                    string raw_info = Utility.ReadFromNetworkStream(stream);


                    string sender = raw_info.Split("|", 2)[0];
                    string msg = raw_info.Split("|", 2)[1];
                    if (sender == "SERVER")
                    {
                        handleCommands(raw_info);
                    }
                    MessageArrivedEventArgs e = new MessageArrivedEventArgs();
                    e.MessageSender = sender;
                    e.Message = msg;
                    OnMessageArrived(e);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Smth shit happened in private chat reading, error message: " + e.Message);
                }
            }
        }

        
        private void handleCommands(string command)
        {
            string[] commandargs = command.Split("|");

            if (commandargs[1] == "!LEFT")
            {
                ShutDownChat();
            }
            else
            {
                Trace.WriteLine("Unknown command: " + command);
            }
        }

        /// <summary>
        /// when you want to leave the chat
        /// </summary>
        public void ExitChat()
        {
            if (client == null)
            {
                return;
            }
            if (curUser.HasOngoingChat || curUser.HasOngoingChatSearch)
            {
                //client.Connect(PortManager.instance().Host, PortManager.instance().Matchport);
                try
                {
                    NetworkStream stream = client.GetStream();
                    string msg = "!LEAVE|" + curUser.Username;
                    byte[] buffer = Encoding.Unicode.GetBytes(msg);
                    stream.Write(buffer);
                    Trace.WriteLine(msg + " sent!");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("exiting with error");
                }
                finally
                {
                    client.Close();

                    curUser.HasOngoingChat = false;

                    EventArgs e = new EventArgs();
                    //e.ConverastionId = curUser.LastPrivateChatConversationId;
                    OnChatEnded(e);
                }
            }

        }

        /// <summary>
        /// when the partner left the chat
        /// </summary>
        public void ShutDownChat()
        {
            if (client == null)
            {
                return;
            }
            if (curUser.HasOngoingChat || curUser.HasOngoingChatSearch)
            {
                client.Close();

                

                curUser.HasOngoingChat = false;

                EventArgs e = new EventArgs();
                //e.ConverastionId = curUser.LastPrivateChatConversationId;

                OnChatEnded(e);
                Trace.WriteLine("Shutting down chat");
            }
        }



        protected virtual void OnMessageArrived(MessageArrivedEventArgs e)
        {
            MessageArrived?.Invoke(this, e);
        }

        protected virtual void OnChatBegins(EventArgs e)
        {
            chatBegins?.Invoke(this, e);
        }

        protected virtual void OnChatEnded(EventArgs e)
        {
            chatEnded?.Invoke(this, e);
        }

    }
}
