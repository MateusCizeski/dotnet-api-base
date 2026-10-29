namespace ApiBase.Domain.Exceptions
{
    /// <summary>
    /// Thrown when a client sends an invalid query: malformed filter/sort JSON, unknown or
    /// non-allowed property, unknown operator or a value that cannot be converted.
    /// The ApiBase exception handler maps it to HTTP 400.
    /// The message is written to be safe to show to the client.
    /// </summary>
    public class InvalidQueryException : Exception
    {
        public InvalidQueryException(string message) : base(message) { }

        public InvalidQueryException(string message, Exception innerException) : base(message, innerException) { }
    }
}
