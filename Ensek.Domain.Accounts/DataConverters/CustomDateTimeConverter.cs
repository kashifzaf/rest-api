using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;

namespace Ensek.Domain.Accounts.DataConverters 
{
    /**
     * The CustomDateTimeConverter class is a custom implementation of the DateTimeConverter class from CsvHelper.
     * It provides custom logic to convert a string representation of a DateTime value to a DateTime object.
     */
    public class CustomDateTimeConverter : DateTimeConverter 
    {
        /**
         * Converts a string representation of a DateTime value to a DateTime object.
         *
         * @param text The string value to convert.
         * @param row The current CSV reader row being processed.
         * @param memberMapData The mapping data for the member being converted.
         * @returns The converted DateTime object.
         */
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData) {
            string[] dateFormats = { "dd/MM/yyyy HH:mm", "d/M/yyyy H:mm" };

            if (DateTime.TryParseExact(text, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime)) {
                return dateTime;
            }

            // Return a default value or throw an exception if the conversion fails
            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}
