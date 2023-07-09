using Ensek.Domain.Accounts;
using Ensek.Domain.Accounts.DataConverters;
using Ensek.Models;
using Ensek.Repository.Accounts;
using Moq;
namespace Ensek.Tests.Domain.Tests
{

    [TestClass]
    public class MeterReadingProcessorTests
    {
        private Mock<ICustomerMeteringDataRepository> customerMeteringDataRepositoryMock;
        private Mock<ICustomerAccountsRepository> customerAccountsRepositoryMock;
        private Mock<ICsvDataConverter<MeterReading>> csvDataConverterMock;

        private MeterReadingProcessor processor;

        [TestInitialize]
        public void TestInitialize()
        {
            customerMeteringDataRepositoryMock = new Mock<ICustomerMeteringDataRepository>();
            customerAccountsRepositoryMock = new Mock<ICustomerAccountsRepository>();
            csvDataConverterMock = new Mock<ICsvDataConverter<MeterReading>>();

            processor = new MeterReadingProcessor(
                customerMeteringDataRepositoryMock.Object,
                customerAccountsRepositoryMock.Object,
                csvDataConverterMock.Object);
        }

        [TestMethod]
        public async Task SaveCsvData_Should_ReturnNoCsvDataMessage_When_CsvDataIsNull()
        {
            // Act
            var result = await processor.SaveCsvData(null);

            // Assert
            Assert.IsFalse(result.IsSuccessful);
            Assert.AreEqual("No csv data found in upload request", result.Message);
        }

       
        [TestMethod]
        public async Task SaveCsvData_Should_ReturnNoValidDataMessage_When_ConversionResultIsNull()
        {
            // Arrange            
            var csvData = new MemoryStream();
            csvDataConverterMock.Setup(c => c.Convert(csvData)).Returns(Task.FromResult<DataConverterResult<MeterReading>>(null));

            // Act
            var result = await processor.SaveCsvData(csvData);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("No valid data found to upload into database", result.Message);
        }

        [TestMethod]
        public async Task SaveCsvData_Should_ReturnNoValidDataMessage_When_ConversionResultHasNoValidData()
        {
            // Arrange            
            var csvData = new MemoryStream();
            var conversionResult = new DataConverterResult<MeterReading>
            {
                ValidData = new List<MeterReading>()
            };
            csvDataConverterMock.Setup(c => c.Convert(csvData)).ReturnsAsync(conversionResult);

            // Act
            var result = await processor.SaveCsvData(csvData);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("No valid data found to upload into database", result.Message);
        }

        [TestMethod]
        public async Task SaveCsvData_Should_SaveValidMeterReadings_When_ConversionResultHasValidData()
        {
            // Arrange            
            var csvData = new MemoryStream();
            var meterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = "10024" },
                new MeterReading { AccountId = 2, MeterReadValue = "20034" }
            };
            var conversionResult = new DataConverterResult<MeterReading>
            {
                ValidData = meterReadings
            };
            csvDataConverterMock.Setup(c => c.Convert(csvData)).ReturnsAsync(conversionResult);
            customerAccountsRepositoryMock.Setup(c => c.GetAccount(1)).ReturnsAsync(new CustomerAccount());
            customerAccountsRepositoryMock.Setup(c => c.GetAccount(2)).ReturnsAsync(new CustomerAccount());
            customerMeteringDataRepositoryMock.Setup(c => c.GetNonDuplicateMeterReadings(conversionResult.ValidData)).ReturnsAsync(meterReadings);
            customerMeteringDataRepositoryMock.Setup(c => c.Save(It.IsAny<List<MeterReading>>())).Returns(Task.CompletedTask);

