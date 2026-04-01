using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Text;

namespace backend.Services.RagService;

/// <summary>
/// Static helper for constructing the three-part RAG prompt sent to the LLM.
/// Owns all prompt-construction constants and the internal <see cref="ScoredChunk"/> record
/// shared between this builder and <see cref="RagAppService"/>.
/// </summary>
public static class RagPromptBuilder
{
    /// <summary>Minimum cosine similarity score for a chunk to be included in the context block.</summary>
    public const float SimilarityThreshold = 0.7f;

    /// <summary>Maximum number of chunks passed to the LLM as context per question.</summary>
    public const int MaxContextChunks = 5;

    /// <summary>LLM temperature used for chat completions. Low value prioritises factual accuracy.</summary>
    public const double ChatTemperature = 0.2;

    /// <summary>
    /// A legislation chunk paired with its cosine similarity score for a specific question.
    /// Immutable record — created during retrieval and passed through to prompt building and persistence.
    /// </summary>
    /// <param name="ChunkId">Primary key of the <see cref="backend.Domains.LegalDocuments.DocumentChunk"/>.</param>
    /// <param name="ActName">Full name of the parent legislation Act.</param>
    /// <param name="SectionNumber">Section identifier (e.g., "§ 26(3)").</param>
    /// <param name="Excerpt">Plain-text content of the chunk, used in the context block.</param>
    /// <param name="Score">Cosine similarity score in [0.7, 1.0] after threshold filtering.</param>
    /// <param name="Vector">The embedding vector; used during scoring, not included in output.</param>
    public record ScoredChunk(
        Guid ChunkId,
        string ActName,
        string SectionNumber,
        string Excerpt,
        float Score,
        float[] Vector);

    /// <summary>
    /// Returns the system message establishing the assistant's identity and citation rules.
    /// The system message is constant and does not vary per question or corpus.
    /// </summary>
    public static string BuildSystemPrompt()
    {
        return
            "You are a South African legal and financial assistant. " +
            "Your role is to help South African residents understand their legal rights and obligations.\n\n" +
            "CRITICAL RULES — follow these without exception:\n" +
            "1. You MUST ONLY answer using information from the legislation context provided below.\n" +
            "2. You MUST ALWAYS include a citation for every claim you make, " +
            "in the format: [Act Name, Section X].\n" +
            "3. If the context does not contain sufficient information to answer the question, " +
            "you MUST respond with exactly: " +
            "\"I don't have enough information in the available legislation to answer this question.\"\n" +
            "4. Do NOT speculate, infer, or draw on general knowledge outside the provided context.\n" +
            "5. Write in plain, accessible English. Avoid legal jargon where a simpler word exists.";
    }

    /// <summary>
    /// Returns a system prompt for the general-knowledge fallback path (no legislation context found).
    /// The LLM may draw on its training knowledge but must not present the answer as legally authoritative.
    /// </summary>
    public static string BuildFallbackSystemPrompt()
    {
        return
            "You are a South African legal and financial assistant. " +
            "Your role is to help South African residents understand their legal rights and obligations.\n\n" +
            "Answer the user's question using your general knowledge of South African law. " +
            "Write in plain, accessible English. Avoid legal jargon where a simpler word exists.";
    }

    /// <summary>
    /// Builds a numbered context block containing each chunk's Act name, section number, and content.
    /// Each block is labelled as <c>[ActName — SectionNumber]</c> followed by the chunk text.
    /// </summary>
    /// <param name="chunks">Scored chunks ordered by relevance descending. Must not be null.</param>
    /// <returns>A formatted multi-line string ready to be embedded in the user prompt.</returns>
    public static string BuildContextBlock(IEnumerable<ScoredChunk> chunks)
    {
        Guard.Against.Null(chunks, nameof(chunks));

        var sb = new StringBuilder();
        foreach (var chunk in chunks)
        {
            sb.AppendLine($"[{chunk.ActName} — {chunk.SectionNumber}]");
            sb.AppendLine(chunk.Excerpt);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    /// <summary>
    /// Builds the user turn combining the retrieved context block and the original question.
    /// </summary>
    /// <param name="questionText">The user's question text. Must not be null or whitespace.</param>
    /// <param name="contextBlock">The formatted context string produced by <see cref="BuildContextBlock"/>.</param>
    /// <returns>The full user message string to pass to the LLM.</returns>
    public static string BuildUserPrompt(string questionText, string contextBlock)
    {
        Guard.Against.NullOrWhiteSpace(questionText, nameof(questionText));
        Guard.Against.Null(contextBlock, nameof(contextBlock));

        return
            $"Legislation context:\n\n{contextBlock}\n\n" +
            $"Question: {questionText}\n\n" +
            "Answer (with citations):";
    }
}
