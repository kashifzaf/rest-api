using Microsoft.Data.Sqlite;
using System.Reflection;

namespace Ensek.Repository.Accounts {
    /**
    * The SqlLiteDatabaseProvider class is responsible for providing and managing the SQLite database connection.
    * It implements the IDatabaseProvider interface.
    */
    public class SqlLiteDatabaseProvider : IDatabaseProvider {
        
        public string ConnectionString { get; private set; }
        public bool IsInitialised { get; private set; }
        private SqliteConnection _sqliteConnection;

        /**
         * Constructs a new instance of the SqlLiteDatabaseProvider class.
         * Initializes the ConnectionString property with a default value.
         */
        public SqlLiteDatabaseProvider() {
            ConnectionString = "Data Source=:memory:"; //TODO: Load from configuration but for now I am hard coding it here;

        }

        /**
         * Retrieves the SQLite database connection.
         *
         * @returns The SQLite connection object.
         */
        public async Task<SqliteConnection> GetConnection() {
            if (_sqliteConnection == null) {
                _sqliteConnection = new SqliteConnection(ConnectionString);
            }

            if (_sqliteConnection.State != System.Data.ConnectionState.Open) {
                await _sqliteConnection.OpenAsync();
            }

            if (!IsInitialised) {
                await SetupDatabase();
            }

            return _sqliteConnection;
        }

        /**
         * Sets up the SQLite database by creating necessary tables and seeding initial data.
         */
        public async Task SetupDatabase() {
            await _sqliteConnection.OpenAsync();

            using (var transaction = _sqliteConnection.BeginTransaction()) {
                using (var command = _sqliteConnection.CreateCommand()) {
                    command.Transaction = transaction;

                    // Create a CustomerAccounts table if it doesn't exist
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS CustomerAccounts (
                                                AccountId INTEGER,
                                                FirstName TEXT,
                                                LastName TEXT
                                            )";
                    await command.ExecuteNonQueryAsync();

                    // Create a MeterReadings table if it doesn't exist
                    command.CommandText = @"CREATE TABLE IF NOT EXISTS MeterReadings (
                                                AccountId INTEGER,
                                                MeterReadingDateTime INTEGER,
                                                MeterReadValue INTEGER
                                            )";
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }

            // Sedd customers accounts
            SeedCustomerAccounts();

            IsInitialised = true;
        }

        /**
         * Seeds the CustomerAccounts table with initial data from a CSV file.
         */
        private void SeedCustomerAccounts() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "Ensek.Repository.Accounts.Test_Accounts.csv";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            using (var transaction = _sqliteConnection.BeginTransaction()) {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null) {
                    if (isFirstLine) {
                        isFirstLine = false;
                        continue; // Skip the header line
                    }

                    string[] fields = line.Split(',');
                    int accountId = int.Parse(fields[0]);
                    string firstName = fields[1];
                    string lastName = fields[2];

                    using (var command = _sqliteConnection.CreateCommand()) {
                        command.CommandText = "INSERT INTO CustomerAccounts (AccountId, FirstName, LastName) VALUES (@AccountId, @FirstName, @LastName)";
                        command.Parameters.AddWithValue("@AccountId", accountId);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }
    }
}