            // Act
            var result = await processor.SaveCsvData(csvData);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("Data has been processed", result.Message);
            Assert.AreEqual(2, result.SuccessfulCount);
            Assert.AreEqual(0, result.FailedCount);
            customerMeteringDataRepositoryMock.Verify(c => c.Save(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveCsvData_Should_SaveValidMeterReadings_And_CheckForDuplicates()
        {
            // Arrange            
            var csvData = new MemoryStream();
            var meterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = "10024" },
                new MeterReading { AccountId = 2, MeterReadValue = "20034" }
            };
            var duplicateMeterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 2, MeterReadValue = "20034" }
            };
            var conversionResult = new DataConverterResult<MeterReading>
            {
                ValidData = meterReadings
            };
            csvDataConverterMock.Setup(c => c.Convert(csvData)).ReturnsAsync(conversionResult);
            customerAccountsRepositoryMock.Setup(c => c.GetAccount(1)).ReturnsAsync(new CustomerAccount());
            customerAccountsRepositoryMock.Setup(c => c.GetAccount(2)).ReturnsAsync(new CustomerAccount());
            customerMeteringDataRepositoryMock.Setup(c => c.GetNonDuplicateMeterReadings(conversionResult.ValidData)).ReturnsAsync(duplicateMeterReadings);
            customerMeteringDataRepositoryMock.Setup(c => c.Save(It.IsAny<List<MeterReading>>())).Returns(Task.CompletedTask);

            // Act
            var result = await processor.SaveCsvData(csvData);

            // Assert
            Assert.IsTrue(result.IsSuccessful);
            Assert.AreEqual("Data has been processed", result.Message);
            Assert.AreEqual(1, result.SuccessfulCount);
            Assert.AreEqual(1, result.FailedCount);
            customerMeteringDataRepositoryMock.Verify(c => c.Save(It.IsAny<List<MeterReading>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetValidMeterReadings_Should_ReturnValidMeterReadings_When_MeterReadings_Are_Valid_And_Account_Exist()
        {
            // Arrange
            var conversionResult = new DataConverterResult<MeterReading>
            {
                InvalidData = new List<DataConversionError<MeterReading>>()
            };
            var nonDuplicateMeterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = "10024" },
                new MeterReading { AccountId = 2, MeterReadValue = "20034" }
            };
            var customerMock = new Mock<ICustomerAccountsRepository>();
            var meterReadingProcessor = new MeterReadingProcessor(
                null,
                customerMock.Object,
                null);

            // Mock the GetAccount method to return a valid account for AccountId = 1
            customerMock.Setup(c => c.GetAccount(1)).ReturnsAsync(new CustomerAccount());
            // Mock the GetAccount method to return a valid account for AccountId = 2
            customerMock.Setup(c => c.GetAccount(2)).ReturnsAsync(new CustomerAccount());

            // Act
            var result = await meterReadingProcessor.GetValidMeterReadings(conversionResult, nonDuplicateMeterReadings);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(0, conversionResult.InvalidData.Count);
            Assert.AreEqual(1, result[0].AccountId);
            Assert.AreEqual(2, result[1].AccountId);
            Assert.AreEqual("10024", result[0].MeterReadValue);
            Assert.AreEqual("20034", result[1].MeterReadValue);
        }

        [TestMethod]
        public async Task GetValidMeterReadings_Should_NotReturnValidMeterReadings_When_Account_Not_Exist()
        {
            // Arrange
            var conversionResult = new DataConverterResult<MeterReading>
            {
                InvalidData = new List<DataConversionError<MeterReading>>()
            };
            var nonDuplicateMeterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = "10024" }
            };
            var customerMock = new Mock<ICustomerAccountsRepository>();
            var meterReadingProcessor = new MeterReadingProcessor(
                null,
                customerMock.Object,
                null);

            // Mock the GetAccount method to return a null
            customerMock.Setup(c => c.GetAccount(1)).ReturnsAsync((CustomerAccount)null);

            // Act
            var result = await meterReadingProcessor.GetValidMeterReadings(conversionResult, nonDuplicateMeterReadings);

            // Assert
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(1, conversionResult.InvalidData.Count);
        }

        [TestMethod]
        public async Task GetValidMeterReadings_Should_NotReturnValidMeterReadings_When_MeterReading_Length_Is_Not_5()
        {
            // Arrange
            var conversionResult = new DataConverterResult<MeterReading>
            {
                InvalidData = new List<DataConversionError<MeterReading>>()
            };
            var nonDuplicateMeterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = "1004" }
            };
            var customerMock = new Mock<ICustomerAccountsRepository>();
            var meterReadingProcessor = new MeterReadingProcessor(
                null,
                customerMock.Object,
                null);

            // Mock the GetAccount method to return a valid account for AccountId = 1
            customerMock.Setup(c => c.GetAccount(1)).ReturnsAsync(new CustomerAccount());

            // Act
            var result = await meterReadingProcessor.GetValidMeterReadings(conversionResult, nonDuplicateMeterReadings);

            // Assert
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(1, conversionResult.InvalidData.Count);
        }

        [TestMethod]
        public async Task GetValidMeterReadings_Should_NotReturnValidMeterReadings_When_MeterReading_Is_Not_Numeric()
        {
            // Arrange
            var conversionResult = new DataConverterResult<MeterReading>
            {
                InvalidData = new List<DataConversionError<MeterReading>>()
            };
            var nonDuplicateMeterReadings = new List<MeterReading>
            {
                new MeterReading { AccountId = 1, MeterReadValue = "abc" }
            };
            var customerMock = new Mock<ICustomerAccountsRepository>();
            var meterReadingProcessor = new MeterReadingProcessor(
                null,
                customerMock.Object,
                null);

            // Mock the GetAccount method to return a valid account for AccountId = 1
            customerMock.Setup(c => c.GetAccount(1)).ReturnsAsync(new CustomerAccount());

            // Act
            var result = await meterReadingProcessor.GetValidMeterReadings(conversionResult, nonDuplicateMeterReadings);

            // Assert
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(1, conversionResult.InvalidData.Count);
        }
    }
}