using ChatServer_1.Core.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfChat.Entities;
using WpfChat.Workers;

namespace WpfChat_server.Core.Workers
{
    public class ThreadWorker
    {
        TcpClient Client;
        NetworkStream Stream;
        User user = null;
        public bool Logining { get; set; } = false;
        public List<User> Users;
        string data = "";
        public ThreadWorker(TcpClient Client, ref List<User> Users)
        {
            this.Client = Client;
            this.Users = Users;
            Stream = Client.GetStream();
            Console.WriteLine($"Подключен:{Client.Client.RemoteEndPoint}");
        }
        public void Starting()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    data = PacketRecipient.GetJsonData(Stream);
                    Console.WriteLine(data);

                    //  Регистрация
                    if (data.Contains("REGISTER::"))
                    {
                        data = data.Remove(0, "REGISTER::".Length);
                        user = JsonWorker.JsonToUser(data);
                        if (FileWorker.ExistsUserFile(user.Login))
                        {
                            user.SessionKey = "ERROR:EXISTS::USER";
                        }
                        else
                        {
                            FileWorker.WriteUserFile(user.Login, JsonWorker.UserToJson(user));
                            user.SessionKey = Guid.NewGuid().ToString();
                        }
                        PacketSender.SendJsonString(Stream, JsonWorker.UserToJson(user));
                    }

                    //  Авторизация
                    if (data.Contains("LOGINING::"))
                    {
                        data = data.Remove(0, "LOGINING::".Length);
                        Console.WriteLine(data);
                        user = JsonWorker.JsonToUser(data);
                        if (FileWorker.ExistsUserFile(user.Login))
                        {
                            if (user.Password == JsonWorker.JsonToUser(FileWorker.ReadUserFile(user.Login)).Password)
                            {
                                user.SessionKey = Guid.NewGuid().ToString();
                                user.Stream = Stream;
                                Users.Add(user);
                            }
                            else
                            {
                                user.SessionKey = "ERROR::BAD::LOGIN";
                            }
                        }
                        else
                        {
                            user.SessionKey = "ERROR::BAD::LOGIN";
                        }
                        PacketSender.SendJsonString(Stream, JsonWorker.UserToJson(user));
                    }

                    //  При входе в аккаунт - передаём все необходимые данные, список подключенных юзеров.
                    if (data.Contains("STARTAPP::"))
                    {
                        foreach (var item in Users)
                        {
                            PacketSender.SendJsonString(item.Stream, "USERS_LIST::" + JsonWorker.UsersListToJson(Users));
                        }
                    }
                    if (data.Contains("MESSAGE::"))
                    {
                        data = data.Remove(0, "MESSAGE::".Length);
                        foreach (var item in Users)
                        {
                            PacketSender.SendJsonString(item.Stream, $"MESSAGE::{user.Login}: {data}");
                        }
                        File.AppendAllText("MessageLog.txt", $"[{DateTime.Now}] {user.Login}: {data}");
                    }
                    data = "";
                }
            }
            catch (Exception exeption)
            {
                Users.Remove(user);
                Console.WriteLine(exeption.Message);
            }
            finally
            {
                if (Stream != null)
                    Stream.Close();
                if (Client != null)
                    Client.Close();
            }
        }
        
    }
}

////  Если нет ключа сессии - проверяем, есть ли в базе такой юзверь. Проверяем логин и пароль - выдаём ключ сессии и отправляем Json строку обратно
//if (JsonWorker.JsonToUser(data).SessionKey == null)
//{
//    //  Если юзверь не авторизован
//    if (user == null)
//    {
//        //  Сериализуем полученные данные юзера для дальнейшего сравнения
//        user = JsonWorker.JsonToUser(data);
//        //  Если найден файл с юзером
//        if (FileWorker.ExistsUserFile(user.Login))
//        {
//            //  Проверяем правильность пароля
//            if (user.Password == JsonWorker.JsonToUser(FileWorker.ReadUserFile(user.Login)).Password)
//            {
//                //  Если пароль верный - даём юзверю ID сессии
//                user.SessionKey = Guid.NewGuid().ToString();
//            }
//            PacketSender.SendJsonString(Stream, JsonWorker.UserToJson(user));
//        }
//    }
//}