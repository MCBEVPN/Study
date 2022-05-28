using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Study.数据库
{
    internal class SqlClient : IDisposable
    {
        private SqlConnection sqlConnection;
        private bool disposedValue;

        public SqlClient(string conStr)
        {
            sqlConnection = new SqlConnection(conStr);
            sqlConnection.Open();
        }

        public void Execute(string cmdText)
        {
            SqlCommand sqlCommand = new SqlCommand(cmdText, sqlConnection);
            sqlCommand.ExecuteNonQuery();
        }

        public void Close()
        {
            sqlConnection.Close();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    sqlConnection.Close();
                    sqlConnection.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
