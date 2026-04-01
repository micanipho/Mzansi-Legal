#nullable enable
using backend.Services.ChunkEnrichmentService;
using backend.Services.ChunkEnrichmentService.DTO;
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

namespace backend.Tests.Services;

/// <summary>
/// Unit tests for ChunkEnrichmentAppService JSON parsing and fallback behaviour.
/// </summary>
public class ChunkEnrichmentServiceTests
{
    [Fact]
    public async Task EnrichAsync_ValidJsonResponse_ReturnsKeywordsAndTopic()
    {
        var service = CreateService(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"choices\":[{\"message\":{\"content\":\"{\\\"keywords\\\":[\\\"employment\\\",\\\"dismissal\\\"],\\\"topic\\\":\\\"Labour Relations\\\"}\"}}]}",
                Encoding.UTF8,
                "application/json")
        });

        var result = await service.EnrichAsync("Dismissal disputes and labour rights.");

        result.Keywords.ShouldBe("employment,dismissal");
        result.TopicClassification.ShouldBe("Labour Relations");
    }

    [Fact]
    public async Task EnrichAsync_HttpFailure_ReturnsFallbackWithoutThrowing()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("OpenAI").Returns(_ => new HttpClient(new ThrowingHandler()));

        var service = new ChunkEnrichmentAppService(BuildConfig(), factory);
        var result = await service.EnrichAsync("Any content");

        result.ShouldBeEquivalentTo(ChunkEnrichmentResult.Fallback());
    }

    private static ChunkEnrichmentAppService CreateService(HttpResponseMessage responseMessage)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("OpenAI").Returns(_ => new HttpClient(new StubHandler(responseMessage))
        {
            BaseAddress = new Uri("https://api.openai.com/")
        });

        return new ChunkEnrichmentAppService(BuildConfig(), factory);
    }

    private static IConfiguration BuildConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("OpenAI:ApiKey", "sk-test"),
                new KeyValuePair<string, string?>("OpenAI:EnrichmentModel", "gpt-4o-mini")
            })
            .Build();
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _responseMessage;

        public StubHandler(HttpResponseMessage responseMessage)
        {
            _responseMessage = responseMessage;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseMessage);
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
