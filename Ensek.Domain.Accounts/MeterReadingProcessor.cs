using Ensek.Domain.Accounts.DataConverters;
using Ensek.Models;
using Ensek.Repository.Accounts;
using System.Text.RegularExpressions;

namespace Ensek.Domain.Accounts
{
    /**
     * The MeterReadingProcessor class is responsible for processing and saving CSV meter reading data.
     * It utilizes repositories and data converters to perform the required operations.
     */
    public class MeterReadingProcessor : IMeterReadingProcessor {

        private readonly ICustomerMeteringDataRepository _customerMeteringDataRepository;
        private readonly ICustomerAccountsRepository _customerAccountsRepository;
        private readonly ICsvDataConverter<MeterReading> _csvDataConverter;

        /**
         * Constructs a new instance of the MeterReadingProcessor class.
         *
         * @param customerMeteringDataRepository The repository for customer metering data.
         * @param customerAccountsRepository The repository for customer accounts.
         * @param csvDataConverter The converter for CSV data to MeterReading objects.
         */
        public MeterReadingProcessor(
                ICustomerMeteringDataRepository customerMeteringDataRepository,
                ICustomerAccountsRepository customerAccountsRepository,
                ICsvDataConverter<MeterReading> csvDataConverter) {
            _customerMeteringDataRepository = customerMeteringDataRepository;
            _customerAccountsRepository = customerAccountsRepository;
            _csvDataConverter = csvDataConverter;
        }

        /**
         * Saves the provided CSV data into the database.
         *
         * @param csvData The stream containing the CSV data.
         * @returns A Result object indicating the success of the operation and any relevant messages.
         */
        public async Task<Result> SaveCsvData(Stream csvData) {
            var result = new Result();

            // stop processing if no stream provided 
            if (csvData == null || csvData == Stream.Null) {
                result.IsSuccessful = false;
                result.Message = "No csv data found in upload request";
                return result;
            }

            // Convert csv contents 
            var conversionResult = await _csvDataConverter.Convert(csvData);

            // stop processing if csv conversion failed
            if (conversionResult == null) {
                result.IsSuccessful = true;
                result.Message = "No valid data found to upload into database";
                return result;
            }

            // add csv conversion fail count into result
            if (conversionResult.InvalidData != null) {
                result.FailedCount = conversionResult.InvalidData.Count;
            }

            // stop processing if there is no valid rows
            if (conversionResult.ValidData == null || conversionResult.ValidData.Count == 0) {               
                result.IsSuccessful = true;
                result.Message = "No valid data found to upload into database";
                return result;
            }
                        
            // Check meter readings duplication
            var nonDuplicateMeterReadings = await _customerMeteringDataRepository.GetNonDuplicateMeterReadings(conversionResult.ValidData);
            var duplicateCount = conversionResult.ValidData.Count - nonDuplicateMeterReadings.Count;

            if (nonDuplicateMeterReadings != null && nonDuplicateMeterReadings.Any()) {

                if (conversionResult.InvalidData == null) {
                    conversionResult.InvalidData = new List<DataConversionError<MeterReading>>();
                }

                //Now check if metering data has the valid metering value and account id
                var meterReadingsToSaveList = await GetValidMeterReadings(conversionResult, nonDuplicateMeterReadings);

                if (meterReadingsToSaveList != null && meterReadingsToSaveList.Any()) {
                    // save into database
                    await _customerMeteringDataRepository.Save(meterReadingsToSaveList);
                    result.SuccessfulCount = meterReadingsToSaveList.Count;
                }

                if (conversionResult.InvalidData != null) {
                    result.FailedCount = conversionResult.InvalidData.Count;
                }

                if (duplicateCount > 0) {
                    result.FailedCount += duplicateCount;
                }

                result.IsSuccessful = true;
                result.Message = "Data has been processed";
            } else {
                if (duplicateCount > 0) {
                    result.FailedCount += duplicateCount;
                }
                result.IsSuccessful = true;
                result.Message = "No valid data found to upload into database";
            }

            return result;
        }

        /**
         * Retrieves a list of valid meter readings by performing additional validation on the provided non-duplicate meter readings.
         *
         * @param conversionResult The result of the CSV data conversion.
         * @param nonDuplicateMeterReadings The list of non-duplicate meter readings to validate.
         * @returns A list of valid meter readings.
         */
        public async Task<List<MeterReading>> GetValidMeterReadings(DataConverterResult<MeterReading> conversionResult, List<MeterReading> nonDuplicateMeterReadings)
        {
            var list = new List<MeterReading>();

            for (int i = 0; i < nonDuplicateMeterReadings.Count; i++) {

                var row = nonDuplicateMeterReadings[i];

                //Check Metering data is in valid format NNNNN.
                string pattern = @"^\d{5}$";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(row.MeterReadValue);

                // if validation failed
                if (!match.Success) {
                    var error = new DataConversionError<MeterReading> {
                        Text = "Invalid metering data"
                    };
                    conversionResult.InvalidData.Add(error);
                }
                else
                {
                    var accountId = row.AccountId;

                    //Check if provided account exists in database
                    var account = await _customerAccountsRepository.GetAccount(accountId);
                    if (account != null)
                    {
                        list.Add(nonDuplicateMeterReadings[i]);
                    }
                    else
                    {
                        var error = new DataConversionError<MeterReading>
                        {
                            Text = "Invalid account"
                        };

                        conversionResult.InvalidData.Add(error);
                    }
                }
            }
            return list;
        }
    }
}