using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.ContractAnalysis;

/// <summary>
/// Represents a single finding identified during the analysis of a <see cref="ContractAnalysis"/>.
/// Each flag captures the severity of the finding, its title, a plain-language description,
/// the verbatim clause text, and an optional citation to applicable South African legislation.
/// Flags are ordered for display using <see cref="SortOrder"/>.
/// </summary>
public class ContractFlag : FullAuditedEntity<Guid>
{
    /// <summary>Foreign key to the parent <see cref="ContractAnalysis"/>.</summary>
    [Required]
    public Guid ContractAnalysisId { get; set; }

    /// <summary>Navigation property to the parent ContractAnalysis.</summary>
    [ForeignKey(nameof(ContractAnalysisId))]
    public virtual ContractAnalysis ContractAnalysis { get; set; }

    /// <summary>
    /// Severity classification of this finding.
    /// Red = serious concern; Amber = caution; Green = standard/acceptable clause.
    /// </summary>
    [Required]
    public FlagSeverity Severity { get; set; }

    /// <summary>Short display title summarising the nature of this finding (max 200 characters).</summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; }

    /// <summary>
    /// Plain-language explanation of why this clause is flagged and what it means for the user.
    /// Written in the language of the parent <see cref="ContractAnalysis"/>.
    /// </summary>
    [Required]
    public string Description { get; set; }

    /// <summary>
    /// Verbatim text of the clause extracted from the contract that produced this flag.
    /// Allows the user to locate and read the exact clause in context.
    /// </summary>
    [Required]
    public string ClauseText { get; set; }

    /// <summary>
    /// Free-text citation of the South African legislation relevant to this finding
    /// (e.g., "Labour Relations Act 66 of 1995, Section 37"). Optional — null when no
    /// specific legislation applies or has been identified.
    /// </summary>
    [MaxLength(1000)]
    public string LegislationCitation { get; set; }

    /// <summary>
    /// Display ordering position among flags within the same <see cref="ContractAnalysis"/>.
    /// Lower values appear first. Defaults to 0 when not explicitly assigned.
    /// </summary>
    public int SortOrder { get; set; }
}
