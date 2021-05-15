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
        public event EventHandler lostConnection;

        public MiscController()
        {

        }

        public bool sendPrivateChatHistory(string conversationID, string history, string sender)
        {
            bool success = true;

            if(history == "")
            {
                return false;
            }

            Trace.WriteLine("history: " + history);
            

            lock(llock)
            {
                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);

                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return false;
                }

                NetworkStream stream = client.GetStream();

                try
                {
                    
                    byte[] idData = Encoding.Unicode.GetBytes("KNOCKNOCK|" + "CONVSAVE|" + conversationID + "|" + sender);
                    Trace.WriteLine("CONVID: " + conversationID);
                    stream.Write(idData);
                }
                catch(Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt send privatechat history id, error message: " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
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
                        byte[] data = Encoding.Unicode.GetBytes(/*"INSERTHISTORY|" +*/ "KNOCKNOCK|" + history);


                        //StreamWriter writer = new StreamWriter(stream);
                        Trace.WriteLine("Sending: " + history);
                        /*Thread t = new Thread( (_) => { stream.Write(data); });
                        t.Start();*/
                        
                        //writer.Write(data);
                        stream.Write(data);
                        Trace.WriteLine("sent: " + history);

                        string okmsg = Utility.ReadFromNetworkStream(stream);
                        success = (okmsg == "OK");
                    }
                }
                catch(Exception e)
                {
                    Trace.WriteLine("Error in sending history: error message " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return false;
                }                
            }
            return success;
            
        }

        public string[] GetUserHistoryIDs(string username)
        {
            lock (llock)
            {
                string[] results = new string[0];
                
                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return new string[0];
                }

                NetworkStream stream = client.GetStream();
                byte[] idData = Encoding.Unicode.GetBytes("KNOCKNOCK|" + "LISTLOAD|" + username);

                try
                {
                    stream.Write(idData);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt send username for history, error message: " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return new string[0];
                }

                try
                {
                    Thread.Sleep(100);
                    string attemptResult = Utility.ReadFromNetworkStream(stream);

                    if(attemptResult=="!")
                    {
                        return new string[0];
                    }

                    results = attemptResult.Split("!");

                    
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error in sending history: error message " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return new string[0];

                }

                return results;
            }
        }

        public string GetChatHistoryMessage(string convID)
        {
            Trace.WriteLine("her");
            lock (llock)
            {
                string result = "";

                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);
                    Trace.WriteLine("here");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return "";
                }

                Trace.WriteLine("heree");

                NetworkStream stream = client.GetStream();
                byte[] idData = Encoding.Unicode.GetBytes("KNOCKNOCK|" + "CONVLOAD|" + convID);

                try
                {
                    stream.Write(idData);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt send convID for history, error message: " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return "";
                }

                Trace.WriteLine("hereee");

                try
                {
                    Thread.Sleep(100);
                    result = Utility.ReadFromNetworkStream(stream);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error in sending history: error message " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return "";
                }

                return result;
            }
        }

        protected virtual void OnLostConnection(EventArgs e)
        {
            lostConnection?.Invoke(this, e);
        }

        public bool SendBlockRequest(string blocker, string blocked)
        {
            bool success = false;

            lock (llock)
            {


                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);
                    Trace.WriteLine("here");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return success;
                }

                NetworkStream stream = client.GetStream();
                string msg = "BLOCK|" + blocker + "|" + blocked;
                byte[] data = Encoding.Unicode.GetBytes("KNOCKNOCK|" + msg);

                try
                {
                    stream.Write(data);
                    Thread.Sleep(100);
                    string result = Utility.ReadFromNetworkStream(stream);

                    success = (result == "OK");

                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error in sending block request: error message " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return false;
                }
            }
            return success;
        }

        public bool SendFriendRequest(string friender, string friended)
        {
            bool success = false;

            lock (llock)
            {


                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);
                    Trace.WriteLine("here");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return success;
                }

                NetworkStream stream = client.GetStream();
                string msg = "FRIEND|" + friender + "|" + friended;
                byte[] data = Encoding.Unicode.GetBytes("KNOCKNOCK|" + msg);

                try
                {
                    stream.Write(data);
                    Thread.Sleep(100);
                    string result = Utility.ReadFromNetworkStream(stream);

                    success = (result == "OK");

                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error in sending block request: error message " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return false;
                }
            }
            return success;
        }

        public string GetFriendLists(string username)
        {
            string message = "FRIENDLOAD|" + username;
            string reply = "!!";

            lock (llock)
            {


                try
                {
                    client = new TcpClient(PortManager.instance().Host, PortManager.instance().Miscport);
                    Trace.WriteLine("here");
                }
                catch (Exception e)
                {
                    Trace.WriteLine("MiscController error: couldnt connect to server, error message: " + e.Message);
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return "!!";
                }

                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.Unicode.GetBytes("KNOCKNOCK|" + message);

                try
                {
                    stream.Write(data);
                    Thread.Sleep(100);
                    reply = Utility.ReadFromNetworkStream(stream);
                    Trace.WriteLine("The friendlist request's result: " + reply);
                }
                catch (Exception e)
                {
                    Trace.WriteLine("Error in sending block request: error message " + e.Message);
                    client.Close();
                    EventArgs er = new EventArgs();
                    OnLostConnection(er);
                    return "!!";
                }
            }
            return reply;
        }
    }


}
