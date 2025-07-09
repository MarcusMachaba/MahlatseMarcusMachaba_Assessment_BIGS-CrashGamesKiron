using DatabaseLayer.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer
{
    internal static class QueryHelper
    {
        internal static async Task<int> ExecuteInsertStoredProcedureAsync(
          string storedProcedureName,
          ConnectionInfo connection,
          params QueryParameter[] parameters)
        {
            int num1;
            try
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    ((DbCommand)cmd).CommandText = storedProcedureName;
                    ((DbCommand)cmd).CommandType = CommandType.StoredProcedure;
                    foreach (QueryParameter parameter in parameters)
                        cmd.Parameters.Add(parameter.GetSqlParameter());
                    ((DbParameter)cmd.Parameters.Add("@RetVal", (SqlDbType)8)).Direction = ParameterDirection.ReturnValue;
                    int num2 = await ((DbCommand)cmd).ExecuteNonQueryAsync();
                    if (!(((DbParameter)cmd.Parameters["@RetVal"]).Value is int))
                        throw new DataAccessException("Insert stored procedure command '" + storedProcedureName + "' did not return an identity value.");
                    num1 = (int)((DbParameter)cmd.Parameters["@RetVal"]).Value;
                }
            }
            catch (Exception ex)
            {
                connection.Log.Error(string.Format("An error occurred while trying to execute insert procedure.{0}{1}{2}Error: {3}", (object)Environment.NewLine, (object)QueryHelper.GetProcedureExecutionString(storedProcedureName, parameters), (object)Environment.NewLine, (object)ex));
                throw;
            }
            return num1;
        }

        private static string GetProcedureExecutionString(
          string storedProcedureName,
          QueryParameter[] parameters)
        {
            return "exec " + storedProcedureName + Environment.NewLine + string.Join(Environment.NewLine + ",", ((IEnumerable<QueryParameter>)parameters).Select<QueryParameter, string>((Func<QueryParameter, string>)(p => string.Format("\t{0} = {1}", (object)p.Name, p.Value))));
        }

        internal static async Task<int> ExecuteStoredProcedureAsync(
          string storedProcedureName,
          ConnectionInfo connection,
          params QueryParameter[] parameters)
        {
            int num;
            try
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    ((DbCommand)cmd).CommandText = storedProcedureName;
                    ((DbCommand)cmd).CommandType = CommandType.StoredProcedure;
                    foreach (QueryParameter parameter in parameters)
                        cmd.Parameters.Add(parameter.GetSqlParameter());
                    num = await ((DbCommand)cmd).ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                connection.Log.Error(string.Format("An error occurred while trying to execute procedure.{0}{1}{2}Error: {3}", (object)Environment.NewLine, (object)QueryHelper.GetProcedureExecutionString(storedProcedureName, parameters), (object)Environment.NewLine, (object)ex));
                throw;
            }
            return num;
        }

        internal static async Task<List<T>> ExecuteRetrievalProcedureAsync<T>(
          string storedProcedureName,
          Func<SqlDataReader, T> convertRowToObj,
          ConnectionInfo connection,
          params QueryParameter[] parameters)
        {
            List<T> objList;
            try
            {
                using (SqlCommand cmd = connection.CreateCommand())
                {
                    ((DbCommand)cmd).CommandText = storedProcedureName;
                    ((DbCommand)cmd).CommandType = CommandType.StoredProcedure;
                    foreach (QueryParameter parameter in parameters)
                        cmd.Parameters.Add(parameter.GetSqlParameter());
                    List<T> result = new List<T>();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (true)
                        {
                            if (await ((DbDataReader)reader).ReadAsync())
                                result.Add(convertRowToObj(reader));
                            else
                                break;
                        }
                    }
                    objList = result;
                }
            }
            catch (Exception ex)
            {
                connection.Log.Error(string.Format("An error occurred while trying to execute retrieve procedure.{0}{1}{2}Error: {3}", (object)Environment.NewLine, (object)QueryHelper.GetProcedureExecutionString(storedProcedureName, parameters), (object)Environment.NewLine, (object)ex));
                throw;
            }
            return objList;
        }

        internal static int ExecuteInsertStoredProcedure(
          string storedProcedureName,
          ConnectionInfo connection,
          params QueryParameter[] parameters)
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    ((DbCommand)command).CommandText = storedProcedureName;
                    ((DbCommand)command).CommandType = CommandType.StoredProcedure;
                    foreach (QueryParameter parameter in parameters)
                        command.Parameters.Add(parameter.GetSqlParameter());
                    ((DbParameter)command.Parameters.Add("@RetVal", (SqlDbType)8)).Direction = ParameterDirection.ReturnValue;
                    ((DbCommand)command).ExecuteNonQuery();
                    if (((DbParameter)command.Parameters["@RetVal"]).Value is int)
                        return (int)((DbParameter)command.Parameters["@RetVal"]).Value;
                    throw new DataAccessException("Insert stored procedure command '" + storedProcedureName + "' did not return an identity value.");
                }
            }
            catch (Exception ex)
            {
                connection.Log.Error(string.Format("An error occurred while trying to execute insert procedure.{0}{1}{2}Error: {3}", (object)Environment.NewLine, (object)QueryHelper.GetProcedureExecutionString(storedProcedureName, parameters), (object)Environment.NewLine, (object)ex));
                throw;
            }
        }

        internal static int ExecuteStoredProcedure(
          string storedProcedureName,
          ConnectionInfo connection,
          params QueryParameter[] parameters)
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    ((DbCommand)command).CommandText = storedProcedureName;
                    ((DbCommand)command).CommandType = CommandType.StoredProcedure;
                    foreach (QueryParameter parameter in parameters)
                        command.Parameters.Add(parameter.GetSqlParameter());
                    return ((DbCommand)command).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                connection.Log.Error(string.Format("An error occurred while trying to execute procedure.{0}{1}{2}Error: {3}", (object)Environment.NewLine, (object)QueryHelper.GetProcedureExecutionString(storedProcedureName, parameters), (object)Environment.NewLine, (object)ex));
                throw;
            }
        }

        internal static List<T> ExecuteRetrievalProcedure<T>(
          string storedProcedureName,
          Func<SqlDataReader, T> convertRowToObj,
          ConnectionInfo connection,
          params QueryParameter[] parameters)
        {
            try
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    ((DbCommand)command).CommandText = storedProcedureName;
                    ((DbCommand)command).CommandType = CommandType.StoredProcedure;
                    foreach (QueryParameter parameter in parameters)
                        command.Parameters.Add(parameter.GetSqlParameter());
                    List<T> objList = new List<T>();
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (((DbDataReader)sqlDataReader).Read())
                            objList.Add(convertRowToObj(sqlDataReader));
                    }
                    return objList;
                }
            }
            catch (Exception ex)
            {
                connection.Log.Error(string.Format("An error occurred while trying to execute retrieve procedure.{0}{1}{2}Error: {3}", (object)Environment.NewLine, (object)QueryHelper.GetProcedureExecutionString(storedProcedureName, parameters), (object)Environment.NewLine, (object)ex));
                throw;
            }
        }
    }
}
