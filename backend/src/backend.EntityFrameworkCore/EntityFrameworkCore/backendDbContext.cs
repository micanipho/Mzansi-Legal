using Abp.Zero.EntityFrameworkCore;
using backend.Authorization.Roles;
using backend.Authorization.Users;
using backend.Domains.LegalDocuments;
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
}


