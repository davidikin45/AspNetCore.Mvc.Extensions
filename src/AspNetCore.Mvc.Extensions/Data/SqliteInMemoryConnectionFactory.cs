using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data
{
    //https://www.meziantou.net/2017/09/11/testing-ef-core-in-memory-using-sqlite
    public class SqliteInMemoryConnectionFactory : IDisposable
    {
        protected DbConnection _connection;

        //cant create and seed using the same context
        public async Task<DbConnection> GetConnection(CancellationToken cancellationToken = default)
        {
            if (_connection == null)
            {
                _connection = new SqliteConnection("DataSource=:memory:");
                await _connection.OpenAsync(cancellationToken);
            }

            return _connection;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
