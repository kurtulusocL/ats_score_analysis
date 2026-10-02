using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ATS.Application.Analyzers.Base;
using ATS.Application.Analyzers.Base.Models;
using ATS.Core.Constants;
using ATS.Core.Extensions;

namespace ATS.Application.Analyzers;

public class ConsistencyAnalyzer : IAnalyzer
{
	private readonly IEnumerable<string> _summaryHeaders;

	private readonly IEnumerable<string> _experienceHeaders;

	private readonly IEnumerable<string> _skillsHeaders;

	private static readonly Regex _chronologyRegex = new Regex("\\b(19|20)\\d{2}\\b", RegexOptions.IgnoreCase);

	private static readonly string[] _explainedGapKeywords = new string[10] { "research", "independent", "project", "self-employed", "education", "course", "health problem", "study", "freelance", "reposition" };

	public string SectionName => "Consistency";

	public int MaxScore => 25;

	public ConsistencyAnalyzer(IEnumerable<string> summaryHeaders, IEnumerable<string> experienceHeaders, IEnumerable<string> skillsHeaders)
	{
		_summaryHeaders = summaryHeaders;
		_experienceHeaders = experienceHeaders;
		_skillsHeaders = skillsHeaders;
	}

	public AnalyzerResult Analyze(string cvText, string? jobDescription = null)
	{
		AnalyzerResult analyzerResult = new AnalyzerResult
		{
			SectionName = SectionName,
			MaxScore = MaxScore
		};
		string lowerText = cvText.ToLowerInvariant();
		string summaryText = ExtractSectionAnywhere(lowerText, _summaryHeaders);
		string experienceText = ExtractSectionAnywhere(lowerText, _experienceHeaders);
		string skillsText = ExtractSectionAnywhere(lowerText, _skillsHeaders);
		bool flag = CheckSectionOrderPenalty(lowerText);
		AnalyzeSummaryExperienceConsistency(analyzerResult, summaryText, experienceText);
		AnalyzeTechConsistency(analyzerResult, skillsText, experienceText);
		AnalyzeChronology(analyzerResult, cvText);
		if (flag)
		{
			int num = 3;
			analyzerResult.Score -= num;
			analyzerResult.Suggestions.Add("Section order is non-standard. Recommended order: Summary → Experience → Skills.");
			analyzerResult.Feedbacks.Add($"⚠\ufe0f Non-standard section order detected (-{num} pts).");
		}
		analyzerResult.Score = Math.Max(0, Math.Min(analyzerResult.Score, MaxScore));
		analyzerResult.IsPassed = analyzerResult.Score >= MaxScore / 2;
		return analyzerResult;
	}

