using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

using MK.Utilities;

namespace MK.Data.RelationalDB
{
    /// <summary>
    /// This class is not thread-safe by default
    /// </summary>
    public class DBAccess
    {
        #region Internal Definitions

        private enum ExecuteType
        {
            NonQuery,
            Scalar,
            Reader,
            DataTable
        }

        #endregion

        #region Private Properties

        private DbConnection _Connection;
        private static DbConnection _SharedConnection;
        private DbConnection Connection
        {
            get
            {
                return ShareConnection ? _SharedConnection : _Connection;
            }
            set
            {
                if (ShareConnection)
                    _SharedConnection = value;
                else
                    _Connection = value;
            }
        }

        private DbTransaction _Transaction;
        private static DbTransaction _SharedTransaction;
        private DbTransaction Transaction
        {
            get
            {
                return ShareConnection ? _SharedTransaction : _Transaction;
            }
            set
            {
                if (ShareConnection)
                    _SharedTransaction = value;
                else
                    _Transaction = value;
            }
        }

        public bool ShareConnection{ get; private set; }

        #endregion
        #region Public Properties

        public Database DB { get; set; }

        #endregion


        #region Constructor

        static DBAccess()
        {
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());
        }

        public DBAccess(string connectionString, bool shareConnection = true)
        {
            ShareConnection = shareConnection;            

            DB = DatabaseFactory.CreateDatabase(connectionString);

            if (!(DB is SqlDatabase))
                throw new Exception("Unsupported database!");

            InitDB();
        }

        public DBAccess()
            : this(Constans.ConnectionString)
        {
        }

        #endregion


        #region Methods

        public long GetLastIdentity()
        {
            if (!InTransaction)
                throw new Exception("GetLastIdentity does not work if transaction is not active!");

            return DBUtils.GetLastIdentity(DB, Transaction);
        }

        public DateTime GetDateTimeFromServer()
        {
            return (DateTime)ExecuteScalar<DateTime>(GetSqlStringCommand("SELECT getdate();"));
        }

        public DbCommand GetSqlStringCommand(string cmd)
        {
            return DB.GetSqlStringCommand(cmd);
        }

        public string BuildParameterName(string name)
        {
            name.NotNull("name");
            return DB.BuildParameterName(name);
        }

        protected virtual void InitDB()
        {
        }

        #endregion

        #region Execute

        public void ExecuteNonQuery(string cmd)
        {
            cmd.NotNull("cmd");
            ExecuteNonQuery(GetSqlStringCommand(cmd));
        }
        public T ExecuteScalar<T>(string cmd)
        {
            cmd.NotNull("cmd");
            return (T)ExecuteScalar<T>(GetSqlStringCommand(cmd));
        }
        public IDataReader ExecuteReader(string cmd)
        {
            cmd.NotNull("cmd");
            return (IDataReader)ExecuteReader(GetSqlStringCommand(cmd));
        }
        public DataTable ExecuteDataTable(string cmd)
        {
            cmd.NotNull("cmd");
            return (DataTable)ExecuteDataTable(GetSqlStringCommand(cmd));
        }

        public void ExecuteNonQuery(DbCommand cmd)
        {
            cmd.NotNull("cmd");
            Execute(cmd, ExecuteType.NonQuery);
        }
        public T ExecuteScalar<T>(DbCommand cmd)
        {
            cmd.NotNull("cmd");
            return (T)Execute(cmd, ExecuteType.Scalar);
        }
        public IDataReader ExecuteReader(DbCommand cmd)
        {
            cmd.NotNull("cmd");
            return (IDataReader)Execute(cmd, ExecuteType.Reader);
        }
        public DataTable ExecuteDataTable(DbCommand cmd)
        {
            cmd.NotNull("cmd");
            return (DataTable)Execute(cmd, ExecuteType.DataTable);
        }

        private object Execute(DbCommand cmd, ExecuteType type)
        {
            object res = null;
            DbConnection conn = null;
            try
            {
                conn = ConnectionAndCommandSetup(cmd);
                switch (type)
                {
                    case ExecuteType.NonQuery:
                        cmd.ExecuteNonQuery();
                        break;

                    case ExecuteType.Scalar:
                        res = cmd.ExecuteScalar();
                        break;

                    case ExecuteType.Reader:
                        //We don't use CommandBehavior.CloseConnection because when reader is closed,
                        //the underlying connection will be also closed which means that transaction/session 
                        //will also end
                        if (InTransaction || InSession)
                            res = cmd.ExecuteReader();
                        else
                            res = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        break;

                    case ExecuteType.DataTable:
                        IDataReader reader = null;

                        //We don't use CommandBehavior.CloseConnection because when reader is closed,
                        //the underlying connection will be also closed which means that transaction/session 
                        //will also end
                        if (InTransaction || InSession)
                            reader = cmd.ExecuteReader();
                        else
                            reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                        DataTable dt = new DataTable();
                        dt.Load((IDataReader)reader);

                        res = dt;
                        break;
                }
            }
            finally
            {
                if (type != DBAccess.ExecuteType.Reader)
                    CloseConnection(conn);
            }

            return res;
        }

        private DbConnection ConnectionAndCommandSetup(DbCommand cmd)
        {
            DbConnection conn = OpenConnection();
            cmd.Connection = conn;
            if (InTransaction)
                cmd.Transaction = Transaction;

            return conn;
        }

        #endregion

        #region Connection

        private DbConnection OpenConnection()
        {
            DbConnection conn = null;

            if (InTransaction || InSession)
                conn = Connection;
            else
            {
                conn = DB.CreateConnection();
                conn.Open();
            }

            return conn;
        }

        private void CloseConnection(DbConnection connection)
        {
            if (connection != null && !InTransaction && !InSession)
                connection.Close();
        }

        #endregion

        #region Transaction

        public bool InTransaction
        {
            get { return Transaction != null; }
        }

        public void BeginTransaction()
        {
            if (!InTransaction)
            {
                Connection = OpenConnection();
                Transaction = Connection.BeginTransaction(IsolationLevel.RepeatableRead);
            }
        }
        public void CommitTransaction()
        {
            if (InTransaction)
            {
                Transaction.Commit();
                CloseConnection(Connection);

                Transaction = null;
                Connection = null;
            }
        }
        public void RollbackTransaction()
        {
            if (InTransaction)
            {
                Transaction.Rollback();
                CloseConnection(Connection);

                Transaction = null;
                Connection = null;
            }
        }

        #endregion
        #region Session

        public bool InSession
        {
            get { return Connection != null; }
        }

        public void OpenSession()
        {
            if (!InSession)
            {
                Connection = OpenConnection();
            }
        }
        public void CloseSession()
        {
            if (InSession)
            {
                CloseConnection(Connection);

                Connection = null;
            }
        }

        #endregion
    }
}
