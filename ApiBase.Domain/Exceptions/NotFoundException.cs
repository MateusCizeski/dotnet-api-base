namespace ApiBase.Domain.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource does not exist.
    /// The ApiBase exception handler maps it to HTTP 404.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        /// <summary>Creates the exception with a standard message: "Product 'id' was not found."</summary>
        public static NotFoundException For<T>(Guid id) => new($"{typeof(T).Name} '{id}' was not found.");
    }
}
