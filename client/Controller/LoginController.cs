using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;

using knock.Model;

namespace knock.Controller
{
    class LoginController
    {
        private TcpClient client;
        private bool canAccess;

        public LoginController()
        {
            try
            {
                client = new TcpClient("localhost", 11000);
                canAccess = true;
            }
            catch
            {
                canAccess = false;
            }
        }

        public bool tryLogin(User user, string pwd)
        {
            try
            {
                bool success = false;

                
                string message = "LOGIN" + "|" + user.Username + "|" + MD5.HashData(        ASCIIEncoding.ASCII.GetBytes(pwd)             );
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
                Console.WriteLine("Received: {0}", responseData);






                return success;
            }
            catch(Exception e)
            {
                ///maybe we should also send an event here
                return false;
            }
        }


        public bool tryRegister(User user, string pwd)
        {
            try
            {
                bool success = false;


                string message = "LOGIN" + "|" + user.Username + "|" + MD5.HashData(ASCIIEncoding.ASCII.GetBytes(pwd));
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
                Console.WriteLine("Received: {0}", responseData);






                return success;
            }
            catch (Exception e)
            {
                ///maybe we should also send an event here
                return false;
            }
        }
    }
}
