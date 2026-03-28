using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfIngestionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChunkStrategy",
                table: "DocumentChunks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IngestionJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExtractStartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtractCompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtractedCharacterCount = table.Column<int>(type: "integer", nullable: false),
                    TransformStartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TransformCompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChunksProduced = table.Column<int>(type: "integer", nullable: false),
                    LoadStartedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LoadCompletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ChunksLoaded = table.Column<int>(type: "integer", nullable: false),
                    Strategy = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngestionJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngestionJobs_LegalDocuments_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "LegalDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngestionJobs_DocumentId",
                table: "IngestionJobs",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_IngestionJobs_Status",
                table: "IngestionJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngestionJobs");

            migrationBuilder.DropColumn(
                name: "ChunkStrategy",
                table: "DocumentChunks");
        }
    }
}
