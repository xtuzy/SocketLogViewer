
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = MessageServerApp.MessageServerApp.CreatServerAppProgram();
            Console.Title = app.MyIp.ToString();
            app.AcceptedMessageEvent += (s) =>
            {
                Console.WriteLine(s);
            };
            while (true)
            {
                Console.WriteLine("Not Exit (y/n) y:");
                var key = Console.ReadLine();
                if (key == "n")
                {
                    return;
                }
            }
        }

        private static void DoSomething(System.Collections.Generic.KeyValuePair<TcpClient, byte[]> data)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data.Value));
        }
    }
}
