using System;
using System.Data;
using System.Reflection;

namespace DatabaseLayer
{
    public static class TypeConverter
    {
        public static SqlDbType ConvertFromObjectType(Type type, int length, bool isTimeStamp)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GenericTypeArguments[0];
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);
            switch (((MemberInfo)type).Name)
            {
                case "Boolean":
                    return (SqlDbType)2;
                case "Byte":
                    return (SqlDbType)20;
                case "Byte[]":
                    return length != 0 ? (SqlDbType)21 : (SqlDbType)7;
                case "Char":
                    return (SqlDbType)3;
                case "DateTime":
                    return !isTimeStamp ? (SqlDbType)31 : (SqlDbType)4;
                case "Decimal":
                    return (SqlDbType)5;
                case "Double":
                    return (SqlDbType)6;
                case "Guid":
                    return (SqlDbType)14;
                case "Int16":
                    return (SqlDbType)16;
                case "Int32":
                    return (SqlDbType)8;
                case "Int64":
                    return (SqlDbType)0;
                case "String":
                    return length != 0 ? (SqlDbType)22 : (SqlDbType)18;
                case "TimeSpan":
                    return (SqlDbType)32;
                default:
                    throw new ArgumentException("Could not convert object type " + ((MemberInfo)type).Name + " to a sql db type.");
            }
        }

        public static Type ConvertFromSqlType(string type, int length)
        {
            switch (type.ToUpper())
            {
                case "BIGINT":
                    return typeof(long);
                case "BIT":
                    return typeof(bool);
                case "CHAR":
                    return typeof(char);
                case "DATE":
                    return typeof(DateTime);
                case "DATETIME":
                    return typeof(DateTime);
                case "DECIMAL":
                    return typeof(Decimal);
                case "FLOAT":
                    return typeof(double);
                case "IMAGE":
                    return typeof(byte[]);
                case "INT":
                    return typeof(int);
                case "SMALLINT":
                    return typeof(short);
                case "TEXT":
                    return typeof(string);
                case "TIME":
                    return typeof(TimeSpan);
                case "TINYINT":
                    return typeof(byte);
                case "UNIQUEIDENTIFIER":
                    return typeof(Guid);
                case "VARBINARY":
                    return typeof(byte[]);
                case "VARCHAR":
                    return length != 1 ? typeof(string) : typeof(char);
                default:
                    throw new ArgumentException("Could not convert sql db type " + type + " to a object type.");
            }
        }

        internal static SqlDbType ConvertFromObjectType(object dataType, int v1, bool v2) => throw new NotImplementedException();

        internal static SqlDbType ConvertFromObjectType(
          Type dataType,
          object length,
          object isTimeStamp)
        {
            throw new NotImplementedException();
        }
    }
}
