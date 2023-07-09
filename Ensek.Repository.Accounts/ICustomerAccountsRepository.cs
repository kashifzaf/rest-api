using Ensek.Models;

namespace Ensek.Repository.Accounts {
    public interface ICustomerAccountsRepository {
        Task<CustomerAccount?> GetAccount(int accountId);
    }
}