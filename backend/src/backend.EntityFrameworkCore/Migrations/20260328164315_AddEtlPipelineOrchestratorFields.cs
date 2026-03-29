using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEtlPipelineOrchestratorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "IngestionJobs",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmbeddingsGenerated",
                table: "IngestionJobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "TriggeredByUserId",
                table: "IngestionJobs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "DocumentChunks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TopicClassification",
                table: "DocumentChunks",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngestionJobs_TriggeredByUserId",
                table: "IngestionJobs",
                column: "TriggeredByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_IngestionJobs_AbpUsers_TriggeredByUserId",
                table: "IngestionJobs",
                column: "TriggeredByUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IngestionJobs_AbpUsers_TriggeredByUserId",
                table: "IngestionJobs");

            migrationBuilder.DropIndex(
                name: "IX_IngestionJobs_TriggeredByUserId",
                table: "IngestionJobs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "IngestionJobs");

            migrationBuilder.DropColumn(
                name: "EmbeddingsGenerated",
                table: "IngestionJobs");

            migrationBuilder.DropColumn(
                name: "TriggeredByUserId",
                table: "IngestionJobs");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "DocumentChunks");

            migrationBuilder.DropColumn(
                name: "TopicClassification",
                table: "DocumentChunks");
        }
    }
}
