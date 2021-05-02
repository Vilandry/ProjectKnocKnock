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
        private volatile User curUser;

        public event EventHandler<MessageArrivedEventArgs> MessageArrived;
        public event EventHandler chatEnded;
        public event EventHandler chatBegins;
        public event EventHandler lostConnection;


        public PrivateChatController(User cu)
        {
            curUser = cu;
        }

        public bool HandlePrivateChatting(GENDER LookingForSex)
        {           
            bool success = false;
            try
            {
                client = new TcpClient(PortManager.instance().Host, PortManager.instance().Matchport);                
            }
            catch(Exception e)
            {
                Trace.WriteLine("error in connection to matchmaking shit err msg: " + e.Message);
                EventArgs er = new EventArgs();
                OnLostConnection(er);
                return false;
            }
            

            if (!connectToPrivateChatQueue(LookingForSex))
            {
                Trace.WriteLine("Chatconnecting error at phase 1");
                ShutDownChat();
                return false;
            }

            try
            {
                NetworkStream stream = client.GetStream();
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
                    string[] verifyparams = verify.Split("|",2);

                    if (verifyparams[0] == "OK")
                    {
                        curUser.LastPrivateChatHistory = "";
                        curUser.LastPrivateChatConversationId = verifyparams[1];
                        Trace.WriteLine("PrivateChat: verified! Conversationid: " + verifyparams[1]);

                        if (curUser.Username == curUser.LastPrivateChatConversationId.Split("|")[1])
                        {
                            curUser.LastPrivateChatUsername = verifyparams[1].Split("|")[2];
                        }
                        else
                        {
                            curUser.LastPrivateChatUsername = verifyparams[1].Split("|")[1];
                        }
                        
                        curUser.HasOngoingChat = true;

                        client = new TcpClient(PortManager.instance().Host, chatport);


                        EventArgs e = new EventArgs();
                        OnChatBegins(e);

                        Thread t = new Thread(handleReading);
                        t.Start();

                        MessageArrivedEventArgs startMsg = new MessageArrivedEventArgs();
                        startMsg.MessageSender = "SERVER";
                        startMsg.Message = "!JOINED|" + curUser.LastPrivateChatUsername;
                        OnMessageArrived(startMsg);
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
                if(curUser.HasOngoingChatSearch)
                {
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                }
                ShutDownChat();
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

                Trace.WriteLine("\n" + responseData + "\nReceived from matchserver: " + responseData + "\n");

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
                try
                {
                    NetworkStream stream = client.GetStream();
                    string toBeSend = username + "|" + message;
                    Trace.WriteLine("Privatechat message sent: " + message);

                    byte[] buffer = Encoding.Unicode.GetBytes(toBeSend);
                    stream.Write(buffer);
                }
                catch(Exception e)
                {
                    Trace.WriteLine("dead server prob tbh exception message: " + e.Message);
                    ShutDownChat();
                }
                
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

                    string historymessage = Utility.MessageFormatter(sender, msg) + "\n";

                    curUser.LastPrivateChatHistory = curUser.LastPrivateChatHistory  + historymessage;

                    e.MessageSender = sender;
                    e.Message = msg;
                    OnMessageArrived(e);
                }
                catch (Exception e)
                {
                    Thread.Sleep(100);
                    if(!curUser.HasOngoingChat)
                    {
                        Trace.WriteLine("Looks like chat ended by occasion error message: " + e.Message);
                    }
                    else
                    {
                        Trace.WriteLine("Smth shit happened in private chat reading, error message: " + e.Message);
                        EventArgs eventarg = new EventArgs();

                        OnLostConnection(eventarg);
                    }

                }
            }
        }

        
        private void handleCommands(string command)
        {
            string[] commandargs = command.Split("|");

            if (commandargs[1] == "!LEFT")
            {
                ExitChat();
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
                    Trace.WriteLine("exiting with error on exitchat error msg: " + e.Message);
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
                curUser.HasOngoingChatSearch = false;

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

        protected virtual void OnLostConnection(EventArgs e)
        {
            lostConnection?.Invoke(this, e);
            ShutDownChat();
        }

    }
}
