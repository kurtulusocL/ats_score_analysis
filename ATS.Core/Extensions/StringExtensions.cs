using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ATS.Core.Extensions;

public static class StringExtensions
{
	private static readonly Regex _termRegex = new Regex("\\.?[\\p{L}\\p{N}]+(?:[.\\-/][\\p{L}\\p{N}]+)*(?:\\+\\+|#|\\+)?", RegexOptions.Compiled);

	private static readonly HashSet<string> _singleLetterTerms = new HashSet<string> { "c", "r" };

	public static string ToLower(this string text)
	{
		return text.ToLowerInvariant();
	}

	public static bool ContainsIgnoreCase(this string text, string value)
	{
		return text.Contains(value, StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsNullOrWhiteSpace(this string? text)
	{
		return string.IsNullOrWhiteSpace(text);
	}

	public static string[] SplitWords(this string text)
	{
		return text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
	}

	public static string[] SplitLines(this string text)
	{
		return text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
	}

	public static HashSet<string> Tokenize(this string text, HashSet<string> stopWords)
	{
		return new HashSet<string>((from w in Regex.Split(text.ToLowerInvariant(), "\\W+")
			where w.Length > 3
			select w).Except(stopWords));
	}

	public static HashSet<string> TokenizeTerms(this string text, HashSet<string> stopWords)
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (Match item in _termRegex.Matches(text.ToLowerInvariant()))
		{
			string value = item.Value;
			if ((value.Length >= 2 || _singleLetterTerms.Contains(value)) && value.Any(char.IsLetter) && !stopWords.Contains(value))
			{
				hashSet.Add(value);
			}
		}
		return hashSet;
	}

	public static string ExtractSection(this string text, IEnumerable<string> startHeaders, IEnumerable<string>? endHeaders = null)
	{
		int startIndex = (from h in startHeaders
			select text.IndexOf(h, StringComparison.OrdinalIgnoreCase) into i
			where i >= 0
			select i).DefaultIfEmpty(-1).Min();
		if (startIndex < 0)
		{
			return string.Empty;
		}
		int num = text.Length;
		if (endHeaders != null)
		{
			int num2 = (from h in endHeaders
				select text.IndexOf(h, startIndex + 1, StringComparison.OrdinalIgnoreCase) into i
				where i > startIndex
				select i).DefaultIfEmpty(text.Length).Min();
			num = num2;
		}
		return text.Substring(startIndex, num - startIndex);
	}

	public static string TruncateAt(this string text, int maxLength)
	{
		return (text.Length <= maxLength) ? text : (text.Substring(0, maxLength) + "...");
	}

	public static int WordCount(this string text)
	{
		return text.SplitWords().Length;
	}

	public static bool HasMinimumWordCount(this string text, int min)
	{
		return text.WordCount() >= min;
	}
}
