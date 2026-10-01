using ATS.Core.Extensions;
using ATS.Infrastructure.Concrete.ServiceManagers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text;

namespace ATS.Tests
{
    public class RapidApiTranslatorTests
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

        private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
        {
            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                return Task.FromResult(respond());
            }
        }

        private static RapidApiTranslator CreateTranslator(StubHandler handler)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["RapidApi:Host"] = "translator.example.test",
                    ["RapidApi:Key"] = "test-key",
                    ["RapidApi:TranslatePath"] = "/v2"
                })
                .Build();

            return new RapidApiTranslator(new HttpClient(handler), configuration, NullLogger<RapidApiTranslator>.Instance);
        }

        [Fact]
        public async Task GetAnalysisTextAsync_ReturnsEnglishTextUnchanged_WithoutCallingTheService()
        {
            var handler = new StubHandler(() => throw new InvalidOperationException("must not be called"));

            var result = await CreateTranslator(handler).GetAnalysisTextAsync(EnglishText);

            Assert.Equal(EnglishText, result);
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task GetAnalysisTextAsync_TranslatesTurkishText_ThroughTheService()
        {
            var handler = new StubHandler(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"data\":{\"translations\":[{\"translatedText\":\"Translated text\",\"detectedSourceLanguage\":\"tr\"}]}}",
                    Encoding.UTF8, "application/json")
            });

            var result = await CreateTranslator(handler).GetAnalysisTextAsync(TurkishText);

            Assert.Equal("Translated text", result);
            Assert.Equal(1, handler.CallCount);
        }

        [Fact]
        public async Task GetAnalysisTextAsync_ThrowsTranslationUnavailable_WhenTheServiceFailsForTurkishText()
        {
            var handler = new StubHandler(() => new HttpResponseMessage(HttpStatusCode.TooManyRequests));

            await Assert.ThrowsAsync<TranslationUnavailableException>(() =>
                CreateTranslator(handler).GetAnalysisTextAsync(TurkishText));
        }
    }
}
