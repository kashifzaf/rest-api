namespace Ensek.Repository.Accounts
{
    /**
     * The RepositoryBase class is an abstract base class that provides common functionality for repository classes.
     * It contains methods for converting DateTime objects to Unix timestamps and vice versa.
     */
    public abstract class RepositoryBase {
        /**
         * Converts a DateTime object to a Unix timestamp (seconds since January 1, 1970).
         *
         * @param dateTime The DateTime object to convert.
         * @returns The Unix timestamp in seconds.
         */
        protected long ToUnixTimeSeconds(DateTime dateTime) {
            DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
            return dateTimeOffset.ToUnixTimeSeconds();
        }

        /**
         * Converts a Unix timestamp (seconds since January 1, 1970) to a DateTime object.
         *
         * @param unixTimestamp The Unix timestamp in seconds.
         * @returns The corresponding DateTime object.
         */
        protected DateTime ToDateTime(long unixTimestamp) {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            return dateTimeOffset.UtcDateTime;
        }
    }
}