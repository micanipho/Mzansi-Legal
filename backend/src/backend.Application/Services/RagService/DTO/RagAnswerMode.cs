using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace backend.Services.RagService.DTO;

[JsonConverter(typeof(StringEnumConverter))]
public enum RagAnswerMode
{
    [EnumMember(Value = "direct")]
    Direct,

    [EnumMember(Value = "cautious")]
    Cautious,

    [EnumMember(Value = "clarification")]
    Clarification,

    [EnumMember(Value = "insufficient")]
    Insufficient
}
