using KustoLoco.Core;

namespace Xbl.Client.Queries;

public interface IKustoQueryExecutor
{
    Task<KustoQueryResult> RunKustoQuery();
}