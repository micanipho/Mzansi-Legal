using Abp.Zero.EntityFrameworkCore;
using backend.Authorization.Roles;
using backend.Authorization.Users;
using backend.MultiTenancy;
using backend.MzansiLegal.Categories;
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
    }
}
