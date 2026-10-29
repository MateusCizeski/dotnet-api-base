namespace ApiBase.Infra.Extensions
{
    public static class Extension
    {
        public static T ThrowIfNull<T>(this T? obj, string message) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(message);
            }

            return obj;
        }
    }
}
