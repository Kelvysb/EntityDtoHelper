using System;
using System.Collections.Generic;
using System.Reflection;
using ArgsSplitter.business;
using EntityHelper.Business;
using EntityHelper.Models;
using System.IO;
using System.Linq;

namespace EntityHelper
{
    class Program
    {
        private static ASplitter menu;
        private static EntityHelperCore business;

        static void Main(string[] args)
        {
            bool exit = false;
            Dictionary<string, string> inputs;
            business = new EntityHelperCore();
            LoadMenu();

            Console.WriteLine("Entity Helper");
            Console.WriteLine($"Version: {Assembly.GetEntryAssembly().GetName().Version}");

            do
            {
                try
                {
                    Console.Write("-> ");
                    inputs = menu.ProcessArgs(Console.ReadLine().Split(" "));

                    if (inputs.ContainsKey("EXIT"))
                    {
                        exit = true;
                    }
                    else if (inputs.ContainsKey("RUN"))
                    {
                        RunWizard(inputs.GetValueOrDefault("RUN"));
                    }
                    else if (inputs.ContainsKey("PATH"))
                    {
                        business.CurrentFolder = inputs.GetValueOrDefault("PATH");
                    }
                    else if (inputs.ContainsKey("LISTCONN"))
                    {
                        Console.WriteLine(string.Join("\n", business.GetConnections()));
                    }
                    else if (inputs.ContainsKey("REMCONN"))
                    {
                        business.RemoveConnection(inputs.GetValueOrDefault("REMCONN"));
                        Console.WriteLine($"Connection removed: {inputs.GetValueOrDefault("REMCONN")}");
                    }
                    else if (inputs.ContainsKey("ADDCONN"))
                    {
                        ConnWizard(inputs.GetValueOrDefault("ADDCONN"));
                    }
                    else if (inputs.ContainsKey("ENV"))
                    {
                        Console.WriteLine($"Current directory: {business.CurrentFolder}");
                    }
                    else if (inputs.ContainsKey("VERSION"))
                    {
                        Console.WriteLine("Entity Helper version: " + Assembly.GetEntryAssembly().GetName().Version);
                    }
                    else if (inputs.ContainsKey("HELP"))
                    {
                        Help();
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            } while (!exit);
        }

        private static void RunWizard(string connectionName)
        {
            string defaultNamespace = "";
            Connection connection = business.GetConnection(connectionName);
            ExecConfig execConfig;
            if (connection != null)
            {
                execConfig = new ExecConfig() { connection = connection };

                execConfig.Table = ReadInput("Table or View name (Default: enpty = all tables/views):", "", "");
                execConfig.IncludeViews = false;
                if (string.IsNullOrEmpty(execConfig.Table))
                {
                    execConfig.AllTables = true;
                    execConfig.IncludeViews = ReadInput("Include Views? (Y/[N])", "N", "").Equals("Y");
                }

                execConfig.DtoFolder = ReadInput("Dto's folder? (Default: .\\)", "", "");
                execConfig.EntityFolder = ReadInput("Entities folder? (Default: .\\)", "", "");

                if (!Path.IsPathRooted(execConfig.DtoFolder))
                {
                    execConfig.DtoFolder = Path.Combine(business.CurrentFolder, execConfig.DtoFolder);
                }

                if (!Path.IsPathRooted(execConfig.EntityFolder))
                {
                    execConfig.EntityFolder = Path.Combine(business.CurrentFolder, execConfig.EntityFolder);
                }

                defaultNamespace = FindNameSpace();
                if (string.IsNullOrEmpty(defaultNamespace))
                {
                    defaultNamespace = ReadInput("Base namespace", null, "Must inform the base namespace");
                }
                else
                {
                    defaultNamespace = ReadInput($"Base namespace (Default: {defaultNamespace})", defaultNamespace, "");
                }

                execConfig.DtoNamespace = ReadInput($"Dto's NameSpace: (Default: {defaultNamespace}.Dtos)", $"{defaultNamespace}.Dtos", "");
                execConfig.EntityNamespace = ReadInput($"Entities NameSpace: (Default: {defaultNamespace}.Entities)", $"{defaultNamespace}.Entities", "");

                execConfig.UseEntityFramework = ReadInput("Use Entity Framework Primary key annotations? (Y/[N])", "N", "").Equals("Y");

                List<string> result = business.Run(execConfig);
                Console.WriteLine("Files generated:");
                Console.WriteLine(string.Join('\n', result));
            }
            else
            {
                Console.WriteLine($"Connection not found {connectionName}");
            }

        }

        private static void ConnWizard(string name)
        {
            string response = "";
            Connection connection = new Connection() { Name = name };
            bool ok = false;

            do
            {
                Console.WriteLine("Select data base Type (default 1 - MsSql): ");
                Console.WriteLine("1 - MsSql");
                Console.WriteLine("2 - MySql");
                Console.WriteLine("3 - PostgreSql");
                Console.WriteLine("4 - Sqlite");
                response = Console.ReadLine();

                switch (response)
                {
                    case "2":
                        connection.Type = BDataBaseStandard.DataBase.enmDataBaseType.MySql;
                        break;
                    case "3":
                        connection.Type = BDataBaseStandard.DataBase.enmDataBaseType.Postgre;
                        break;
                    case "4":
                        connection.Type = BDataBaseStandard.DataBase.enmDataBaseType.SqLite;
                        break;
                    default:
                        connection.Type = BDataBaseStandard.DataBase.enmDataBaseType.MsSql;
                        break;
                }

                if (ReadInput("Use connection string? (Y/[N])", "N", "").Equals("Y", StringComparison.InvariantCultureIgnoreCase))
                {
                    connection.ConnectionString = ReadInput("Connection string:", null, "The connection string must have an value.");
                }
                else
                {
                    connection.Server = ReadInput("Server:", null, "The database server cannot be empty.");
                    connection.DataBase = ReadInput("Database Name:", null, "The database name cannot be empty.");
                    connection.User = ReadInput("User:", null, "The database User cannot be empty.");
                    connection.Password = ReadInput("Password:", null, "The Password cannot be empty.");
                }

                if (business.TestConnection(connection))
                {
                    business.AddConnection(connection);
                    Console.WriteLine($"Connection {connection.Name} saved.");
                    ok = true;
                }
                else
                {
                    Console.WriteLine($"Connection test fail.");
                    ok = ReadInput("Review settings? (Y/[N])", "N", "").Equals("N", StringComparison.InvariantCultureIgnoreCase);
                }
            } while (!ok);

        }

        private static string FindNameSpace()
        {
            string response = "";
            string[] files = Directory.GetFiles(business.CurrentFolder, "*.csproj");
            if (files.Any())
            {
                response = Path.GetFileNameWithoutExtension(files.First());
            }
            return response;
        }

        private static string ReadInput(string label, string defaultValue, string error)
        {
            bool ok = false;
            string response;
            do
            {
                Console.WriteLine(label);
                response = Console.ReadLine();
                if (string.IsNullOrEmpty(response) && defaultValue != null)
                {
                    response = defaultValue;
                }
                if (!string.IsNullOrEmpty(response) || response.Equals(defaultValue))
                {
                    ok = true;
                }
                else
                {
                    Console.WriteLine(error);
                }
            } while (!ok);
            return response;
        }

        private static void Help()
        {
            Console.WriteLine("Run execution wizard:");
            Console.WriteLine(" --run or -run <connection name>");
            Console.WriteLine("");

            Console.WriteLine("Set Target Path:");
            Console.WriteLine(" --path or -p <connection name>");
            Console.WriteLine("");

            Console.WriteLine("Add Connection:");
            Console.WriteLine(" --add or -a <connection name>");
            Console.WriteLine("");

            Console.WriteLine("List saved connections:");
            Console.WriteLine(" --list or -l");
            Console.WriteLine("");

            Console.WriteLine("Remove saved connections:");
            Console.WriteLine(" --rem or -rem <connection name>");
            Console.WriteLine("");

            Console.WriteLine(" Get current dir:");
            Console.WriteLine("     --dir or -d");
            Console.WriteLine("");

            Console.WriteLine(" Program Version:");
            Console.WriteLine("     --version or -v");

            Console.WriteLine(" Help:");
            Console.WriteLine("     --help or -h");

            Console.WriteLine(" Exit:");
            Console.WriteLine("     --exit or -e");
        }

        private static void LoadMenu()
        {
            menu = new ASplitter(@"{
                                      ""args"": [
                                        {
                                          ""commands"": [ ""-l"", ""--list"" ],
                                          ""params"": [
                                              { ""key"": ""LISTCONN"", ""void"":  true }
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-a"", ""--add"" ],
                                          ""params"": [
                                              { ""key"": ""ADDCONN"", ""void"":  true }
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-rem"", ""--rem"" ],
                                          ""params"": [
                                              { ""key"": ""REMCONN"" }
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-run"", ""--run"" ],
                                          ""params"": [
                                              { ""key"": ""RUN""}
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-v"", ""--version"" ],
                                          ""params"": [
                                              { ""key"": ""VERSION"", ""void"": true }
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-h"", ""--help"" ],
                                          ""params"": [
                                              { ""key"": ""HELP"", ""void"": true }
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-d"", ""--dir"" ],
                                          ""params"": [
                                              { ""key"": ""ENV"", ""void"": true }
                                          ]
                                        },
                                        {
                                          ""commands"": [ ""-p"", ""--path"" ],
                                          ""params"": [
                                              { ""key"": ""PATH"" }
                                          ]
                                        },                           
                                        {
                                          ""commands"": [ ""-e"", ""--exit"" ],
                                          ""params"": [
                                            { ""key"": ""EXIT"", ""void"":  true }
                                          ]
                                        }
                                      ]
                                    }");
        }
    }
}
