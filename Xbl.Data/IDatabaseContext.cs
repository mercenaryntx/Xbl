using System.Data;

namespace Xbl.Data;

public interface IDatabaseContext
{
    //IDbConnection Connection { get; }
    //Dictionary<Type, Container> Containers { get; }
    Task CreateTables();
    IDataRepository<T> GetRepository<T>() where T : class, IHaveId;
}