namespace Ensek.Domain.Accounts.DataConverters {
    public class DataConverterResult<T> {
        public List<T> ValidData { get; set; }
        public List<DataConversionError<T>> InvalidData { get; set; }
        public string Error { get; set; }
    }
}
