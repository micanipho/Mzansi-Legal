using Abp.Domain.Services;
using Ardalis.GuardClauses;
using backend.Services.EmbeddingService.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace backend.Services.EmbeddingService;

/// <summary>
/// Calls the OpenAI embeddings REST API to convert a text string into a
/// 1,536-dimensional float vector for semantic search.
/// Text exceeding 30,000 characters is silently truncated before the API call.
/// Domain service - no authorization or proxying required.
/// </summary>
public class EmbeddingAppService : DomainService, IEmbeddingAppService
{
    private const int MaxInputCharacters = 30_000;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _embeddingModel;

    /// <summary>
    /// Initialises the service and validates required configuration keys.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>OpenAI:ApiKey</c> or <c>OpenAI:EmbeddingModel</c> is missing or empty
    /// in application configuration.
    /// </exception>
    public EmbeddingAppService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        Guard.Against.Null(configuration, nameof(configuration));
        Guard.Against.Null(httpClientFactory, nameof(httpClientFactory));

        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException(
                "OpenAI:ApiKey must be set in appsettings.json. " +
                "Store your real key in appsettings.Development.json (gitignored).");

        _embeddingModel = configuration["OpenAI:EmbeddingModel"];
        if (string.IsNullOrWhiteSpace(_embeddingModel))
            throw new InvalidOperationException(
                "OpenAI:EmbeddingModel must be set in appsettings.json " +
                "(e.g., \"text-embedding-ada-002\").");

        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Generates a 1,536-dimensional embedding vector for the provided text.
    /// Text exceeding 30,000 characters is silently truncated before the API call.
    /// </summary>
    /// <param name="text">Plain-text content to embed. Must not be null or whitespace.</param>
    /// <returns>
    /// An <see cref="EmbeddingResult"/> with the float[1536] vector and diagnostic metadata.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when text is null or whitespace.</exception>
    /// <exception cref="HttpRequestException">Propagated on network or non-2xx API response.</exception>
    public async Task<EmbeddingResult> GenerateEmbeddingAsync(string text)
    {
        Guard.Against.NullOrWhiteSpace(text, nameof(text));

        var truncated = EmbeddingHelper.TruncateToLimit(text, MaxInputCharacters);

        var requestBody = new OpenAiEmbeddingRequest(truncated, _embeddingModel);

        using var client = _httpClientFactory.CreateClient("OpenAI");
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/embeddings")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
            Content = JsonContent.Create(requestBody)
        };

        using var response = await client.SendAsync(requestMessage);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<OpenAiEmbeddingResponse>();

        return new EmbeddingResult
        {
            Vector = body.Data[0].Embedding,
            Model = body.Model,
            InputCharacterCount = truncated.Length
        };
    }

    // ── Private types for OpenAI REST serialisation only ─────────────────────

    /// <summary>Request body sent to POST /v1/embeddings.</summary>
    private sealed record OpenAiEmbeddingRequest(
        [property: JsonPropertyName("input")] string Input,
        [property: JsonPropertyName("model")] string Model);

    /// <summary>Top-level response envelope from POST /v1/embeddings.</summary>
    private sealed record OpenAiEmbeddingResponse(
        [property: JsonPropertyName("data")] OpenAiEmbeddingData[] Data,
        [property: JsonPropertyName("model")] string Model);

    /// <summary>Single embedding entry in the response data array.</summary>
    private sealed record OpenAiEmbeddingData(
        [property: JsonPropertyName("embedding")] float[] Embedding,
        [property: JsonPropertyName("index")] int Index);
}
