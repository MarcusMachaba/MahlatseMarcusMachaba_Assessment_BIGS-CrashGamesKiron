﻿using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Threading;

namespace DatabaseLayer
{
    internal sealed class ConnectionInfo : IDisposable
    {
        private static readonly SemaphoreSlim _connectionLimiter = new SemaphoreSlim(10, 10);
        private readonly string mConnectionString;

        public Guid ConnectionInfoIdentifier { get; }

        public Logger.Logger Log { get; }

        public ConnectionInfo(string connectionString)
        {
            this.mConnectionString = connectionString;
            this.ConnectionInfoIdentifier = Guid.NewGuid();
            this.Log = Logger.Logger.GetLogger(this.GetType());
            this.Log.Debug(string.Format("Creating new connection info object with identifier {0} and connection string \"{1}\"", (object)this.ConnectionInfoIdentifier, (object)this.mConnectionString));
        }

        public void Open()
        {
            // on first creation, try to acquire a slot
            if (this.Connection == null)
            {
                // non-blocking: if pool is already at 10, throw immediately
                if (!_connectionLimiter.Wait(0))
                    throw new InvalidOperationException("Maximum number of open database connections (10) exceeded.");

                this.Log.Debug(string.Format("Creating new connection object with identifier {0}", (object)this.ConnectionInfoIdentifier));
                this.Connection = new SqlConnection(this.mConnectionString);
            }
            if (((DbConnection)this.Connection).State == ConnectionState.Open)
                return;
            this.Log.Debug(string.Format("Opening connection with identifier {0}", (object)this.ConnectionInfoIdentifier));
            ((DbConnection)this.Connection).Open();
        }

        public void Close()
        {
            if (this.Connection == null)
                return;
            this.Log.Debug(string.Format("Closing and disposing connection with identifier {0}", (object)this.ConnectionInfoIdentifier));
            // physically dispose the connection...
            ((Component)this.Connection).Dispose();
            this.Connection = (SqlConnection)null;
            // ...and release our slot back into the pool
            _connectionLimiter.Release();
        }

        public SqlConnection Connection { get; private set; }

        public SqlTransaction Transaction { get; set; }

        public void StartTransaction()
        {
            this.Open();
            this.Log.Debug(string.Format("Beginning new transaction on connection with identifier {0}", (object)this.ConnectionInfoIdentifier));
            this.Transaction = this.Connection.BeginTransaction(IsolationLevel.ReadCommitted, this.GetHashCode().ToString());
        }

        public void CommitTransaction()
        {
            this.Log.Debug(string.Format("Committing transaction on connection with identifier {0}", (object)this.ConnectionInfoIdentifier));
            ((DbTransaction)this.Transaction).Commit();
            ((DbTransaction)this.Transaction).Dispose();
            this.Transaction = (SqlTransaction)null;
        }

        public void RollbackTransaction()
        {
            this.Log.Debug(string.Format("Rolling back transaction on connection with identifier {0}", (object)this.ConnectionInfoIdentifier));
            ((DbTransaction)this.Transaction).Rollback();
            ((DbTransaction)this.Transaction).Dispose();
            this.Transaction = (SqlTransaction)null;
        }

        internal SqlCommand CreateCommand()
        {
            this.Open();
            SqlCommand command = this.Connection.CreateCommand();
            if (this.Transaction != null)
                command.Transaction = this.Transaction;
            return command;
        }

        internal SqlDataAdapter CreateDataAdapter(string storedProcedureName)
        {
            this.Open();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(storedProcedureName, this.Connection);
            if (this.Transaction != null)
                dataAdapter.SelectCommand.Transaction = this.Transaction;
            return dataAdapter;
        }

        public void Dispose()
        {
            if (this.Transaction != null)
                this.RollbackTransaction();
            this.Close();
        }
    }
}
