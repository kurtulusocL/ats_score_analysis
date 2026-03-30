using ATS.Application.Analyzers.Base.Models;

namespace ATS.Application.Analyzers.Base
{
    public interface IAnalyzer
    {
        string SectionName { get; }
        int MaxScore { get; }
        AnalyzerResult Analyze(string cvText, string? jobDescription = null);
    }
}