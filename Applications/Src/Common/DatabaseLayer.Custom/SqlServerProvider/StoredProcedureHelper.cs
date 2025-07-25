﻿using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;

namespace DatabaseLayer.SqlServerProvider
{
    internal class StoredProcedureHelper
    {
        public static bool CheckStoredProcedure(SqlConnection conn, IStoredProcedure storedProcedure)
        {
            try
            {
                string result = string.Empty;
                if (!StoredProcedureHelper.StoredProcedureExists(conn, storedProcedure.StoredProcedureName))
                    return false;
                using (SqlCommand command = conn.CreateCommand())
                {
                    ((DbCommand)command).CommandText = "sp_helptext";
                    ((DbCommand)command).CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ObjName", (object)storedProcedure.StoredProcedureName);
                    using (SqlDataReader sqlDataReader = command.ExecuteReader())
                    {
                        while (((DbDataReader)sqlDataReader).Read())
                            result = result + ((DbDataReader)sqlDataReader).GetString(0) + Environment.NewLine;
                    }
                }
                return StoredProcedureHelper.SkeletenCompare(result, storedProcedure.StoredProcedureCreateText);
            }
            catch
            {
                return false;
            }
        }

        private static bool StoredProcedureExists(SqlConnection conn, string storedProcedureName)
        {
            string objectName;
            if (storedProcedureName.StartsWith("dbo.", StringComparison.OrdinalIgnoreCase))
            {
                // already has a schema, strip it off
                var parts = storedProcedureName.Split(new[] { '.' }, 2);
                objectName = $"[dbo].[{parts[1]}]";
            }
            else
            {
                // no schema, prefix with dbo
                objectName = $"[dbo].[{storedProcedureName}]";
            }

            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.CheckStoredProcedureExists";
                cmd.CommandType = CommandType.StoredProcedure;
                // pass the same “[dbo].[ProcName]” string we were building inline
                cmd.Parameters.AddWithValue("@ObjectName", (object)objectName);
                var res = cmd.ExecuteScalar() is int num && num > 0;
                return res;
            }
        }

        private static bool SkeletenCompare(string result, string value) => StoredProcedureHelper.SkeletanValue(result).CompareTo(StoredProcedureHelper.SkeletanValue(value)) == 0;

        private static string SkeletanValue(string value) => value.Replace(Environment.NewLine, "").Replace(" ", "");

        internal static void Deploy(SqlConnection conn, IStoredProcedure storedProcedure, DeploySettings settings, BaseDataProvider baseDataProvider)
        {
            if (StoredProcedureHelper.StoredProcedureExists(conn, storedProcedure.StoredProcedureName))
            {
                // 1) drop
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "dbo.DropProcedureIfExists";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProcedureName", storedProcedure.StoredProcedureName);
                    cmd.ExecuteNonQuery();
                }
            }

            // 2) create
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "dbo.CreateProcedureFromText";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ProcedureCreateText", storedProcedure.StoredProcedureCreateText);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
