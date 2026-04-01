using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace backend.Services.RagService.DTO;

/// <summary>
/// Output DTO returned by <see cref="IRagAppService.AskAsync"/>.
/// Contains the generated answer, structured citations, and traceability identifiers.
/// When <see cref="IsInsufficientInformation"/> is <c>true</c>, all other fields are null or empty.
/// </summary>
public class RagAnswerResult
{
    /// <summary>
    /// The AI-generated answer grounded in retrieved legislation.
    /// <c>null</c> when <see cref="IsInsufficientInformation"/> is <c>true</c>.
    /// </summary>
    public string AnswerText { get; set; }

    /// <summary>
    /// <c>true</c> when no legislation chunk scored ≥ 0.7 cosine similarity against the question.
    /// When <c>true</c>, no LLM call was made and no records were persisted.
    /// </summary>
    public bool IsInsufficientInformation { get; set; }

    /// <summary>
    /// Structured list of legislation citations supporting the answer, ordered by relevance score descending.
    /// Empty when <see cref="IsInsufficientInformation"/> is <c>true</c>.
    /// </summary>
    public List<RagCitationDto> Citations { get; set; } = new();

    /// <summary>
    /// IDs of the <see cref="backend.Domains.LegalDocuments.DocumentChunk"/> records passed to the LLM as context.
    /// Empty when <see cref="IsInsufficientInformation"/> is <c>true</c>.
    /// Used for traceability and admin review.
    /// </summary>
    public List<Guid> ChunkIds { get; set; } = new();

    /// <summary>
    /// ID of the persisted <see cref="backend.Domains.QA.Answer"/> entity created for this Q&amp;A exchange.
    /// <c>null</c> when <see cref="IsInsufficientInformation"/> is <c>true</c>.
    /// </summary>
    public Guid? AnswerId { get; set; }

    /// <summary>
    /// ISO 639-1 code of the detected input language (e.g. "zu", "st", "af", "en").
    /// Defaults to "en" when language detection is unavailable.
    /// </summary>
    [JsonProperty("detectedLanguageCode")]
    public string DetectedLanguageCode { get; set; } = "en";
}
