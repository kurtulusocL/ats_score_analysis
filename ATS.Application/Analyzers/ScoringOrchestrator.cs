using System;
using System.Collections.Generic;
using System.Linq;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Application.Results;

namespace ATS.Application.Analyzers;

public class ScoringOrchestrator
{
	private readonly IEnumerable<IAnalyzer> _analyzers;

	public ScoringOrchestrator(IEnumerable<IAnalyzer> analyzers)
	{
		_analyzers = analyzers;
	}

	public OrchestratorResult Run(string cvText, string fileName, string? jobDescription = null)
	{
		List<AnalyzerResult> list = new List<AnalyzerResult>();
		foreach (IAnalyzer analyzer in _analyzers)
		{
			if (!(analyzer.SectionName == "Job Match") || !string.IsNullOrWhiteSpace(jobDescription))
			{
				AnalyzerResult item = analyzer.Analyze(cvText, jobDescription);
				list.Add(item);
			}
		}
		return BuildResult(list, fileName);
	}

	private OrchestratorResult BuildResult(List<AnalyzerResult> results, string fileName)
	{
		if (!results.Any((AnalyzerResult r) => r.SectionName == "Job Match"))
		{
			foreach (AnalyzerResult result in results)
			{
				result.Score = (int)Math.Round((double)result.Score * 1.25);
				result.MaxScore = (int)Math.Round((double)result.MaxScore * 1.25);
			}
		}
		OrchestratorResult orchestratorResult = new OrchestratorResult
		{
			FileName = fileName,
			AnalyzerResults = results
		};
		orchestratorResult.RecalculateTotals();
		return orchestratorResult;
	}
}
