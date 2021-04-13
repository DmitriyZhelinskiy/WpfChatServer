using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfChat.Entities;
using WpfChat_server.Core.Workers;

namespace WpfChat_server.Core
{
    public class Application
    {
        public List<User> Users = new List<User>();
        TcpListener Listener;
        public Application()
        {
            FileWorker.CreateAllDirectories();
            Listener = new TcpListener(IPAddress.Any, 5400);
            Listener.Start();
        }
        public void Start()
        {
            Console.WriteLine("Сервер онлайн");
            try
            {
                while (true)
                {
                    TcpClient Client = Listener.AcceptTcpClient();
                    ThreadWorker client = new ThreadWorker(Client, ref Users);

                    Thread clientThread = new Thread(new ThreadStart(client.Starting));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (Listener != null)
                    Listener.Stop();
            }
        }
    }
}
