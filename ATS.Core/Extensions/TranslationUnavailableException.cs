

namespace ATS.Core.Extensions
{
    public class TranslationUnavailableException : Exception
    {
        public TranslationUnavailableException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }
}
