//Program
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
namespace SocketPratice
{
    class Program
    {
        public static SocketServer Server { get; set; }
        //檔案夾名稱 => 詳細路徑在serversocket修改
        public static string filename = @"Data/";
        static void Main(string[] args)
        {
            Server = new SocketServer();
            Server.OnMessageReceive += (Message) =>
            {
                Console.WriteLine(Message);
            };
            Server.Listen();
            Server.StartConnect();
            //Server.StartCreateTxt(filename);
            while (true)
            {
                if (Server.isConnected())
                {
                    var SendMessage = Server.GetData(filename);
                    //Console.WriteLine(SendMessage);
                    Server.Send(SendMessage);
                }
            }
        }
    }
}