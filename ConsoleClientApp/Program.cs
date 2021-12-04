using System;

namespace ConsoleClientApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var app = new MessageClientApp.MessageClientApp() { ServerIPAddressText="192.168.0.108"};
            app.ConnectServer();
            while (true)
            {
                Console.WriteLine("Not Exit (y/n) y:");
                var key = Console.ReadLine();
                if (key == "n")
                {
                    return;
                }
                else
                {
                    app.SendMessage(key);
                }
            }
        }
    }
}
