using Ensek.Domain.Accounts.DataConverters;
using Ensek.Models;

namespace Ensek.Domain.Accounts {
    public interface IMeterReadingProcessor {
        Task<Result> SaveCsvData(Stream csvData);
        Task<List<MeterReading>> GetValidMeterReadings(DataConverterResult<MeterReading> conversionResult, List<MeterReading> nonDuplicateMeterReadings);
    }
}