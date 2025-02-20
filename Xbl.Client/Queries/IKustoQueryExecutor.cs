namespace Xbl.Client.Queries;

public interface IKustoQueryExecutor
{
    Task<int> RunKustoQuery();
}