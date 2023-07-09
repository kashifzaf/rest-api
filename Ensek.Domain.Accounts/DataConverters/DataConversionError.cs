namespace Ensek.Domain.Accounts.DataConverters {
    public class DataConversionError<T> {
        public string Text { get; set; }
        public string MemberName { get; set; }
        public Type MemberType { get; set; }
        public string TypeConverter { get; set; }
    }
}
