using Abp.Domain.Entities.Auditing;
using backend.Domains.QA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Domains.ContractAnalysis;

/// <summary>
/// Represents the result of a contract analysis session initiated by a registered user.
/// Stores the uploaded file reference, extracted contract text, classification, health score,
/// plain-language summary, and the language and timestamp of the analysis.
/// Acts as the aggregate root that owns all <see cref="ContractFlag"/> findings for this session.
/// Every ContractAnalysis is mandatory-linked to an AppUser — anonymous analyses are not permitted.
/// </summary>
public class ContractAnalysis : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Foreign key to the ABP Zero AppUser who owns this contract analysis.
    /// Typed as <c>long</c> to match ABP Zero's User primary key type.
    /// </summary>
    [Required]
    public long UserId { get; set; }

    /// <summary>
    /// Foreign key to the ABP BinaryObject representing the uploaded contract file.
    /// Null until the file has been uploaded and stored.
    /// The actual binary is managed by ABP's IBinaryObjectManager, not the database row.
    /// </summary>
    public Guid? OriginalFileId { get; set; }

    /// <summary>
    /// Plain text extracted from the contract by the ingestion pipeline (OCR or direct extraction).
    /// May be null or empty when text extraction failed (e.g., a scanned image without OCR support).
    /// </summary>
    public string ExtractedText { get; set; }

    /// <summary>
    /// Classification of the contract type, derived by the analysis pipeline.
    /// Must be one of the supported types: Employment, Lease, Credit, or Service.
    /// </summary>
    [Required]
    public ContractType ContractType { get; set; }

    /// <summary>
    /// Overall health score of the contract on a scale of 0 to 100 (inclusive).
    /// Higher scores indicate a contract more favourable to the user.
    /// 0 = highly problematic; 100 = fully standard and user-friendly.
    /// </summary>
    [Required]
    [Range(0, 100)]
    public int HealthScore { get; set; }

    /// <summary>
    /// AI-generated plain-language summary of the contract's key terms and findings,
    /// written in the language indicated by <see cref="Language"/>.
    /// May be null when the analysis has not yet produced a summary.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// The language in which the contract was analysed and in which the summary was generated.
    /// Must be one of the four constitutionally required SA languages.
    /// </summary>
    [Required]
    public Language Language { get; set; }

    /// <summary>UTC timestamp recording when this contract analysis was completed by the pipeline.</summary>
    [Required]
    public DateTime AnalysedAt { get; set; }

    /// <summary>
    /// Collection of individual findings (red flags, cautions, standard notes) produced by the analysis.
    /// Loaded via Include; not populated on lightweight queries.
    /// Cascade-deleted when this ContractAnalysis is deleted.
    /// </summary>
    public virtual ICollection<ContractFlag> Flags { get; set; }
}
