using CsvHelper;
using System.Globalization;

namespace Ensek.Domain.Accounts.DataConverters
{
    /**
     * The CsvDataConverter class is responsible for converting CSV data into a list of objects of type T.
     * It utilizes CsvHelper library for reading and parsing the CSV data.
     */
    public class CsvDataConverter<T> : ICsvDataConverter<T> {

        /**
         * Converts the provided CSV data stream into a DataConverterResult object.
         *
         * @param stream The stream containing the CSV data.
         * @returns A DataConverterResult object containing the converted data or any conversion errors.
         */
        public async Task<DataConverterResult<T>> Convert(Stream stream) {
            var result = new DataConverterResult<T>();
            
            using var reader = new StreamReader(stream);
            using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture)) {
                try
                {
                    var validData = new List<T>();
                    var invalidData = new List<DataConversionError<T>>();
                    csvReader.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());                    

                    await csvReader.ReadAsync();
                    csvReader.ReadHeader();

                    while (await csvReader.ReadAsync()) {
                        try {
                            var record = csvReader.GetRecord<T>();
                            if (record != null)
                            {
                                validData.Add(record);
                            }                            
                        }
                        catch (CsvHelper.TypeConversion.TypeConverterException ex) {
                            var error = new DataConversionError<T> {
                                Text = ex.Text,
                                TypeConverter = ex.TypeConverter.GetType().FullName
                            };

                            invalidData.Add(error);
                        }
                    }

                    result.ValidData = validData;
                    result.InvalidData = invalidData;
                }
                catch (Exception ex) {
                    // Handle any other exceptions that may occur during CSV processing
                    result.Error = ex.Message;
                }
            }

            return result;
        }
    }
}
