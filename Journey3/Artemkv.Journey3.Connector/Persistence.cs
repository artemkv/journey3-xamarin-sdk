using Newtonsoft.Json;
using System;
using System.IO;

namespace Artemkv.Journey3.Connector
{
    internal class Persistence : IPersistence
    {
        private static readonly string SESSION_FILE_NAME = "journey.session.json";

        public Session LoadLastSession()
        {
            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SESSION_FILE_NAME);

            if (!new FileInfo(fileName).Exists)
            {
                return null;
            }

            string json = File.ReadAllText(fileName);
            Session session = Session.FromJson(json, new Timeline(), new IdGenerator());
            return session;
        }

        public void SaveSession(Session session)
        {
            string fileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SESSION_FILE_NAME);

            string json = JsonConvert.SerializeObject(session);
            File.WriteAllText(fileName, json);
        }
    }
}
