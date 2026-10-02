using System.Threading;
using System.Threading.Tasks;

namespace ATS.Application.Abstract.Services;

public interface ICvParserService
{
	Task<string> ParseAsync(string filePath, string fileType, CancellationToken cancellationToken = default(CancellationToken));
}
