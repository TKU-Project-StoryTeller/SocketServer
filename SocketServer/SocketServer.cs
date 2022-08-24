//SocketServer
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
namespace SocketPratice
{
    public class SocketServer
    {
        public Socket Server { get; set; }
        public Socket Client { get; set; }
        public IPEndPoint EndPoint { get; set; }
        Dictionary<string, Socket> dict = new Dictionary<string, Socket>();
        private int ClientNum { get; set; }
        public event Action<string>? OnMessageReceive;
        public bool IsReceive { get; set; }
        //public int ErrorCount { get; set; }
        public int FileNum { get; set; }
        public int ConnectedNum { get; set; } //計算已連上的client端數量
        public int MaxErrorCount { get; set; }
        public string? receiveMessage;
        private string? sendMessage;

        //本機存放骨架txt檔案路徑
        //private string SendFilePath = @"C:\Users\alumi\OneDrive\桌面\專題\keypoints\";
        private string SendFilePath = @"keypoints/";

        public SocketServer()
        {
            EndPoint = new IPEndPoint(IPAddress.Any, 8000);
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientNum = 1;
            FileNum = 0; // 沒用StartCreatTxt為0 ，有用為-1
            ConnectedNum = 0;
        }
        public void Listen()
        {
            try
            {
                Server.Bind(EndPoint);
                Server.Listen(ClientNum);
                Console.WriteLine("Listening....");
                //StartConnect();
                //Server.Accept();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message);
            }

        }
        public void StartConnect()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (ConnectedNum < ClientNum)
                        {
                            Client = Server.Accept();//等到連線成功後才會往下執行
                            ConnectedNum++;
                            Console.WriteLine(("Client IP = " + Client.RemoteEndPoint.ToString()) + " Connect Succese!");
                            dict.Add(Client.RemoteEndPoint.ToString(), Client);
                        }

                        //連線成功後，若是不想再接受其他連線，可以關閉serverSocket
                        //serverSocket.Close();
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }

            });
        }
        public bool isConnected()
        {
            if (Client != null) return true;
            else return false;
        }
        //停止連線
        public void StopConnect()
        {
            try
            {
                Client.Close();
            }
            catch (Exception)
            {

            }
        }
        public string GetData(string filename)
        {
            //本機檔案位置
            string path = SendFilePath + filename + "point_" + FileNum + ".txt";
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                FileNum++; //沒有用StartCreateTxt()時使用
                return data;
            }
            else
            {
                return "NoData";
            }
        }
        //寄送訊息
        public void Send(string Message)
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;
            else
            {
                sendMessage = Message;
                Task.Run(() =>
                {
                    try
                    {
                        if (Client.Connected)//若成功連線才傳遞資料
                        {
                            //將資料進行編碼並轉為Byte後傳遞
                            //Client.Send(Encoding.ASCII.GetBytes("Server: " + sendMessage), SocketFlags.None);

                            var ConvertBuffer = Encoding.UTF8.GetBytes(sendMessage);
                            var SendBuffer = new byte[1024];
                            ConvertBuffer.CopyTo(SendBuffer, 0);
                            foreach (Socket s in dict.Values)
                            {
                                s.Send(SendBuffer, SocketFlags.None);
                            }

                        }
                        //由於資料傳遞速度很快，沒必要使用Thread
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                });
            }
        }
        public void StartReceive()
        {
            Task.Run(() =>
            {
                IsReceive = true;

                while (IsReceive)
                {

                    try
                    {
                        if (isConnected())
                        {
                            if (dict.Count > 0)
                            {
                                foreach (Socket s in dict.Values)
                                {
                                    byte[] Buffer = new byte[1024];//用來儲存傳遞過來的資料
                                                                   //Console.WriteLine(isConnected());
                                    if (s != null)
                                    {
                                        int len = s.Receive(Buffer);
                                        //Console.WriteLine(len);
                                        if (len > 0)
                                        {
                                            receiveMessage = Encoding.UTF8.GetString(Buffer, 0, len);
                                            OnMessageReceive?.Invoke("Client: " + receiveMessage);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine(ex.Message);
                    }
                }
            });

        }
        private async void ReceiveFile(byte[] Buffer, int datasize)
        {
            await Task.Run(() =>
            {
                //byte[] Buffer = new byte[1024];
                //int datasize = Client.Receive(Buffer);
                string path = @"C:\SocketTest\point_" + FileNum + ".json";
                using (var output = File.Create(path))
                {
                    output.Write(Buffer, 0, datasize);
                    output.Flush();
                    FileNum++;
                    Console.WriteLine("Create File:\"" + path + "\" Success!");
                }

                // read the file in chunks of 1KB
            });

        }
        private void SendMessage()
        {
            try
            {
                if (Client.Connected == true)//若成功連線才傳遞資料
                {
                    //將資料進行編碼並轉為Byte後傳遞
                    Client.Send(Encoding.UTF8.GetBytes(sendMessage));
                }
            }
            catch (Exception)
            {

            }
        }
    }
}