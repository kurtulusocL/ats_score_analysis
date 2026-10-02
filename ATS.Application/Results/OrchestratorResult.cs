using System.Collections.Generic;
using ATS.Application.Analyzers.Base.Models;

namespace ATS.Application.Results;

public class OrchestratorResult
{
	public string FileName { get; set; } = string.Empty;

	public int TotalScore { get; set; }

	public int TotalMaxScore { get; set; }

	public bool IsGenerallyPassed { get; set; }

	public List<AnalyzerResult> AnalyzerResults { get; set; } = new List<AnalyzerResult>();

	public void RecalculateTotals()
	{
		OverallScore overallScore = OverallScoreCalculator.Calculate(AnalyzerResults);
		TotalScore = overallScore.TotalScore;
		TotalMaxScore = overallScore.TotalMaxScore;
		IsGenerallyPassed = overallScore.IsGenerallyPassed;
	}
}
