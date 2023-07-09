using Ensek.Domain.Accounts.DataConverters;
using Ensek.Models;

namespace Ensek.Tests.Domain.Tests
{
    [TestClass]
    public class CsvDataConverterTests
    {
        [TestMethod]
        public async Task Convert_Should_ConvertValidCsvData()
        {
            // Arrange
            var converter = new CsvDataConverter<MeterReading>();
            var validCsvData = "AccountId,MeterReadingDateTime,MeterReadValue,\n2344,22/04/2019 09:24,10022,\n2233,22/04/2019 12:25,32322,";
            var stream = GenerateStreamFromString(validCsvData);

            // Act
            var result = await converter.Convert(stream);

            // Assert
            Assert.AreEqual(2, result.ValidData.Count);
            Assert.AreEqual(0, result.InvalidData.Count);
            Assert.IsNull(result.Error);
        }
        
        [TestMethod]
        public async Task Convert_Should_HandleInvalidAccountCsvData()
        {
            // Arrange
            var converter = new CsvDataConverter<MeterReading>();
            var validCsvData = "AccountId,MeterReadingDateTime,MeterReadValue,\nabb,22/04/2019 09:24,10022,";
            var stream = GenerateStreamFromString(validCsvData);

            // Act
            var result = await converter.Convert(stream);

            // Assert
            Assert.AreEqual(0, result.ValidData.Count);
            Assert.AreEqual(1, result.InvalidData.Count);
            Assert.IsNull(result.Error);
        }

        [TestMethod]
        public async Task Convert_Should_HandleInvalidDatetimeCsvData()
        {
            // Arrange
            var converter = new CsvDataConverter<MeterReading>();
            var validCsvData = "AccountId,MeterReadingDateTime,MeterReadValue,\n2344,abc,10022,";
            var stream = GenerateStreamFromString(validCsvData);

            // Act
            var result = await converter.Convert(stream);

            // Assert
            Assert.AreEqual(0, result.ValidData.Count);
            Assert.AreEqual(1, result.InvalidData.Count);
            Assert.IsNull(result.Error);
        }

        [TestMethod]
        public async Task Convert_Should_HandleValidAndInvalidCsvData()
        {
            // Arrange
            var converter = new CsvDataConverter<MeterReading>();
            var validCsvData = "AccountId,MeterReadingDateTime,MeterReadValue,\n2344,22/04/2019 09:24,10022,\naccount,22/04/2019 12:25,32322,";
            var stream = GenerateStreamFromString(validCsvData);

            // Act
            var result = await converter.Convert(stream);

            // Assert
            Assert.AreEqual(1, result.ValidData.Count);
            Assert.AreEqual(1, result.InvalidData.Count);
            Assert.IsNull(result.Error);
        }

        [TestMethod]
        public async Task Convert_Should_HandleExceptions()
        {
            // Arrange
            var converter = new CsvDataConverter<MeterReading>();
            var validCsvData = "AccountId,MeterReadingDateTime,MeterReadValue,\n2344,22/04/2019 09:24";
            var stream = GenerateStreamFromString(validCsvData);

            // Act
            var result = await converter.Convert(stream);

            // Assert
            Assert.IsNull(result.ValidData);
            Assert.IsNull(result.InvalidData);
            Assert.IsNotNull(result.Error);
        }

        // Helper method to convert a string to a stream
        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
