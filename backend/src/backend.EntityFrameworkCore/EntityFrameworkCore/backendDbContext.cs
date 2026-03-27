using Abp.Zero.EntityFrameworkCore;
using backend.Authorization.Roles;
using backend.Authorization.Users;
using backend.MultiTenancy;
using backend.MzansiLegal.Categories;
using backend.MzansiLegal.Conversations;
using backend.MzansiLegal.KnowledgeBase;
using Microsoft.EntityFrameworkCore;

namespace backend.EntityFrameworkCore;

public class backendDbContext : AbpZeroDbContext<Tenant, Role, User, backendDbContext>
{
    // MzansiLegal — Knowledge Base
    public DbSet<Category> Categories { get; set; }
    public DbSet<LegalDocument> LegalDocuments { get; set; }
    public DbSet<DocumentChunk> DocumentChunks { get; set; }
    public DbSet<ChunkEmbedding> ChunkEmbeddings { get; set; }

    // MzansiLegal — Conversations (Phase 3)
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<AnswerCitation> AnswerCitations { get; set; }

    public backendDbContext(DbContextOptions<backendDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("MzansiCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Icon).HasMaxLength(64);
            b.Property(x => x.LocalizedLabels).HasColumnType("text");
        });

        modelBuilder.Entity<LegalDocument>(b =>
        {
            b.ToTable("MzansiLegalDocuments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(256);
            b.Property(x => x.ShortName).HasMaxLength(128);
            b.Property(x => x.ActNumber).HasMaxLength(64);
            b.Property(x => x.FullText).HasColumnType("text");
            b.Property(x => x.OriginalPdfPath).HasMaxLength(512);
            b.HasMany(x => x.Chunks).WithOne(c => c.LegalDocument)
                .HasForeignKey(c => c.LegalDocumentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentChunk>(b =>
        {
            b.ToTable("MzansiDocumentChunks");
            b.HasKey(x => x.Id);
            b.Property(x => x.SectionNumber).HasMaxLength(32);
            b.Property(x => x.SectionTitle).HasMaxLength(256);
            b.Property(x => x.ChapterTitle).HasMaxLength(256);
            b.Property(x => x.Content).HasColumnType("text");
            b.HasOne(x => x.Embedding).WithOne(e => e.DocumentChunk)
                .HasForeignKey<ChunkEmbedding>(e => e.DocumentChunkId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChunkEmbedding>(b =>
        {
            b.ToTable("MzansiChunkEmbeddings");
            b.HasKey(x => x.Id);
            b.Property(x => x.VectorJson).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<Conversation>(b =>
        {
            b.ToTable("MzansiConversations");
            b.HasKey(x => x.Id);
            b.HasMany(x => x.Questions).WithOne(q => q.Conversation)
                .HasForeignKey(q => q.ConversationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(b =>
        {
            b.ToTable("MzansiQuestions");
            b.HasKey(x => x.Id);
            b.Property(x => x.OriginalText).HasColumnType("text").IsRequired();
            b.Property(x => x.TranslatedText).HasColumnType("text");
            b.Property(x => x.AudioFilePath).HasMaxLength(512);
            b.HasOne(x => x.Answer).WithOne(a => a.Question)
                .HasForeignKey<Answer>(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Answer>(b =>
        {
            b.ToTable("MzansiAnswers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Text).HasColumnType("text").IsRequired();
            b.Property(x => x.AdminNotes).HasColumnType("text");
            b.Property(x => x.AudioFilePath).HasMaxLength(512);
            b.HasMany(x => x.Citations).WithOne(c => c.Answer)
                .HasForeignKey(c => c.AnswerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnswerCitation>(b =>
        {
            b.ToTable("MzansiAnswerCitations");
            b.HasKey(x => x.Id);
            b.Property(x => x.SectionNumber).HasMaxLength(64);
            b.Property(x => x.Excerpt).HasColumnType("text");
            b.Property(x => x.RelevanceScore).HasColumnType("decimal(5,4)");
        });
    }
}
