

namespace ATS.Core.Helpers
{
    public sealed record EnglishTextAnalysis
    (
        int WordCount,
        double EnglishFunctionWordRatio,
        double TurkishFunctionWordRatio,
        double TurkishLetterRatio,
        bool IsLikelyEnglish
    );
}
