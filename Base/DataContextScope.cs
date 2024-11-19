//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/05 14:28:01</date>
//------------------------------------------------------------------------------

using System;
using System.Data;
using System.Threading;

namespace Zyh.Common.Data
{
    public class DataContextScope : IDisposable
    {
        private const string DefaultConnectionName = "DefaultConnectionString";
        static ThreadLocal<DataContextScope> Local = new ThreadLocal<DataContextScope>();

        public DataContext DataContext { get; private set; }


        public DataContextScope() : this(DefaultConnectionName)
        {

        }

        public DataContextScope(string connectionName)
        {
            DataContext = new DataContext(connectionName);
        }

        public static DataContextScope GetCurrent(string? connectionName = null)
        {
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = DefaultConnectionName;
            }

            DataContextScope ctx = Local.Value;
            if (ctx == null)
            {
                ctx = new DataContextScope(connectionName);
                Local.Value = ctx;
            }

            return ctx;
        }

        public DataContextScope Begin()
        {
            return Begin(false);
        }

        public DataContextScope Begin(bool useTran)
        {
            DataContext.OpenConnection();

            if (useTran)
            {
                DataContext.BeginTransaction();
            }

            return this;
        }

        public DataContextScope Begin(IsolationLevel isolationLevel)
        {
            DataContext.OpenConnection();
            DataContext.BeginTransaction(isolationLevel);

            return this;
        }

        public void Commit()
        {
            DataContext.CommitTransaction();
        }

        public void Rollback()
        {
            DataContext.RollbackTransaction();
        }

        public void Dispose()
        {
            DataContext.DoDispose(true);
        }
    }
}
