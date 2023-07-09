using Ensek.Models;

namespace Ensek.Repository.Accounts {
    public interface ICustomerMeteringDataRepository {
        Task Save(List<MeterReading> meterReadings);
        Task<List<MeterReading>> GetNonDuplicateMeterReadings(List<MeterReading> meterReadings);
    }
}