	private static string ExtractSectionAnywhere(string lowerText, IEnumerable<string> targetHeaders)
	{
		string[] source = new string[27]
		{
			"summary", "professional summary", "career summary", "about me", "profile", "experience", "professional experience", "work experience", "employment history", "skills",
			"technical skills", "core technical skills", "core skills", "education", "academic background", "bachelor's degree", "graduate", "certifications", "certificates", "languages",
			"projects", "personal projects", "references", "awards", "publications", "volunteer", "interests"
		};
		int startIndex = (from h in targetHeaders
			select lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase) into i
			where i >= 0
			select i).DefaultIfEmpty(-1).Min();
		if (startIndex < 0)
		{
			return string.Empty;
		}
		int num = (from h in source
			where !targetHeaders.Contains(h, StringComparer.OrdinalIgnoreCase)
			select lowerText.IndexOf(h, startIndex + 1, StringComparison.OrdinalIgnoreCase) into i
			where i > startIndex
			select i).DefaultIfEmpty(lowerText.Length).Min();
		return lowerText.Substring(startIndex, num - startIndex);
	}

	private bool CheckSectionOrderPenalty(string lowerText)
	{
		int num = (from h in _summaryHeaders
			select lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase) into i
			where i >= 0
			select i).DefaultIfEmpty(int.MaxValue).Min();
		int num2 = (from h in _experienceHeaders
			select lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase) into i
			where i >= 0
			select i).DefaultIfEmpty(int.MaxValue).Min();
		int num3 = (from h in _skillsHeaders
			select lowerText.IndexOf(h, StringComparison.OrdinalIgnoreCase) into i
			where i >= 0
			select i).DefaultIfEmpty(int.MaxValue).Min();
		if (num == int.MaxValue || num2 == int.MaxValue || num3 == int.MaxValue)
		{
			return false;
		}
		return num > num2;
	}

	private void AnalyzeSummaryExperienceConsistency(AnalyzerResult result, string summaryText, string experienceText)
	{
		if (summaryText.IsNullOrWhiteSpace())
		{
			result.MissingItems.Add("Career Summary");
			result.Suggestions.Add("No career summary found. Add a professional summary that reflects your experience.");
			return;
		}
		if (experienceText.IsNullOrWhiteSpace())
		{
			result.MissingItems.Add("Experience Section");
			result.Suggestions.Add("No experience section found. Add your work history.");
			return;
		}
		HashSet<string> hashSet = summaryText.Tokenize(StopWords.Default);
		HashSet<string> second = experienceText.Tokenize(StopWords.Default);
		double num = ((hashSet.Count > 0) ? ((double)hashSet.Intersect(second).Count() / (double)hashSet.Count) : 0.0);
		if (num >= 0.45)
		{
			result.Score += 10;
			result.Feedbacks.Add("✓ Career summary strongly reflects the experience section.");
		}
		else if (num >= 0.25)
		{
			result.Score += 6;
			result.Feedbacks.Add("✓ Career summary partially matches experience section.");
			result.Suggestions.Add("Align your summary more closely with your actual experience.");
		}
		else if (num >= 0.1)
		{
			result.Score += 3;
			result.Suggestions.Add("Your career summary and experience section have low overlap.");
		}
		else
		{
			result.MissingItems.Add("Summary-Experience Alignment");
			result.Suggestions.Add("Career summary does not reflect the experience section.");
		}
	}

	private void AnalyzeTechConsistency(AnalyzerResult result, string skillsText, string experienceText)
	{
		if (skillsText.IsNullOrWhiteSpace())
		{
			result.MissingItems.Add("Skills Section");
			result.Suggestions.Add("No skills section found. List your technical skills and tools.");
			return;
		}
		HashSet<string> hashSet = skillsText.Tokenize(StopWords.Default);
		if (!hashSet.Any())
		{
			result.Suggestions.Add("No recognizable skills found in the skills section.");
			return;
		}
		if (experienceText.IsNullOrWhiteSpace())
		{
			result.Score += 3;
			result.Suggestions.Add("Skills listed but no experience section found to verify usage.");
			return;
		}
		HashSet<string> second = experienceText.Tokenize(StopWords.Default);
		List<string> list = hashSet.Intersect(second).ToList();
		List<string> source = hashSet.Except(second).ToList();
		double num = ((hashSet.Count > 0) ? ((double)list.Count / (double)hashSet.Count) : 0.0);
		if (num >= 0.7)
		{
			result.Score += 10;
			result.Feedbacks.Add($"✓ Most listed skills are backed by experience ({list.Count}/{hashSet.Count}).");
		}
		else if (num >= 0.4)
		{
			result.Score += 6;
			result.Feedbacks.Add($"✓ Some skills are mentioned in experience ({list.Count}/{hashSet.Count}).");
			if (source.Any())
			{
				result.Suggestions.Add("These skills are listed but not mentioned in experience: " + string.Join(", ", source.Take(5)) + ".");
			}
		}
		else
		{
			result.Score += 2;
			result.MissingItems.Add("Skills-Experience Alignment");
			result.Suggestions.Add("Most listed skills are not mentioned in your experience. Describe how you used: " + string.Join(", ", source.Take(5)) + ".");
		}
	}

	private void AnalyzeChronology(AnalyzerResult result, string cvText)
	{
		string lowerText = cvText.ToLowerInvariant();
		List<int> list = (from m in _chronologyRegex.Matches(cvText)
			select int.Parse(m.Value) into y
			where y >= 1980 && y <= DateTime.Now.Year
			orderby y
			select y).ToList();
		if (list.Count < 2)
		{
			result.Suggestions.Add("Add employment dates to your experience entries.");
			return;
		}
		bool flag = list.Any((int y) => y >= DateTime.Now.Year - 1);
		bool flag2 = false;
		for (int num = 1; num < list.Count; num++)
		{
			if (list[num] - list[num - 1] > 2)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag2 & flag)
		{
			result.Score += 11;
			result.Feedbacks.Add("✓ Employment history appears chronologically consistent.");
		}
		else if (flag2)
		{
			if (_explainedGapKeywords.Any((string kw) => lowerText.Contains(kw)))
			{
				result.Score += 8;
				result.Feedbacks.Add("✓ Employment gap detected but a valid explanation (Research/Project) was found.");
			}
			else
			{
				result.Score++;
				result.Suggestions.Add("Large employment gap detected without explanation. Consider explaining the gap if you were doing research or personal projects.");
			}
		}
		else
		{
			result.Score += 5;
		}
	}
}
