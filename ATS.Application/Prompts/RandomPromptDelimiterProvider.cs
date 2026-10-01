using System.Security.Cryptography;

namespace ATS.Application.Prompts
{
    public class RandomPromptDelimiterProvider : IPromptDelimiterProvider
    {
        private const int TokenByteCount = 16;

        public string CreateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(TokenByteCount));
    }
}
