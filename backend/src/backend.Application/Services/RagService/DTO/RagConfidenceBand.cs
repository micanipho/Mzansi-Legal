using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace backend.Services.RagService.DTO;

[JsonConverter(typeof(StringEnumConverter))]
public enum RagConfidenceBand
{
    [EnumMember(Value = "high")]
    High,

    [EnumMember(Value = "medium")]
    Medium,

    [EnumMember(Value = "low")]
    Low
}
