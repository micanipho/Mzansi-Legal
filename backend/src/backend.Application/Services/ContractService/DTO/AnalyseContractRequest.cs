using System.ComponentModel.DataAnnotations;

namespace backend.Services.ContractService.DTO;

/// <summary>
/// Application-layer contract-upload request independent of ASP.NET form types.
/// </summary>
public class AnalyseContractRequest
{
    [Required]
    public string FileName { get; set; }

    public string ContentType { get; set; }

    [Required]
    public byte[] FileBytes { get; set; }

    public string ResponseLanguageCode { get; set; }
}
