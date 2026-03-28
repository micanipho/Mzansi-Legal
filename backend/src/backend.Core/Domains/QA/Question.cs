using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Domains.QA;

/// <summary>
/// Represents a single user query submitted within a <see cref="Conversation"/>.
/// Stores the original user text, a translation (may equal the original when no translation is needed),
/// the language and input method used, and an optional audio file reference for voice inputs.
/// </summary>
public class Question : FullAuditedEntity<Guid>
{
    /// <summary>Foreign key to the parent <see cref="Conversation"/>.</summary>
    [Required]
    public Guid ConversationId { get; set; }

    /// <summary>Navigation property to the parent Conversation.</summary>
    [ForeignKey(nameof(ConversationId))]
    public virtual Conversation Conversation { get; set; }

    /// <summary>
    /// Raw text of the question exactly as submitted by the user,
    /// before any translation or normalisation.
    /// </summary>
    [Required]
    public string OriginalText { get; set; }

    /// <summary>
    /// Translation of <see cref="OriginalText"/> into the pipeline's processing language.
    /// May equal <see cref="OriginalText"/> when the user's language matches the default.
    /// </summary>
    [Required]
    public string TranslatedText { get; set; }

    /// <summary>Language in which this specific question was submitted.</summary>
    [Required]
    public Language Language { get; set; }

    /// <summary>
    /// Method by which this question was submitted (Text or Voice).
    /// May differ from the parent <see cref="Conversation.InputMethod"/> for mixed-mode sessions.
    /// </summary>
    [Required]
    public InputMethod InputMethod { get; set; }

    /// <summary>
    /// Opaque reference to the audio file when <see cref="InputMethod"/> is Voice
    /// (e.g., a storage object key or URL). Null for text-only questions.
    /// The actual binary is managed by the file storage service, not the database.
    /// </summary>
    [MaxLength(500)]
    public string AudioFile { get; set; }

    /// <summary>
    /// Collection of AI-generated answers produced in response to this question.
    /// Loaded via Include; not populated on lightweight queries.
    /// </summary>
    public virtual ICollection<Answer> Answers { get; set; }
}
