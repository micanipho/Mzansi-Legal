using Abp.Zero.EntityFrameworkCore;
using backend.Authorization.Roles;
using backend.Authorization.Users;
using backend.Domains.LegalDocuments;
using backend.Domains.QA;
using backend.MultiTenancy;
using Microsoft.EntityFrameworkCore;

namespace backend.EntityFrameworkCore;

/// <summary>
/// Main EF Core DbContext for the backend application.
/// Declares all DbSets and applies Fluent API configuration that cannot be expressed with data annotations.
/// </summary>
public class backendDbContext : AbpZeroDbContext<Tenant, Role, User, backendDbContext>
{
    // ── Legal Documents domain ──────────────────────────────────────────────

    /// <summary>Categories that classify legal and financial documents.</summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>Legislation documents stored in the system.</summary>
    public DbSet<LegalDocument> LegalDocuments { get; set; }

    /// <summary>Text chunks produced by the ingestion pipeline from a LegalDocument.</summary>
    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    /// <summary>Embedding vectors associated with each DocumentChunk.</summary>
    public DbSet<ChunkEmbedding> ChunkEmbeddings { get; set; }

    // ── Q&A domain ──────────────────────────────────────────────────────────

    /// <summary>Legal assistance conversations, each scoped to an authenticated user.</summary>
    public DbSet<Conversation> Conversations { get; set; }

    /// <summary>User questions submitted within a Conversation.</summary>
    public DbSet<Question> Questions { get; set; }

    /// <summary>AI-generated answers produced in response to a Question.</summary>
    public DbSet<Answer> Answers { get; set; }

    /// <summary>Legislation citations linking an Answer to a specific DocumentChunk.</summary>
    public DbSet<AnswerCitation> AnswerCitations { get; set; }

    // ───────────────────────────────────────────────────────────────────────

    public backendDbContext(DbContextOptions<backendDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Applies Fluent API configurations for constraints and relationships that cannot
    /// be expressed with data annotations (cascade rules, composite indexes, unique indexes).
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureLegalDocumentRelationships(modelBuilder);
        ConfigureDocumentChunkRelationships(modelBuilder);
        ConfigureChunkEmbeddingRelationships(modelBuilder);
        ConfigureQARelationships(modelBuilder);
    }

    /// <summary>Configures the LegalDocument → Category FK and unique act index.</summary>
    private static void ConfigureLegalDocumentRelationships(ModelBuilder modelBuilder)
    {
        // A document must belong to a category; deleting a category is restricted while documents exist.
        modelBuilder.Entity<LegalDocument>()
            .HasOne(d => d.Category)
            .WithMany(c => c.LegalDocuments)
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate registrations of the same act in the same year.
        modelBuilder.Entity<LegalDocument>()
            .HasIndex(d => new { d.ActNumber, d.Year })
            .IsUnique();
    }

    /// <summary>Configures the DocumentChunk → LegalDocument FK, cascade, and ordering index.</summary>
    private static void ConfigureDocumentChunkRelationships(ModelBuilder modelBuilder)
    {
        // Chunks belong exclusively to their parent document; deleting a document removes its chunks.
        modelBuilder.Entity<DocumentChunk>()
            .HasOne(c => c.Document)
            .WithMany(d => d.Chunks)
            .HasForeignKey(c => c.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Support efficient ordered retrieval of all chunks for a given document.
        modelBuilder.Entity<DocumentChunk>()
            .HasIndex(c => new { c.DocumentId, c.SortOrder });
    }

    /// <summary>Configures the ChunkEmbedding → DocumentChunk one-to-one FK and cascade.</summary>
    private static void ConfigureChunkEmbeddingRelationships(ModelBuilder modelBuilder)
    {
        // One embedding per chunk; deleting a chunk removes its embedding.
        modelBuilder.Entity<ChunkEmbedding>()
            .HasOne(e => e.Chunk)
            .WithOne(c => c.Embedding)
            .HasForeignKey<ChunkEmbedding>(e => e.ChunkId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    /// <summary>
    /// Configures all FK relationships, cascade rules, and indexes for the Q&A domain:
    /// Conversation, Question, Answer, and AnswerCitation entities.
    /// </summary>
    private static void ConfigureQARelationships(ModelBuilder modelBuilder)
    {
        ConfigureConversationRelationships(modelBuilder);
        ConfigureQuestionRelationships(modelBuilder);
        ConfigureAnswerRelationships(modelBuilder);
        ConfigureAnswerCitationRelationships(modelBuilder);
    }

    /// <summary>
    /// Configures Conversation → User FK (restrict) and Conversation → Category FK (nullable, restrict).
    /// Adds indexes to support user history queries and public FAQ filtering by category.
    /// </summary>
    private static void ConfigureConversationRelationships(ModelBuilder modelBuilder)
    {
        // Conversation must always belong to a user; users cannot be deleted while conversations exist.
        modelBuilder.Entity<Conversation>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // FAQ category FK is nullable; category deletion is blocked while FAQ conversations reference it.
        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.FaqCategory)
            .WithMany()
            .HasForeignKey(c => c.FaqCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Support efficient retrieval of all conversations for a given user.
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => c.UserId);

        // Support public FAQ queries filtered by category.
        modelBuilder.Entity<Conversation>()
            .HasIndex(c => new { c.IsPublicFaq, c.FaqCategoryId });
    }

    /// <summary>
    /// Configures Question → Conversation FK (cascade) and an index on ConversationId
    /// to support efficient retrieval of all questions within a conversation.
    /// </summary>
    private static void ConfigureQuestionRelationships(ModelBuilder modelBuilder)
    {
        // Questions are owned by their conversation; deleting a conversation removes its questions.
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Conversation)
            .WithMany(c => c.Questions)
            .HasForeignKey(q => q.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasIndex(q => q.ConversationId);
    }

    /// <summary>
    /// Configures Answer → Question FK (cascade) and an index on QuestionId
    /// to support efficient retrieval of all answers for a given question.
    /// </summary>
    private static void ConfigureAnswerRelationships(ModelBuilder modelBuilder)
    {
        // Answers are owned by their question; deleting a question removes its answers.
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Answer>()
            .HasIndex(a => a.QuestionId);
    }

    /// <summary>
    /// Configures AnswerCitation → Answer FK (cascade) and AnswerCitation → DocumentChunk FK (restrict).
    /// The DocumentChunk reference is a cross-aggregate citation link — chunk deletion is blocked
    /// while any citation references it, protecting the citation integrity of all answers.
    /// </summary>
    private static void ConfigureAnswerCitationRelationships(ModelBuilder modelBuilder)
    {
        // Citations are owned by their answer; deleting an answer removes its citations.
        modelBuilder.Entity<AnswerCitation>()
            .HasOne(ac => ac.Answer)
            .WithMany(a => a.Citations)
            .HasForeignKey(ac => ac.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cross-aggregate reference: chunk deletion is restricted while citations exist.
        modelBuilder.Entity<AnswerCitation>()
            .HasOne(ac => ac.Chunk)
            .WithMany()
            .HasForeignKey(ac => ac.ChunkId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AnswerCitation>()
            .HasIndex(ac => ac.AnswerId);

        // Supports reverse-lookup: which answers cite a given chunk.
        modelBuilder.Entity<AnswerCitation>()
            .HasIndex(ac => ac.ChunkId);
    }
}


