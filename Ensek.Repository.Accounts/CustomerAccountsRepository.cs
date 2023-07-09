using Ensek.Models;

namespace Ensek.Repository.Accounts 
{
    /**
     * The CustomerAccountsRepository class is responsible for retrieving customer account data from the database.
     * It implements the ICustomerAccountsRepository interface.
     */
    public class CustomerAccountsRepository : RepositoryBase, ICustomerAccountsRepository {

        private readonly IDatabaseProvider _provider;

        /**
         * Constructs a new instance of the CustomerAccountsRepository class.
         *
         * @param provider The database provider used to establish a database connection.
         */
        public CustomerAccountsRepository(IDatabaseProvider provider) {
            _provider = provider;
        }

        /**
         * Retrieves a customer account by the provided account ID from the database.
         *
         * @param accountId The ID of the account to retrieve.
         * @returns The CustomerAccount object representing the retrieved account, or null if not found.
         */
        public async Task<CustomerAccount?> GetAccount(int accountId) {
            var connection = await _provider.GetConnection();

            await connection.OpenAsync();

            using (var command = connection.CreateCommand()) {
                command.CommandText = "SELECT AccountId, FirstName, LastName FROM CustomerAccounts WHERE AccountId = @AccountId ORDER BY AccountId DESC LIMIT 1";
                command.Parameters.AddWithValue("@AccountId", accountId);

                using (var reader = command.ExecuteReader()) {
                    if (reader.Read()) {
                        var customerAccount = new CustomerAccount {
                            AccountId = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2)
                        };

                        return customerAccount;
                    }
                }
            }

            return null;
        }
    }
}