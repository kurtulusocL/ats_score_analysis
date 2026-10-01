using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class EnglishTextDetectorTests
    {
        private const string EnglishText =
        "We are looking for a developer with experience in the design of REST APIs and the use of SQL databases. " +
        "You will work with our team to build and maintain the services that are used by customers across the world. " +
        "The candidate should have knowledge of C# and the .NET platform.";

        private const string TurkishText =
            "Yazılım geliştirici olarak çalışıyorum ve REST servisleri ile SQL veritabanları üzerinde deneyimim var. " +
            "Ekibimizle birlikte müşteriler için güvenilir ve ölçeklenebilir çözümler geliştiriyorum. " +
            "Bu görevde C# ve .NET platformunu yoğun olarak kullandım. " +
            "Ayrıca ekip içinde kod incelemeleri yaptım ve yeni çalışanlara rehberlik ettim.";

        [Fact]
        public void IsLikelyEnglish_ReturnsTrue_ForEnglishText()
        {
            Assert.True(EnglishTextDetector.IsLikelyEnglish(EnglishText));
        }

        [Fact]
        public void IsLikelyEnglish_ReturnsFalse_ForTurkishText()
        {
            Assert.False(EnglishTextDetector.IsLikelyEnglish(TurkishText));
        }

        [Fact]
        public void IsLikelyEnglish_ReturnsTrue_ForEnglishTextThatContainsATurkishName()
        {
            var text = "Kurtuluş Öcal\nSenior Software Developer\n" + EnglishText;

            Assert.True(EnglishTextDetector.IsLikelyEnglish(text));
        }

        [Fact]
        public void IsLikelyEnglish_ReturnsFalse_ForTurkishTextWrittenWithoutDiacritics()
        {
            const string text =
                "Yazilim gelistirici olarak calisiyorum ve REST servisleri ile SQL veritabanlari uzerinde deneyimim var. " +
                "Ekibimizle birlikte musteriler icin guvenilir ve olceklenebilir cozumler gelistiriyorum. " +
                "Bu gorevde C# ve .NET platformunu yogun olarak kullandim. " +
                "Ayrica ekip icinde kod incelemeleri yaptim ve yeni calisanlara rehberlik ettim.";

            Assert.False(EnglishTextDetector.IsLikelyEnglish(text));
        }

        [Fact]
        public void IsLikelyEnglish_ReturnsFalse_ForMixedEnglishAndTurkishText()
        {
            Assert.False(EnglishTextDetector.IsLikelyEnglish(EnglishText + "\n" + TurkishText));
        }

        [Fact]
        public void IsLikelyEnglish_ReturnsFalse_ForSpanishText()
        {
            const string text =
                "Soy desarrollador de software con experiencia en el diseño de servicios y el uso de bases de datos. " +
                "Trabajo con un equipo para construir y mantener las aplicaciones que utilizan los clientes en todo el mundo. " +
                "Busco una nueva oportunidad para seguir creciendo como profesional.";

            Assert.False(EnglishTextDetector.IsLikelyEnglish(text));
        }

        [Fact]
        public void IsLikelyEnglish_ReturnsFalse_ForTextThatIsTooShortToJudge()
        {
            Assert.False(EnglishTextDetector.IsLikelyEnglish("Hello world"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void IsLikelyEnglish_ReturnsFalse_ForBlankText(string? text)
        {
            Assert.False(EnglishTextDetector.IsLikelyEnglish(text));
        }

        [Fact]
        public void Analyze_ReportsTheWordCount()
        {
            var analysis = EnglishTextDetector.Analyze("one two three");

            Assert.Equal(3, analysis.WordCount);
            Assert.False(analysis.IsLikelyEnglish);
        }
    }
}
