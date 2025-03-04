using Xbl.Data.Entities;
using Xbl.Data.Repositories;

namespace Xbl.Data;

public interface IDatabaseContext
{
    Task<IRepository<T>> GetRepository<T>(string tableName = null) where T : class, IHaveId;
}