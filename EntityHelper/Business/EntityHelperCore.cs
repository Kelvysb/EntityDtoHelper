using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EntityHelper.Models;
using Newtonsoft.Json;

namespace EntityHelper.Business
{
    public class EntityHelperCore
    {

        private const string workFolder = "EntityHelper";
        private const string connectionsFile = "connections.json";        
        private string workDirectory;
        public List<Connection> Connections { get; private set; }


        public EntityHelperCore()
        {
            this.workDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), workFolder);
            LoadConnections();
        }

        public Connection GetConnection(string name)
        {
            return Connections 
                   .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public List<string> GetConnections(string name)
        {
            return Connections 
                   .Select(conn => $"{conn.Name} - {conn.Server} - {conn.DataBase}")
                   .ToList();
        }

        public void RemoveConnection(string name)
        {
            Connections
                .RemoveAll(conn => conn.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            SaveConnections();
        }

        public void AddConnection(Connection connection)
        {
            Connection current = Connections
                    .FirstOrDefault(conn => conn.Name.Equals(connection.Name, StringComparison.InvariantCultureIgnoreCase));

            if(current == null)
            {
                Connections.Add(connection);
            }else
            {
                current.Type = connection.Type;
                current.Server = connection.Server;
                current.DataBase = connection.DataBase;
                current.User = connection.User;
                current.Password = connection.Password;
                current.ConnectionString = connection.ConnectionString;
            }
            SaveConnections();
        }

        public bool TestConnection(Connection connection)
        {
            bool result = false;

            

            return result;
        }

        private void SaveConnections()
        {   
            using(StreamWriter file = new StreamWriter(Path.Combine(workDirectory, connectionsFile), false))
            {
                string connData = Newtonsoft.Json.JsonConvert.SerializeObject(Connections);
                file.Write(connData);
                file.Close();
            }        
        }

        private void LoadConnections()
        {
            if(File.Exists(Path.Combine(workDirectory, connectionsFile)))
            {
                using(StreamReader file = new StreamReader(Path.Combine(workDirectory, connectionsFile)))
                {
                    string connData = file.ReadToEnd();
                    Connections = JsonConvert.DeserializeObject<List<Connection>>(connData);
                    file.Close();
                }        
            }else
            {
                Connections = new List<Connection>();
                SaveConnections();
            }
        }
    }
}