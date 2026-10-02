using System.Collections.Generic;
using System.Linq;

namespace ATS.Core.Constants;

public static class StopWords
{
	public static readonly HashSet<string> Default = new HashSet<string>
	{
		"with", "that", "this", "from", "have", "been", "will", "were", "they", "their",
		"them", "when", "what", "which", "while", "where", "also", "into", "over", "such",
		"some", "than", "then", "your", "more", "other", "about", "using", "used", "work",
		"worked", "team", "able", "good", "high", "large", "well", "very", "both", "must",
		"should", "would", "could", "shall", "need", "required", "experience", "years", "knowledge", "strong",
		"ability", "skills", "seeking", "passionate", "looking", "player", "alongside", "highly", "skilled", "looking",
		"across", "assist", "benefits", "looking", "plus", "excellent", "working", "proven", "solid", "dedicated",
		"proactive", "curious", "attention", "detail", "mindset", "self", "manage", "priorities", "directly", "different"
	};

	public static readonly HashSet<string> Short = new HashSet<string>
	{
		"a", "an", "the", "and", "or", "of", "to", "in", "on", "at",
		"for", "by", "is", "are", "be", "as", "we", "you", "our", "us",
		"it", "if", "so", "do", "no", "not", "all", "any", "can", "has",
		"who", "but", "was", "his", "her", "its", "may", "per", "via", "etc",
		"new", "own", "one", "two", "how", "why"
	};

	public static readonly HashSet<string> Terms = new HashSet<string>(Default.Concat(Short));
}
