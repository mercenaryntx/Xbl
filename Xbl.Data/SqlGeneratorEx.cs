using MicroOrm.Dapper.Repositories.SqlGenerator;

namespace Xbl.Data;

public class SqlGeneratorEx<T> : SqlGenerator<T> where T : class
{
    public SqlGeneratorEx(string tableName)
    {
        TableName = tableName;
    }
}