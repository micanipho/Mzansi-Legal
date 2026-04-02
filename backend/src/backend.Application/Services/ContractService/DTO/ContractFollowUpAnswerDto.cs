using backend.Services.RagService.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace backend.Services.ContractService.DTO;

/// <summary>
/// Contract-aware follow-up answer grounded in the saved contract text and legislation context.
/// </summary>
public class ContractFollowUpAnswerDto
{
    public string AnswerText { get; set; }

    [JsonProperty("answerMode")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public RagAnswerMode AnswerMode { get; set; } = RagAnswerMode.Insufficient;

    [JsonProperty("confidenceBand")]
    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public RagConfidenceBand ConfidenceBand { get; set; } = RagConfidenceBand.Low;

    [JsonProperty("requiresUrgentAttention")]
    public bool RequiresUrgentAttention { get; set; }

    [JsonProperty("detectedLanguageCode")]
    public string DetectedLanguageCode { get; set; } = "en";

    public List<string> ContractExcerpts { get; set; } = new();

    public List<RagCitationDto> Citations { get; set; } = new();
}
