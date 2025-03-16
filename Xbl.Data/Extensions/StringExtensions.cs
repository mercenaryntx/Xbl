using System.Text.RegularExpressions;

namespace Xbl.Data.Extensions;

internal static class StringExtensions
{
    public static string FixFieldReferencesInQuery<TEntity>(this string query)
    {
        const string where = @"(?<=FROM\s+(?<table>\w+)\s+WHERE\s+)(?<condition>(\k<table>\.\w+\s*=\s*@\w+)(?:\s+AND\s+|\s+OR\s+)?)+";
        const string orderBy = @"(?<=ORDER BY )(?:(?:(?<column>\w+)(?:\sASC|\sDESC|)\s*)(?:\s*,\s*)?)+";

        var mapping = PropertyMappings.GetMapping<TEntity>();

        query = Regex.Replace(query, where, match =>
        {
            var table = match.Groups["table"].Value;
            var condition = match.Groups["condition"].Value;
            return Regex.Replace(condition, $@"{table}\.(\w+)\s*=\s*(@\w+)", mm =>
            {
                var field = mm.Groups[1].Value;
                return $"{mapping[field].ColumnName} = {mm.Groups[2].Value}";
            });
        });

        return Regex.Replace(query, orderBy, match =>
        {
            var columns = match.Groups["column"].Captures;
            return string.Join(", ", columns.Select(c => mapping[c.Value].ColumnName));
        });
    }
}