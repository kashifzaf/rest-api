using Ensek.Models;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace Ensek.Repository.Accounts
{
    /**
     * The CustomerMeteringDataRepository class is responsible for saving and retrieving customer metering data from the database.
     * It implements the ICustomerMeteringDataRepository interface.
     */
    public class CustomerMeteringDataRepository : RepositoryBase, ICustomerMeteringDataRepository {
        
        private readonly IDatabaseProvider _provider;

        /**
         * Constructs a new instance of the CustomerMeteringDataRepository class.
         *
         * @param provider The database provider used to establish a database connection.
         */
        public CustomerMeteringDataRepository(IDatabaseProvider provider) {
            _provider = provider;
        }

        /**
         * Saves a list of meter readings into the database.
         *
         * @param meterReadings The list of meter readings to save.
         */
        public async Task Save(List<MeterReading> meterReadings) {

            var connection = await _provider.GetConnection();

            using (var transaction = connection.BeginTransaction()) {
                using (var command = connection.CreateCommand()) {
                    command.Transaction = transaction;

                    // Insert MeterReading data into the table
                    command.CommandText = "INSERT INTO MeterReadings (AccountId, MeterReadingDateTime, MeterReadValue) VALUES (@AccountId, @MeterReadingDateTime, @MeterReadValue)";
                    command.Parameters.Add("@AccountId", SqliteType.Text);
                    command.Parameters.Add("@MeterReadingDateTime", SqliteType.Integer); //Unix time because sqlite does not have datetime field. For sql we can change this to datetime
                    command.Parameters.Add("@MeterReadValue", SqliteType.Integer);

                    foreach (var meterReading in meterReadings) {
                        command.Parameters["@AccountId"].Value = meterReading.AccountId;
                        command.Parameters["@MeterReadingDateTime"].Value = ToUnixTimeSeconds(meterReading.MeterReadingDateTime);
                        command.Parameters["@MeterReadValue"].Value = meterReading.MeterReadValue;

                        await command.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
            }
        }

        /**
         * Retrieves a list of non-duplicate meter readings from the provided list of meter readings.
         *
         * @param meterReadings The list of meter readings to check for duplicates.
         * @returns A list of non-duplicate meter readings.
         */
        public async Task<List<MeterReading>> GetNonDuplicateMeterReadings(List<MeterReading> meterReadings) {
            var data = new List<MeterReading>();

            var connection = await _provider.GetConnection();
                // Initialize SQLite provider
                Batteries.Init();
                await connection.OpenAsync();

            using (var command = connection.CreateCommand()) {

                // Check if each meter reading already exists in the database
                command.CommandText = "SELECT COUNT(AccountId) FROM MeterReadings WHERE AccountId = @AccountId AND MeterReadingDateTime = @MeterReadingDateTime AND MeterReadValue = @MeterReadValue";
                command.Parameters.Add("@AccountId", SqliteType.Text);
                command.Parameters.Add("@MeterReadingDateTime", SqliteType.Integer);
                command.Parameters.Add("@MeterReadValue", SqliteType.Text);

                foreach (var meterReading in meterReadings) {
                    command.Parameters["@AccountId"].Value = meterReading.AccountId;
                    command.Parameters["@MeterReadingDateTime"].Value = ToUnixTimeSeconds(meterReading.MeterReadingDateTime);
                    command.Parameters["@MeterReadValue"].Value = meterReading.MeterReadValue;

                    long count = (long)await command.ExecuteScalarAsync();

                    // Add non duplicate data
                    if (count == 0) {
                        data.Add(meterReading);
                    }
                }
            }

            return data;
        }        
    }
}