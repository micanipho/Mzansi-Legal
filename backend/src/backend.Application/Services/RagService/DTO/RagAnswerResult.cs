using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace backend.Services.RagService.DTO;

/// <summary>
/// Output DTO returned by <see cref="IRagAppService.AskAsync"/>.
/// Contains the generated answer, structured citations, and traceability identifiers.
/// When no grounded answer is returned, the answer ID is null even though the question itself
/// may still be recorded for authenticated users.
/// </summary>
public class RagAnswerResult
{
    /// <summary>
    /// The returned answer text, clarification lead-in, or deterministic limitation message.
    /// </summary>
    public string AnswerText { get; set; }

    /// <summary>
    /// <c>true</c> when the response is not a grounded final legal answer.
    /// This remains <c>true</c> for clarification and insufficient modes for backward compatibility.
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
    /// <c>null</c> when no grounded answer record was created.
    /// </summary>
    public Guid? AnswerId { get; set; }

    /// <summary>
    /// ID of the persisted <see cref="backend.Domains.QA.Conversation"/> linked to this exchange.
    /// <c>null</c> when no conversation was persisted or surfaced to the client.
    /// </summary>
    public Guid? ConversationId { get; set; }

    /// <summary>
    /// ISO 639-1 code of the detected input language (e.g. "zu", "st", "af", "en").
    /// Defaults to "en" when language detection is unavailable.
    /// </summary>
    [JsonProperty("detectedLanguageCode")]
    public string DetectedLanguageCode { get; set; } = "en";

    /// <summary>
    /// Structured response posture returned by the retrieval pipeline.
    /// </summary>
    [JsonProperty("answerMode")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public RagAnswerMode AnswerMode { get; set; } = RagAnswerMode.Insufficient;

    /// <summary>
    /// Retrieval-derived confidence band for the returned response mode.
    /// </summary>
    [JsonProperty("confidenceBand")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public RagConfidenceBand ConfidenceBand { get; set; } = RagConfidenceBand.Low;

    /// <summary>
    /// Follow-up question to ask the user when the answer mode is clarification.
    /// </summary>
    [JsonProperty("clarificationQuestion")]
    public string ClarificationQuestion { get; set; }

    /// <summary>
    /// <c>true</c> when the system detected urgent risk or deadline indicators and wants the client
    /// to surface immediate-help language even if some legal grounding is available.
    /// </summary>
    [JsonProperty("requiresUrgentAttention")]
    public bool RequiresUrgentAttention { get; set; }
}
