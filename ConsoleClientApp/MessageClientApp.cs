using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessageClientApp
{
    delegate void AddMessage( string sNewMessage );
    public class MessageClientApp:IDisposable
    {
        // My Attributes
        private Socket m_sock;                      // Server connection
        private byte[] m_byBuff = new byte[256];    // Recieved data buffer
        private event AddMessage m_AddMessageEventHandler;              // Add Message Event handler for Form

        public string ServerIPAddressText;
        
        List<string> ClientRecievedData = new List<string>();
        public  MessageClientApp()
        {
            Console.WriteLine("Hello World!");

            m_AddMessageEventHandler += OnAddMessage;

            //这是在Console中的源码,不理他
            bool IsRunApp = false;
            while (IsRunApp)
            {
                Console.WriteLine("# 输入命令 按Enter完成输入");
                Console.WriteLine("# 命令Run:ip    - 连接ip服务器");
                Console.WriteLine("# 命令Send:text - 发送text消息");
                Console.WriteLine("# 命令History   - 打印接收到的消息历史");
                Console.WriteLine("# 命令Exit      - 退出");
                var commend = Console.ReadLine();

                //退出
                if (commend.Contains("Exit"))
                {
                    IsRunApp = false;
                    Connect_Closing();
                    continue;
                }

                //运行
                if (commend.Contains("Run"))
                {
                   var s= commend.Split(':');
                   ServerIPAddressText = s[1];
                   ConnectServer();
                    continue;
                }

                //发送消息
                if (commend.Contains("Send"))
                {
                    var s = commend.Split(':');
                    SendMessage(s[1]);
                    continue;
                }

                //打印历史接收消息
                if (commend.Contains("History"))
                {
                    foreach (var m in ClientRecievedData)
                        Console.WriteLine(m);
                }
            }
        }


        /// <summary>
        /// Connect button pressed. Attempt a connection to the server and 
        /// setup Recieved data callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConnectServer()
        {
            try
            {
                // Close the socket if it is still open
                if (m_sock != null && m_sock.Connected)
                {
                    m_sock.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(10);
                    m_sock.Close();
                }

                // Create the socket object
                m_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Define the Server address and port
                IPEndPoint epServer = new IPEndPoint(IPAddress.Parse(ServerIPAddressText), 399);

                // Connect to the server blocking method and setup callback for recieved data
                // m_sock.Connect( epServer );
                // SetupRecieveCallback( m_sock );

                // Connect to server non-Blocking method
                m_sock.Blocking = false;
                AsyncCallback onconnect = new AsyncCallback(OnConnect);
                m_sock.BeginConnect(epServer, onconnect, m_sock);
            }
            catch (Exception ex)
            {
                Console.WriteLine( ex.Message+ ",Server Connect failed!");
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we were sucessfull
            try
            {
                //sock.EndConnect( ar );
                if (sock.Connected)
                    SetupRecieveCallback(sock);
                else
                    Console.WriteLine("Unable to connect to remote machine,"+ "Connect Failed!");
            }
            catch (Exception ex)
            {
                Console.WriteLine( ex.Message, ",Unusual error during Connect!");
            }
        }

        /// <summary>
        /// Get the new data and send it out to all other connections. 
        /// Note: If not data was recieved the connection has probably 
        /// died.
        /// </summary>
        /// <param name="ar"></param>
        private void OnRecievedData(IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;

            // Check if we got any data
            try
            {
                int nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    // Wrote the data to the List
                    //string sRecieved = Encoding.ASCII.GetString(m_byBuff, 0, nBytesRec);//改成支持中文
                    string sRecieved = Encoding.UTF8.GetString(m_byBuff, 0, nBytesRec);

                    // WARNING : The following line is NOT thread safe. Invoke is
                    // m_lbRecievedData.Items.Add( sRecieved );
                    m_AddMessageEventHandler.Invoke(sRecieved);

                    // If the connection is still usable restablish the callback
                    SetupRecieveCallback(sock);
                }
                else
                {
                    // If no data was recieved then the connection is probably dead
                    Console.WriteLine("Client {0}, disconnected", sock.RemoteEndPoint);
                    sock.Shutdown(SocketShutdown.Both);
                    sock.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine( ex.Message, ",Unusual error druing Recieve!");
            }
        }

        private void OnAddMessage(string sMessage)
        {
            ClientRecievedData.Add(sMessage);
            Console.WriteLine("Accept:" + sMessage);
        }

        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        private void SetupRecieveCallback(Socket sock)
        {
            try
            {
                AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None, recieveData, sock);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ",Setup Recieve Callback failed!");
            }
        }

        /// <summary>
        /// Close the Socket connection bofore going home
        /// </summary>
        private void Connect_Closing()
        {
            if (m_sock != null && m_sock.Connected)
            {
                m_sock.Shutdown(SocketShutdown.Both);
                m_sock.Close();
                Console.WriteLine("连接关闭");
            }
        }

        /// <summary>
        /// Send the Message in the Message area. Only do this if we are connected
        /// </summary>
        public void SendMessage(string messageText)
        {
            // Check we are connected
            if (m_sock == null || !m_sock.Connected)
            {
                Console.WriteLine( "Must be connected to Send a message");
                return;
            }

            // Read the message from the text box and send it
            try
            {
                // Convert to byte array and send.
                //Byte[] byteDateLine = Encoding.ASCII.GetBytes(NeedSendMessageText.ToCharArray());//改成支持中文
                Byte[] byteDateLine = Encoding.UTF8.GetBytes(messageText.ToCharArray());
                m_sock.Send(byteDateLine, byteDateLine.Length, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message+ " Send Message Failed!");
            }
        }

        public void Dispose()
        {
            Connect_Closing();
            ClientRecievedData.Clear();
            ClientRecievedData = null;
        }
    }
}
