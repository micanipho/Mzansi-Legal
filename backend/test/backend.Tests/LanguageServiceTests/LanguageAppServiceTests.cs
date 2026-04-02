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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace backend.Tests.LanguageServiceTests;

public class LanguageAppServiceTests
{
    [Fact]
    public void Constructor_ValidConfig_ConstructsWithoutError()
    {
        Should.NotThrow(() => new LanguageAppService(CreateFactory(new StubHandler("en")), BuildConfig()));
    }

    [Theory]
    [InlineData("zu", Language.Zulu)]
    [InlineData("st", Language.Sesotho)]
    [InlineData("af", Language.Afrikaans)]
    [InlineData("en", Language.English)]
    [InlineData("xh", Language.English)]
    public async Task DetectLanguageAsync_MapsSupportedIsoCodes(string rawCode, Language expectedLanguage)
    {
        var service = CreateService(new StubHandler(rawCode));

        var result = await service.DetectLanguageAsync("sample text");

        result.ShouldBe(expectedLanguage);
    }

    [Fact]
    public async Task DetectLanguageAsync_OnFailure_DefaultsToEnglish()
    {
        var service = CreateService(new ThrowingHandler());

        var result = await service.DetectLanguageAsync("umbuzo");

        result.ShouldBe(Language.English);
    }

    [Fact]
    public async Task TranslateToEnglishAsync_WhenSourceLanguageIsEnglish_ReturnsOriginalWithoutCallingOpenAi()
    {
        var handler = new CountingHandler("unused");
        var service = CreateService(handler);

        var result = await service.TranslateToEnglishAsync("Can my landlord evict me?", Language.English);

        result.ShouldBe("Can my landlord evict me?");
        handler.CallCount.ShouldBe(0);
    }

    [Fact]
    public async Task TranslateToEnglishAsync_ForSupportedNonEnglishLanguage_ReturnsTrimmedTranslation()
    {
        var service = CreateService(new StubHandler(" Can my landlord evict me without a court order? "));

        var result = await service.TranslateToEnglishAsync(
            "Ingabe umnikazi wendlu angangixosha?",
            Language.Zulu);

        result.ShouldBe("Can my landlord evict me without a court order?");
    }

    [Fact]
    public async Task TranslateToEnglishAsync_OnFailure_ReturnsOriginalText()
    {
        var service = CreateService(new ThrowingHandler());

        var result = await service.TranslateToEnglishAsync(
            "Kan my verhuurder my uitsit?",
            Language.Afrikaans);

        result.ShouldBe("Kan my verhuurder my uitsit?");
    }

    private static LanguageAppService CreateService(HttpMessageHandler handler) =>
        new(CreateFactory(handler), BuildConfig());

    private static IHttpClientFactory CreateFactory(HttpMessageHandler handler)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("OpenAI").Returns(_ => new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/")
        });

        return factory;
    }

    private static IConfiguration BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
            {
                new("OpenAI:ApiKey", "sk-test"),
                new("OpenAI:ChatModel", "gpt-4o")
            })
            .Build();
    }

    private class StubHandler : HttpMessageHandler
    {
        private readonly string _content;

        public StubHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = _content
                        }
                    }
                }
            });

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class CountingHandler : StubHandler
    {
        public CountingHandler(string content)
            : base(content)
        {
        }

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated OpenAI outage.");
        }
    }
}
