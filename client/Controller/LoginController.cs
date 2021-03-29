using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

using knock.Model;

namespace knock.Controller
{
    class LoginController
    {
        private TcpClient client;
        private bool canAccess;
        private string hostname;
        private int port;

        public LoginController()
        {
            client = new TcpClient();
            /*try
            {
                hostname = "localhost";
                port = 11000;
                client = new TcpClient(hostname, port);
                canAccess = true;
                
            }
            catch
            {
                canAccess = false;
            }*/
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
                    client.Connect(hostname, port);
                }
                catch(Exception e)
                {
                    ///to be decided
                    Trace.WriteLine("\n\ncouldnt really connect!\n\n");
                }
            }
            return client.Connected;
        }

        public bool tryLogin(User user, string pwd)
        {
            try
            {
                Reconnect();
                bool success = false;

                
                string message = "LOGIN" + "|" + user.Username + "|" + Utility.CreateMD5(pwd);
                string server = "localhost";
                /// Create a TcpClient.
                /// Note, for this client to work you need to have a TcpServer
                /// connected to the same address as specified by the server, port
                /// combination.
                Int32 port = 13000;

                /// Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                /// Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                /// Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                /// Receive the TcpServer.response.

                /// Buffer to store the response bytes.
                data = new Byte[256];

                /// String to store the response ASCII representation.
                String responseData = String.Empty;

                /// Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                success = responseData == "OK";
                Trace.WriteLine("Received: {0}", responseData);






                return success;
            }
            catch(Exception e)
            {
                ///maybe we should also send an event here
                return false;
            }
            finally
            {
                client.Close();
            }
        }


        public bool tryRegister(User user, string pwd)
        {
            try
            {
                Reconnect();
                bool success = false;


                string message = "REGISTER" + "|" + user.Username + "|" + Utility.CreateMD5(pwd) + "|" + (int)user.AgeCategory + "|" + (int)user.Gender;
                string server = "localhost";
                /// Create a TcpClient.
                /// Note, for this client to work you need to have a TcpServer
                /// connected to the same address as specified by the server, port
                /// combination.
                Int32 port = 13000;

                /// Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                /// Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                /// Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                /// Receive the TcpServer.response.

                /// Buffer to store the response bytes.
                data = new Byte[256];

                /// String to store the response ASCII representation.
                String responseData = String.Empty;

                /// Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                success = responseData == "OK";
                Trace.WriteLine("Received: {0}", responseData);






                return success;
            }
            catch (Exception e)
            {
                ///maybe we should also send an event here
                return false;
            }
            finally
            {
                client.Close();
            }
        }
    }
}
