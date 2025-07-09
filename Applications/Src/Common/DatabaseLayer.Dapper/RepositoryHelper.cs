using Core;
using Dapper.Contrib.Extensions;
using System.Linq;
using System.Reflection;

namespace DatabaseLayer.Dapper
{
    internal static class RepositoryHelper
    {
        internal static string GetTableName(this System.Type t)
        {
            var typedef = t;
            var defaultTableName = typedef.Name + "s";

            var dapperTableNameAttribute = typedef.GetCustomAttribute<TableAttribute>();
            if (dapperTableNameAttribute != null && !string.IsNullOrEmpty(dapperTableNameAttribute.Name))
                return dapperTableNameAttribute.Name;
            return defaultTableName;
        }

        public static string GetTableNameValue<T>() where T : class, IHasId
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true)?.FirstOrDefault();
            if (attribute != null)
                return ((TableAttribute)attribute).Name;
            return $"{typeof(T).Name}s";
        }

        public static string TableNameValue<T>() where T : class, IHasId
        {
            return GetTableNameValue<T>();
        }

        public static string TableNameValue(this System.Type type)
        {
            var attribute = type.GetCustomAttributes(typeof(TableAttribute), true)?.FirstOrDefault();
            if (attribute != null)
                return ((TableAttribute)attribute).Name;
            return $"{type.Name}s";
        }

        public static T[] AsArray<T>(this T item)
        {
            if (item != null)
                return new[] { item };
            return new T[0];
        }

        public static string AutoCacheKey<T>(this object keyColumnValue) where T : IHasId
        {
            return $"{typeof(T).Name}_{keyColumnValue}";
        }

        public static string GetFieldsSelectList<T>()
        {
            var properties = typeof(T).GetProperties();
            var sqlSelectColumns = string.Empty;

            if (properties == null || properties.Length == 0)
                return sqlSelectColumns;

            var stringFields = properties
                .Where(p => IsWriteProperty(p))
                .Select(p => GetFieldName(p));

            return string.Join(",", stringFields);
        }

        private static string GetFieldName(PropertyInfo p)
        {
            var fieldNameProp = p.GetCustomAttribute<SourceColumnAttribute>();
            if (fieldNameProp == null || string.IsNullOrWhiteSpace(fieldNameProp.FieldName))
                return p.Name;

            if (fieldNameProp.FieldName.Trim().Contains(" "))
            {
                var fieldName = $"[{fieldNameProp.FieldName}]";
                return $"{fieldName} AS [{p.Name}]";
            }
            else
            {
                var fieldName = fieldNameProp.FieldName;
                if (fieldName == p.Name)
                    return fieldName;

                return $"{fieldName} AS [{p.Name}]";
            }
        }


        private static bool IsWriteProperty(PropertyInfo p)
        {
            var writeAttribute = p.GetCustomAttribute<WriteAttribute>();
            return writeAttribute == null || writeAttribute.Write == true;
        }
    }
}
