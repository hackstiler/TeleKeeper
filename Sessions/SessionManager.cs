using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleKeeper.Sessions
{
    internal class SessionManager
    {
        public static List<string> ActiveSessions = new List<string>();
        private static string DirectoryName = AppContext.BaseDirectory + "Sessions";
        private static string ConfigPath = $"{DirectoryName}/SavedSessions.txt";
        

        static SessionManager()
        {
            Console.WriteLine(ConfigPath);

            if (!File.Exists(ConfigPath))
            {
                Directory.CreateDirectory(DirectoryName);
                File.Create(ConfigPath).Dispose();
            }

            GetSavedSessions();
        }

        public static void AddSession(in string sessionId)
        {
            ActiveSessions.Add(sessionId);
            Save();
        }

        public static void DeleteSession(in string sessionId)
        {
            ActiveSessions.Remove(sessionId);
            Save();

            File.Delete($"Sessions/{sessionId}.session");
        }

        private static void GetSavedSessions()
        {
            var sr = new StreamReader(ConfigPath);
            ActiveSessions = sr.ReadToEnd().Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
            sr.Close();
        }

        static private void Save()
        {
            File.WriteAllText(ConfigPath, string.Empty);
            var sw = new StreamWriter(ConfigPath);

            foreach (var session in ActiveSessions)
            {
                sw.WriteLine(session);
            }

            sw.Close();
        }
    }
}
