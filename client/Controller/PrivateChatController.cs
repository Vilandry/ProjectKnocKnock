﻿using System;
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
                Trace.WriteLine("smth shit happened in the matchmaking connection");
                return false;
            }



            int buffersize = 1024;
            byte[] data = new byte[1024];
            stream.Read(data, 0, buffersize);

            string matchinfo = System.Text.Encoding.UTF8.GetString(data);
            int chatport = int.Parse(matchinfo);

            try
            {
                client.Connect(PortManager.instance().Host, chatport);

                handleReading();

            }
            catch(Exception e)
            {
                Trace.WriteLine("Chatconnecting shit happened. Error message: " + e.Message);
            }

            return success;
        }

        private bool connectToPrivateChatQueue(User curUser, GENDER LookingForSex)
        {
            bool success = false;


            NetworkStream stream = client.GetStream();

            string msg = curUser.Username + "|" + (int)curUser.AgeCategory + "|" + (int)curUser.Gender + "|" + (int)LookingForSex;

            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);            
            stream.Write(data, 0, data.Length);

            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data);
            Trace.WriteLine("Received from matchserver: " + responseData);

            success = (responseData == "OK");

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
                string toBeSend = Utility.MessageFormatter(username, message);

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
                    byte[] buffer = new byte[1024];
                    int buffersize = 1024;

                    stream.Read(buffer, 0, 1024);

                    string raw_info = System.Text.Encoding.UTF8.GetString(buffer);

                    string sender = raw_info.Split("|", 2)[0];
                    string msg = raw_info.Split("|", 2)[1];
                    MessageArrivedEventArgs e = new MessageArrivedEventArgs();
                    e.MessageSender = sender;
                    e.Message = msg;
                    OnMessageArrived(e);
                }
                catch(Exception e)
                {
                    Trace.WriteLine("Smth shit happened in private chat, event message: " + e.Message);
                }
            }
        }

        protected virtual void OnMessageArrived(MessageArrivedEventArgs e)
        {
            MessageArrived?.Invoke(this, e);
        }

        public void ExitChat()
        {
            if(ongoingChat)
            {
                string msg = "!LEAVE";
                ongoingChat = false;
                client.Close();
            }

        }



    }
}
