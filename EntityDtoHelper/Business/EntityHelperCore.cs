using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BDataBaseStandard;
using EntityHelper.Models;
using EntityHelper.Repositories;
using EntityHelper.Repositories.Interfaces;
using Newtonsoft.Json;

namespace EntityHelper.Business
{
    public class EntityHelperCore
    {

        private const string workFolder = "EntityHelper";
        private const string connectionsFile = "connections.json";
        private string workDirectory;
        public List<Connection> Connections { get; private set; }
        public string CurrentFolder { get; set; }


        public EntityHelperCore()
        {
            this.workDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), workFolder);
            this.CurrentFolder = Environment.CurrentDirectory;
            LoadConnections();
        }

        public List<string> Run(ExecConfig execConfig)
        {
            List<string> result = new List<string>();

            using (IEntityHelperRepository repository = new EntityHelperRepository(execConfig.connection))
            {

                List<string> tables = new List<string>();
                if (!execConfig.AllTables)
                {
                    tables.Add(execConfig.Table);
                }
                else
                {
                    tables = repository.GetTables(execConfig.IncludeViews);
                }

                foreach (string table in tables)
                {
                    clsTableInfo info = repository.GetTableInfo(table);
                    result.Add(GenerateDto(execConfig, info));
                    result.Add(GenerateEntity(execConfig, info));
                }

            }

            return result;
        }

        private string GenerateEntity(ExecConfig execConfig, clsTableInfo info)
        {
            string className = info.Name.Substring(0, 1).ToUpper() + info.Name.Substring(1);
            string filePath = Path.Combine(Path.GetFullPath(execConfig.EntityFolder), className + ".cs");


            string content = $@"{GetUsing(info, execConfig.UseEntityFramework)}

namespace {execConfig.EntityNamespace}
{{
    public class {className}
    {{
{GetProperties(info, execConfig.UseEntityFramework)}
    }}
}}";
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            StreamWriter file = new StreamWriter(filePath);
            file.WriteLine(content);
            file.Close();
            file.Dispose();

            return filePath;
        }

           private string GenerateDto(ExecConfig execConfig, clsTableInfo info)
        {
            string className = info.Name.Substring(0, 1).ToUpper() + info.Name.Substring(1) + "Dto";
            string filePath = Path.Combine(Path.GetFullPath(execConfig.DtoFolder), className + ".cs");


            string content = $@"{GetUsing(info, false)}

namespace {execConfig.DtoNamespace}
{{
    public class {className}
    {{
        {GetProperties(info, false)}
    }}
}}";
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            StreamWriter file = new StreamWriter(filePath);
            file.WriteLine(content);
            file.Close();
            file.Dispose();

            return filePath;
        }

        private string GetProperties(clsTableInfo info, bool useEF)
        {
            string result = "";
            foreach (clsColumnInfo column in info.Columns)
            {
                string columnName = column.Name.Substring(0, 1).ToUpper() + column.Name.Substring(1);
                string columnType = "";

                switch (column.Type)
                {
                    case enm_ColumnType.Bool:
                        columnType = "bool";
                        break;
                    case enm_ColumnType.DateTime:
                        columnType = "DateTime";
                        break;
                    case enm_ColumnType.File:
                        columnType = "Object";
                        break;
                    case enm_ColumnType.Int:
                        columnType = "int";
                        break;
                    case enm_ColumnType.Number:
                        columnType = "double";
                        break;
                    case enm_ColumnType.Text:
                        columnType = "string";
                        break;
                    case enm_ColumnType.Xml:
                        columnType = "XDocument";
                        break;
                    default:
                        columnType = "Object";
                        break;
                }

                if (column.PrimaryKey && useEF)
                {
                    result += $"        [Key]\n";
                }
                result += $"        public {columnType} {columnName} {{ get; set; }}\n";
            }
            return result;
        }

        private object GetUsing(clsTableInfo info, bool useEF)
        {
            string result = "";

            if (info.Columns.Any(column => column.Type == enm_ColumnType.DateTime))
            {
                result += "using using System;";
            }

            if (info.Columns.Any(column => column.Type == enm_ColumnType.Xml))
            {
                result += "using System.Xml.Linq;";
            }

            if (useEF)
            {
                result += "using System.ComponentModel.DataAnnotations;";
            }

            return result;
        }

        public Connection GetConnection(string name)
        {
            return Connections
                   .FirstOrDefault(conn => conn.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public List<string> GetConnections()
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

            if (current == null)
            {
                Connections.Add(connection);
            }
            else
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
            IEntityHelperRepository repository = new EntityHelperRepository();
            try
            {
                repository.Connect(connection);
                repository.Dispose();
                result = true;
            }
            catch (System.Exception)
            {
                result = false;
            }
            return result;
        }

        private void SaveConnections()
        {
            using (StreamWriter file = new StreamWriter(Path.Combine(workDirectory, connectionsFile), false))
            {
                string connData = Newtonsoft.Json.JsonConvert.SerializeObject(Connections);
                file.Write(connData);
                file.Close();
            }
        }

        private void LoadConnections()
        {
            if (!Directory.Exists(workDirectory))
            {
                Directory.CreateDirectory(workDirectory);
            }
            if (File.Exists(Path.Combine(workDirectory, connectionsFile)))
            {
                using (StreamReader file = new StreamReader(Path.Combine(workDirectory, connectionsFile)))
                {
                    string connData = file.ReadToEnd();
                    Connections = JsonConvert.DeserializeObject<List<Connection>>(connData);
                    file.Close();
                }
            }
            else
            {
                Connections = new List<Connection>();
                SaveConnections();
            }
        }
    }
}