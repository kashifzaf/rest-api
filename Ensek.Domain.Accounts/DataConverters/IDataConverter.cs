namespace Ensek.Domain.Accounts.DataConverters
{
    public interface IDataConverter<T> {
        Task<DataConverterResult<T>> Convert(Stream stream);
    }
}
