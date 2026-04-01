using Abp.Application.Services;
using Abp.Authorization;
using Ardalis.GuardClauses;
using backend.Services.ChunkEnrichmentService.DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace backend.Services.ChunkEnrichmentService;

/// <summary>
/// Calls OpenAI chat completions to extract keywords and a topic classification
/// for a legislation chunk. All failures degrade gracefully to fallback values
/// so ETL ingestion is never blocked by enrichment errors.
/// </summary>
[AbpAuthorize]
public class ChunkEnrichmentAppService : ApplicationService, IChunkEnrichmentAppService
{
    private const int MaxInputCharacters = 3_000;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _apiKey;
    private readonly string _enrichmentModel;

    /// <summary>
    /// Initialises the service and validates required OpenAI configuration.
    /// </summary>
    public ChunkEnrichmentAppService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        Guard.Against.Null(configuration, nameof(configuration));
        Guard.Against.Null(httpClientFactory, nameof(httpClientFactory));

        _apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI:ApiKey must be set in appsettings.json. " +
                "Store your real key in appsettings.Development.json (gitignored).");
        }

        _enrichmentModel = configuration["OpenAI:EnrichmentModel"];
        if (string.IsNullOrWhiteSpace(_enrichmentModel))
        {
            throw new InvalidOperationException(
                "OpenAI:EnrichmentModel must be set in appsettings.json " +
                "(e.g., \"gpt-4o-mini\").");
        }

        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Extracts semantic metadata from a chunk using a lightweight LLM call.
    /// Returns fallback values on any error, including invalid JSON responses.
    /// </summary>
    public async Task<ChunkEnrichmentResult> EnrichAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return ChunkEnrichmentResult.Fallback();
        }

        var truncatedContent = Truncate(content, MaxInputCharacters);
        var requestBody = BuildRequestBody(truncatedContent);

        try
        {
            using var client = _httpClientFactory.CreateClient("OpenAI");
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", _apiKey) },
                Content = JsonContent.Create(requestBody)
            };

            using var response = await client.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<OpenAiChatResponse>();
            var json = StripMarkdownFence(body?.Choices?.FirstOrDefault()?.Message?.Content);
            if (string.IsNullOrWhiteSpace(json))
            {
                return ChunkEnrichmentResult.Fallback();
            }

            var parsed = JsonSerializer.Deserialize<EnrichmentPayload>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (parsed == null)
            {
                return ChunkEnrichmentResult.Fallback();
            }

            var keywords = parsed.Keywords?
                .Where(k => !string.IsNullOrWhiteSpace(k))
                .Select(k => k.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToArray() ?? Array.Empty<string>();

            var joinedKeywords = string.Join(",", keywords);
            var topic = string.IsNullOrWhiteSpace(parsed.Topic) ? "Unknown" : parsed.Topic.Trim();

            return new ChunkEnrichmentResult
            {
                Keywords = Truncate(joinedKeywords, 500),
                TopicClassification = Truncate(topic, 200)
            };
        }
        catch (Exception ex)
        {
            Logger.Warn($"Chunk enrichment failed. Falling back to empty metadata. Reason: {ex.Message}");
            return ChunkEnrichmentResult.Fallback();
        }
    }

    private object BuildRequestBody(string content)
    {
        return new OpenAiChatRequest
        {
            Model = _enrichmentModel,
            Temperature = 0,
            Messages =
            [
                new ChatMessage
                {
                    Role = "system",
                    Content =
                        "Extract 3-5 legal keywords and a topic classification from the provided " +
                        "South African legislation excerpt. Respond ONLY with valid JSON in the " +
                        "shape {\"keywords\":[\"...\"],\"topic\":\"...\"}."
                },
                new ChatMessage
                {
                    Role = "user",
                    Content = $"EXCERPT:\n{content}"
                }
            ]
        };
    }

    /// <summary>
    /// Strips a markdown code fence (```json ... ``` or ``` ... ```) that the model
    /// sometimes wraps around its JSON response despite being instructed not to.
    /// </summary>
    private static string StripMarkdownFence(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        var trimmed = raw.Trim();
        if (!trimmed.StartsWith("```"))
            return trimmed;

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0)
            return trimmed;

        var inner = trimmed.Substring(firstNewline + 1);
        if (inner.EndsWith("```"))
            inner = inner.Substring(0, inner.Length - 3);

        return inner.Trim();
    }

    private static string Truncate(string content, int maxLength)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
        {
            return content;
        }

        return content.Substring(0, maxLength);
    }

    private sealed class OpenAiChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public ChatMessage[] Messages { get; set; }

        [JsonPropertyName("temperature")]
        public int Temperature { get; set; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    private sealed class OpenAiChatResponse
    {
        [JsonPropertyName("choices")]
        public OpenAiChoice[] Choices { get; set; }
    }

    private sealed class OpenAiChoice
    {
        [JsonPropertyName("message")]
        public OpenAiMessage Message { get; set; }
    }

    private sealed class OpenAiMessage
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    private sealed class EnrichmentPayload
    {
        [JsonPropertyName("keywords")]
        public string[] Keywords { get; set; }

        [JsonPropertyName("topic")]
        public string Topic { get; set; }
    }
}
