using System.Collections.Generic;
using BDataBaseStandard;
using EntityHelper.Models;
using EntityHelper.Repositories.Interfaces;

namespace EntityHelper.Repositories
{
    public class EntityHelperRepository : IEntityHelperRepository
    {
        private IDataBase dataBase;

        public EntityHelperRepository() { }

        public EntityHelperRepository(Connection connection)
        {
            Connect(connection);
        }

        public void Connect(Connection connection)
        {
            if (!string.IsNullOrEmpty(connection.ConnectionString))
            {
                dataBase = DataBase.fnOpenConnection(connection.ConnectionString, connection.Type);
            }
            else
            {
                dataBase = DataBase.fnOpenConnection(connection.Server, connection.DataBase,
                                                     connection.User, connection.Password,
                                                     connection.Type);
            }
        }

        public void Dispose()
        {
            if (dataBase != null && dataBase.isOpen)
            {
                dataBase.sbClose();
            }
            dataBase = null;
        }

        public clsTableInfo GetTableInfo(string table)
        {
            return dataBase.fnGetTableInfo(table);
        }

        public List<string> GetTables(bool includeViews)
        {
            string include = includeViews ? "('U', 'V')" : "('U')";
            return dataBase.fnExecute<string>($"select name from sysobjects where type in {include} and name <> 'sysdiagrams'");
        }
    }
}