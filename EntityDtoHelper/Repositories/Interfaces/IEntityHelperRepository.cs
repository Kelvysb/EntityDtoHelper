using System;
using System.Collections.Generic;
using BDataBaseStandard;
using EntityHelper.Models;

namespace EntityHelper.Repositories.Interfaces
{
    public interface IEntityHelperRepository: IDisposable
    {
        void Connect(Connection connection);
        clsTableInfo GetTableInfo(string table);
        List<string> GetTables(bool includeViews);
    }
}