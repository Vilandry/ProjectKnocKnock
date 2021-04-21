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
    class LoginController
    {
        private TcpClient client;
        private bool canAccess;
        private int port;

        public LoginController()
        {
            port = PortManager.instance().Loginport;
            client = new TcpClient();
        }

        public bool Connected()
        {
            if(client == null)
            {
                ///then its kinda waow, should do smth shit here
                Trace.WriteLine("\n\nclient was null wtf!\n\n");
                return false;
            }
            return client.Connected;
        }

        public bool Reconnect()
        {
            if(!client.Connected)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(PortManager.instance().Host, port);
                }
                catch(Exception e)
                {
                    ///to be decided
                    Trace.WriteLine(e.Message+"\nso couldnt really connect in reconnect()!\n\n");
                }
            }

            return client.Connected;
        }

        public bool ForcedReconnect()
        {
            try
            {
                client = new TcpClient();
                client.Connect(PortManager.instance().Host, port);
                Trace.WriteLine("\n\n" + client.Connected + "\n\n");
            }
            catch (Exception e)
            {
                ///to be decided
                Trace.WriteLine("\n\n" + "Error during reconnect attempt, error msg: " + e.Message + "\n\n");
            }
            return client.Connected;
        }

        public void DropConnection()
        {
            client.Close();
        }

        public int tryLogin(User user, string pwd)
        {
            try
            {
                ForcedReconnect();
                int success = 0;

                
                string message = "LOGIN" + "|" + user.Username + "|" + Utility.CreateMD5(pwd);

                /// Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                NetworkStream stream = client.GetStream();

                /// Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                data = new Byte[1024];

                /// String to store the response ASCII representation.
                String responseData = String.Empty;

                /// Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Trace.WriteLine("Received: " + responseData);
                success = responseData.Split("|")[0] == "OK" ? 1 : 0;

                if(success == 1)
                {
                    user.AgeCategory = (AGECATEGORY)int.Parse(responseData.Split("|")[1]);
                    user.Gender = (GENDER)int.Parse(responseData.Split("|")[2]);
                }

                stream.Close();

                return success;
            }
            catch(Exception e)
            {
                ///maybe we should also send an event here
                Trace.WriteLine("\n\n" + "Error during login attempt, error msg: " + e.Message + "\n\n");
                return -1;
            }
            finally
            {
                client.Close();
            }
        }


        public int tryRegister(User user, string pwd)
        {
            try
            {
                ForcedReconnect();
                int success = 0;


                string message = "REGISTER" + "|" + user.Username + "|" + Utility.CreateMD5(pwd) + "|" + (int)user.AgeCategory + "|" + (int)user.Gender;
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);


                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
                data = new Byte[256];


                String responseData = String.Empty;

                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                Trace.WriteLine("Received: " + responseData);
                success = responseData.Split("|")[0] == "OK" ? 1 : 0;

                if (success == 1)
                {
                    user.AgeCategory = (AGECATEGORY)int.Parse(responseData.Split("|")[1]);
                    user.Gender = (GENDER)int.Parse(responseData.Split("|")[2]);
                }

                stream.Close();


                return success;
            }
            catch (Exception e)
            {
                ///maybe we should also send an event here
                Trace.WriteLine("\n\n" + "Error during login attempt, error msg: " + e.Message + "\n\n");
                return -1;
            }
            finally
            {
                
                client.Close();
            }
        }
    }
}
