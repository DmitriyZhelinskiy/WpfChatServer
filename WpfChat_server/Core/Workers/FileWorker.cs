using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfChat_server.Core.Workers
{
    public static class FileWorker
    {
        private static string UsersDir = "Users";
        public static void CreateAllDirectories()
        {
            if (!Directory.Exists(UsersDir))
            {
                Directory.CreateDirectory(UsersDir);
            }
        }
        public static string ReadUserFile(string fileName)
        {
            return File.ReadAllText($"{UsersDir}/{fileName}.json");
        }
        public static void WriteUserFile(string fileName, string data)
        {
            File.WriteAllText($"{UsersDir}/{fileName}.json", data);
        }
        public static bool ExistsUserFile(string fileName)
        {
            return File.Exists($"{UsersDir}/{fileName}.json");
        }
    }
}
