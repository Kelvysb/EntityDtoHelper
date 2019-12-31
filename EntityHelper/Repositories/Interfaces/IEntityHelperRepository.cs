using BDataBaseStandard;
using EntityHelper.Models;

namespace EntityHelper.Repositories.Interfaces
{
    public interface IEntityHelperRepository
    {
        void Connect(Connection connection);
        clsTableInfo GetTableInfo(string table);
    }
}