using Microsoft.Data.Sqlite;

namespace Ensek.Repository.Accounts
{
    public interface IDatabaseProvider {
        string ConnectionString { get;  }
        bool IsInitialised { get; }
        Task SetupDatabase();
        Task<SqliteConnection> GetConnection();
    }
}