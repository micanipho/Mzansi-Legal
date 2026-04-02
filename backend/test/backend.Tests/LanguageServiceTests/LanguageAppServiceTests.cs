#nullable enable
using backend.Domains.QA;
using backend.Services.LanguageService;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.LanguageServiceTests;

public class LanguageAppServiceTests
{
    [Fact]
    public void Constructor_ValidConfig_ConstructsWithoutError()
    {
        var config = BuildConfig("sk-test-key", "gpt-4o");

        Should.NotThrow(() => new LanguageAppService(BuildFactory(new StubHttpMessageHandler("en")), config));
    }

    [Fact]
    public async Task DetectLanguageAsync_RecognizedIsoCode_ReturnsZuluAndUsesConstrainedPrompt()
    {
        var handler = new StubHttpMessageHandler("zu");
        var service = new LanguageAppService(BuildFactory(handler), BuildConfig("sk-test-key", "gpt-4o"));

        var result = await service.DetectLanguageAsync("Ingabe umnikazi wendlu angangixosha?");

        result.ShouldBe(Language.Zulu);
        handler.CallCount.ShouldBe(1);
        handler.LastRequestBody.ShouldContain("en, zu, st, af");
        handler.LastRequestBody.ShouldContain("Ingabe umnikazi wendlu angangixosha?");
    }

    [Fact]
    public async Task DetectLanguageAsync_UnknownCode_FallsBackToEnglish()
    {
        var service = new LanguageAppService(
            BuildFactory(new StubHttpMessageHandler("xh")),
            BuildConfig("sk-test-key", "gpt-4o"));

        var result = await service.DetectLanguageAsync("Molo");

        result.ShouldBe(Language.English);
    }

    [Fact]
    public async Task TranslateToEnglishAsync_EnglishInput_ReturnsOriginalTextWithoutCallingOpenAi()
    {
        var handler = new StubHttpMessageHandler("unused");
        var service = new LanguageAppService(BuildFactory(handler), BuildConfig("sk-test-key", "gpt-4o"));

        var result = await service.TranslateToEnglishAsync(
            "Can my landlord evict me without a court order?",
            Language.English);

        result.ShouldBe("Can my landlord evict me without a court order?");
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task TranslateToEnglishAsync_ZuluInput_ReturnsTrimmedTranslationAndUsesLanguageName()
    {
        var handler = new StubHttpMessageHandler(" Can my landlord evict me? ");
        var service = new LanguageAppService(BuildFactory(handler), BuildConfig("sk-test-key", "gpt-4o"));

        var result = await service.TranslateToEnglishAsync(
            "Ingabe umnikazi wendlu angangixosha?",
            Language.Zulu);

        result.ShouldBe("Can my landlord evict me?");
        handler.CallCount.ShouldBe(1);
        handler.LastRequestBody.ShouldContain("Translate the following isiZulu text to English");
        handler.LastRequestBody.ShouldContain("Ingabe umnikazi wendlu angangixosha?");
    }

    [Fact]
    public async Task TranslateToEnglishAsync_OnHttpFailure_ReturnsOriginalText()
    {
        var service = new LanguageAppService(
            BuildFactory(new StubHttpMessageHandler(new HttpRequestException("boom"))),
            BuildConfig("sk-test-key", "gpt-4o"));

        var result = await service.TranslateToEnglishAsync(
            "Ingabe umnikazi wendlu angangixosha?",
            Language.Zulu);

        result.ShouldBe("Ingabe umnikazi wendlu angangixosha?");
    }

    private static IConfiguration BuildConfig(string? apiKey, string? chatModel)
    {
        var pairs = new List<KeyValuePair<string, string?>>();
        if (apiKey is not null)
        {
            pairs.Add(new KeyValuePair<string, string?>("OpenAI:ApiKey", apiKey));
        }

        if (chatModel is not null)
        {
            pairs.Add(new KeyValuePair<string, string?>("OpenAI:ChatModel", chatModel));
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(pairs)
            .Build();
    }

    private static IHttpClientFactory BuildFactory(HttpMessageHandler handler)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("OpenAI").Returns(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        });
        return factory;
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string? _assistantContent;
        private readonly Exception? _exception;

        public StubHttpMessageHandler(string assistantContent)
        {
            _assistantContent = assistantContent;
        }

        public StubHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        public int CallCount { get; private set; }

        public string LastRequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequestBody = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync();

            if (_exception is not null)
            {
                throw _exception;
            }

            var body = $"{{\"choices\":[{{\"message\":{{\"content\":\"{_assistantContent}\"}}}}]}}";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }
    }
}
