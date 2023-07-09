using CsvHelper.Configuration;
using Ensek.Domain.Accounts.DataConverters;

namespace Ensek.Tests.Domain.Tests
{
    [TestClass]
    public class CustomDateTimeConverterTests
    {
        [TestMethod]
        public void ConvertFromString_Should_ConvertValidDateTimeString()
        {
            // Arrange
            var converter = new CustomDateTimeConverter();
            var memberMapData = new MemberMapData(null);
            var validDateTimeString = "22/04/2019 09:24";

            // Act
            var result = converter.ConvertFromString(validDateTimeString, null, memberMapData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(DateTime));
            Assert.AreEqual(new DateTime(2019, 04, 22, 09, 24, 0), result);
        }
    }
}
