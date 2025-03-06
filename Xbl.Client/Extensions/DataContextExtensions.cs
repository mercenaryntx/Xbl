using Xbl.Data.Entities;
using Xbl.Data;

namespace Xbl.Client.Extensions;

public static class DataContextExtensions
{
    public static async Task<IEnumerable<T>> GetAll<T>(this IDatabaseContext db) where T : class, IHaveId
    {
        var repo = await db.GetRepository<T>();
        return await repo.GetAll();
    }
}