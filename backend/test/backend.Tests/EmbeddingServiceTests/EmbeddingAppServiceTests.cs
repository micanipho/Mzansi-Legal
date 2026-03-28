#nullable enable
using backend.Services.EmbeddingService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace backend.Tests.EmbeddingServiceTests;

/// <summary>
/// Unit tests for EmbeddingAppService constructor validation.
/// These tests do not make live HTTP calls — they verify that configuration
/// errors are surfaced at construction time with descriptive messages.
/// </summary>
public class EmbeddingAppServiceTests
{
    private static IHttpClientFactory BuildFactory() =>
        Substitute.For<IHttpClientFactory>();

    private static IConfiguration BuildConfig(string? apiKey, string? embeddingModel)
    {
        // Use real ConfigurationBuilder so we get accurate IConfiguration semantics
        // (null key = key absent from config; empty string = key present but empty).
        var pairs = new List<KeyValuePair<string, string?>>();
        if (apiKey != null)
            pairs.Add(new KeyValuePair<string, string?>("OpenAI:ApiKey", apiKey));
        if (embeddingModel != null)
            pairs.Add(new KeyValuePair<string, string?>("OpenAI:EmbeddingModel", embeddingModel));

        return new ConfigurationBuilder()
            .AddInMemoryCollection(pairs)
            .Build();
    }

    // ── Valid configuration ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidConfig_ConstructsWithoutError()
    {
        var config = BuildConfig("sk-test-key-abc123", "text-embedding-ada-002");
        Should.NotThrow(() => new EmbeddingAppService(config, BuildFactory()));
    }

    // ── ApiKey validation ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_MissingApiKey_ThrowsInvalidOperationException()
    {
        // ApiKey key absent from config entirely
        var config = BuildConfig(apiKey: null, embeddingModel: "text-embedding-ada-002");
        var ex = Should.Throw<InvalidOperationException>(
            () => new EmbeddingAppService(config, BuildFactory()));
        ex.Message.ShouldContain("OpenAI:ApiKey");
    }

    [Fact]
    public void Constructor_EmptyApiKey_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(apiKey: "", embeddingModel: "text-embedding-ada-002");
        var ex = Should.Throw<InvalidOperationException>(
            () => new EmbeddingAppService(config, BuildFactory()));
        ex.Message.ShouldContain("OpenAI:ApiKey");
    }

    [Fact]
    public void Constructor_WhitespaceApiKey_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(apiKey: "   ", embeddingModel: "text-embedding-ada-002");
        var ex = Should.Throw<InvalidOperationException>(
            () => new EmbeddingAppService(config, BuildFactory()));
        ex.Message.ShouldContain("OpenAI:ApiKey");
    }

    // ── EmbeddingModel validation ─────────────────────────────────────────────

    [Fact]
    public void Constructor_MissingEmbeddingModel_ThrowsInvalidOperationException()
    {
        // EmbeddingModel key absent from config entirely
        var config = BuildConfig(apiKey: "sk-test-key", embeddingModel: null);
        var ex = Should.Throw<InvalidOperationException>(
            () => new EmbeddingAppService(config, BuildFactory()));
        ex.Message.ShouldContain("OpenAI:EmbeddingModel");
    }

    [Fact]
    public void Constructor_EmptyEmbeddingModel_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(apiKey: "sk-test-key", embeddingModel: "");
        var ex = Should.Throw<InvalidOperationException>(
            () => new EmbeddingAppService(config, BuildFactory()));
        ex.Message.ShouldContain("OpenAI:EmbeddingModel");
    }

    [Fact]
    public void Constructor_WhitespaceEmbeddingModel_ThrowsInvalidOperationException()
    {
        var config = BuildConfig(apiKey: "sk-test-key", embeddingModel: "   ");
        var ex = Should.Throw<InvalidOperationException>(
            () => new EmbeddingAppService(config, BuildFactory()));
        ex.Message.ShouldContain("OpenAI:EmbeddingModel");
    }
}
