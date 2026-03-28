using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.QA;

/// <summary>
/// Represents the AI-generated response to a <see cref="Question"/>.
/// Stores the answer text, its language, an optional audio file for text-to-speech output,
/// and admin-review metadata. Citations linking the answer to legislation are stored
/// in <see cref="AnswerCitation"/> records owned by this entity.
/// </summary>
public class Answer : FullAuditedEntity<Guid>
{
    /// <summary>Foreign key to the parent <see cref="Question"/>.</summary>
    [Required]
    public Guid QuestionId { get; set; }

    /// <summary>Navigation property to the parent Question.</summary>
    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; }

    /// <summary>Full text of the AI-generated answer, in the language indicated by <see cref="Language"/>.</summary>
    [Required]
    public string Text { get; set; }

    /// <summary>Language in which this answer was generated and delivered to the user.</summary>
    [Required]
    public Language Language { get; set; }

    /// <summary>
    /// Opaque reference to the text-to-speech audio file when voice output was generated
    /// (e.g., a storage object key or URL). Null for text-only answers.
    /// The actual binary is managed by the file storage service, not the database.
    /// </summary>
    [MaxLength(500)]
    public string AudioFile { get; set; }

    /// <summary>
    /// Admin accuracy review flag. Three states are valid:
    /// <list type="bullet">
    ///   <item><description><c>null</c> — not yet reviewed by an administrator.</description></item>
    ///   <item><description><c>true</c> — administrator confirmed the answer is legally accurate.</description></item>
    ///   <item><description><c>false</c> — administrator marked the answer as inaccurate.</description></item>
    /// </list>
    /// </summary>
    public bool? IsAccurate { get; set; }

    /// <summary>
    /// Optional free-text notes added by an administrator during accuracy review.
    /// Null until an administrator has reviewed this answer.
    /// </summary>
    public string AdminNotes { get; set; }

    /// <summary>
    /// Collection of legislation citations that support this answer.
    /// Each citation links to a specific <see cref="DocumentChunk"/> from the knowledge base.
    /// Loaded via Include; not populated on lightweight queries.
    /// </summary>
    public virtual ICollection<AnswerCitation> Citations { get; set; }
}
