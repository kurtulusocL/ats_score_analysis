
namespace ATS.Application.Abstract.Services
{
    public interface ICvParserService
    {
        Task<string> ParseAsync(string filePath, string fileType);
    }
}
