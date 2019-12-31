using BDataBaseStandard;
using EntityHelper.Models;
using EntityHelper.Repositories.Interfaces;

namespace EntityHelper.Repositories
{
    public class EntityHelperRepository : IEntityHelperRepository
    {

        private Connection connection;
        private IDataBase dataBase;

        public EntityHelperRepository() { }

        public EntityHelperRepository(Connection connection)
        {
           Connect(connection);
        }

        public void Connect(Connection connection)
        {             
            if(!string.IsNullOrEmpty(connection.ConnectionString))
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

        public clsTableInfo GetTableInfo(string table)
        {
            return dataBase.fnGetTableInfo(table);
        }
    }
}