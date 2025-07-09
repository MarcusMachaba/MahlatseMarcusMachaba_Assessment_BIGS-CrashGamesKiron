using Core;
using Dapper;
using DatabaseLayer.Dapper.QueryModel;
using System.Linq;
using System.Text;

namespace DatabaseLayer.Dapper
{
    internal static class SqlQueryHelper
    {
        public static string SelectAllQuery<T>(bool withLock = false) where T : class, IHasId
        {
            return $"SELECT  {RepositoryHelper.GetFieldsSelectList<T>()} FROM {typeof(T).TableNameValue()} {(withLock ? "" : "(nolock)")}";
        }

        public static SqlQuerySet SelectMultipleQuery<T>(QueryPropertyPair[] parameters, bool withLock = false, int? count = null) where T : class, IHasId
        {
            if (count.HasValue && count.Value <= 0)
                count = 100;
            var query = $"SELECT {(count.HasValue ? $"TOP ({count.Value})" : "")} {RepositoryHelper.GetFieldsSelectList<T>()} FROM {typeof(T).TableNameValue()} {(withLock ? "" : "(nolock)")} {parameters.WhereQuery()}";
            var dynamicParameter = CreateFilter(parameters);
            return new SqlQuerySet(query, dynamicParameter);
        }

        public static SqlQuerySet SelectLatestQuery<T>(QueryPropertyPair[] parameters, int number = 1, bool withLock = false) where T : class, IHasId
        {
            var query = $"SELECT TOP ({number}) {RepositoryHelper.GetFieldsSelectList<T>()} FROM {typeof(T).TableNameValue()} {(withLock ? "" : "(nolock)")} {parameters.WhereQuery()}";
            query += $" ORDER BY {nameof(IHasIdRecord.CreationDate)} DESC";
            var dynamicParameter = CreateFilter(parameters);
            return new SqlQuerySet(query, dynamicParameter);
        }

        public static SqlQuerySet SelectOneQuery<T>(QueryPropertyPair[] parameters, bool withLock = false) where T : class, IHasId
        {
            var query = $"SELECT TOP 1  {RepositoryHelper.GetFieldsSelectList<T>()} FROM {typeof(T).TableNameValue()} {(withLock ? "" : "(nolock)")} {parameters.WhereQuery()}";
            var dynamicParameter = CreateFilter(parameters);
            return new SqlQuerySet(query, dynamicParameter);
        }

        public static SqlQuerySet DeleteByIdQuery<T>(long id) where T : class, IHasId
        {
            var parameters = new QueryPropertyPair(nameof(IHasId.Id), id)
                .AsArray();
            var query = $"DELETE FROM {typeof(T).TableNameValue()} {parameters.WhereQuery()}";
            var dynamicParameter = CreateFilter(parameters);
            return new SqlQuerySet(query, dynamicParameter);
        }

        private static string WhereQuery(this QueryPropertyPair[] parameters)
        {
            var whereSql = new StringBuilder(string.Empty);
            if (parameters != null && parameters.Length > 0)
            {
                whereSql.Append($" WHERE {parameters[0].Name}{parameters[0].Comparer.SqlCompare()}@{parameters[0].CustomValueName ?? parameters[0].Name}");
                foreach (var p in parameters.Skip(1) ?? new QueryPropertyPair[0])
                {
                    whereSql.Append($" AND {p.Name}{p.Comparer.SqlCompare()}@{p.CustomValueName ?? p.Name}");
                }
            }

            return whereSql.ToString();
        }

        public static DynamicParameters CreateFilter(params QueryPropertyPair[] parameters)
        {
            var dynamicParameters = new DynamicParameters();
            if (parameters != null && parameters.Length > 0)
            {
                foreach (var parameter in parameters.Where(p => (!string.IsNullOrEmpty(p.Name) || !string.IsNullOrEmpty(p.CustomValueName)) && p.Value != null))
                {
                    dynamicParameters.Add(parameter.CustomValueName ?? parameter.Name, parameter.Value);
                }
            }
            return dynamicParameters;
        }

        private static string SqlCompare(this PropertyPairCoparerEnum comparer)
        {
            switch (comparer)
            {
                case PropertyPairCoparerEnum.Greater:
                    return ">";
                case PropertyPairCoparerEnum.GreaterEquals:
                    return ">=";
                case PropertyPairCoparerEnum.Less:
                    return "<";
                case PropertyPairCoparerEnum.LessEquals:
                    return "<=";
                case PropertyPairCoparerEnum.NotEquals:
                    return "<>";
                default: return "=";
            }
        }
    }
}
