namespace MyPhone.IntegrationTest
{
    /// <summary>
    /// Indicates that your PC environment does not support to run the test 
    /// </summary>
    public class EnvironmentNotSupportedException : Exception
    {
        public EnvironmentNotSupportedException()
        {
        }

        public EnvironmentNotSupportedException(string? message) : base(message)
        {
        }

        public EnvironmentNotSupportedException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
