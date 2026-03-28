using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace backend.Domains.LegalDocuments;

/// <summary>
/// Represents a classification grouping for legal or financial documents.
/// Serves as the top-level organisational unit in the legislation catalogue.
/// </summary>
public class Category : FullAuditedEntity<Guid>
{
    /// <summary>Display name of the category (e.g., "Labour Law").</summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    /// <summary>Icon identifier string used by the frontend (e.g., "gavel").</summary>
    [MaxLength(100)]
    public string Icon { get; set; }

    /// <summary>Domain classification: Legal or Financial.</summary>
    [Required]
    public DocumentDomain Domain { get; set; }

    /// <summary>Display ordering among sibling categories.</summary>
    public int SortOrder { get; set; }

    /// <summary>Legal documents belonging to this category.</summary>
    public virtual ICollection<LegalDocument> LegalDocuments { get; set; }
}
