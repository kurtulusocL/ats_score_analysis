using ATS.Application.Results;

namespace ATS.Application.Abstract.Services
{
    public interface IHiddenTextFileScanner
    {
        string FileType { get; }

        IReadOnlyList<SecurityFindingResult> ScanFile(string filePath);
    }
}
