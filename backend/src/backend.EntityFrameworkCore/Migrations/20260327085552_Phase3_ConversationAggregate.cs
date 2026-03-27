using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_ConversationAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MzansiConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppUserId = table.Column<long>(type: "bigint", nullable: false),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    InputMethod = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublicFaq = table.Column<bool>(type: "boolean", nullable: false),
                    FaqCategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MzansiConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MzansiConversations_AbpUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MzansiConversations_MzansiCategories_FaqCategoryId",
                        column: x => x.FaqCategoryId,
                        principalTable: "MzansiCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MzansiQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalText = table.Column<string>(type: "text", nullable: false),
                    TranslatedText = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    InputMethod = table.Column<int>(type: "integer", nullable: false),
                    AudioFilePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MzansiQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MzansiQuestions_MzansiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "MzansiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MzansiAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    AudioFilePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsAccurate = table.Column<bool>(type: "boolean", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MzansiAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MzansiAnswers_MzansiQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "MzansiQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MzansiAnswerCitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Excerpt = table.Column<string>(type: "text", nullable: true),
                    RelevanceScore = table.Column<decimal>(type: "numeric(5,4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MzansiAnswerCitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MzansiAnswerCitations_MzansiAnswers_AnswerId",
                        column: x => x.AnswerId,
                        principalTable: "MzansiAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MzansiAnswerCitations_MzansiDocumentChunks_ChunkId",
                        column: x => x.ChunkId,
                        principalTable: "MzansiDocumentChunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MzansiAnswerCitations_AnswerId",
                table: "MzansiAnswerCitations",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_MzansiAnswerCitations_ChunkId",
                table: "MzansiAnswerCitations",
                column: "ChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_MzansiAnswers_QuestionId",
                table: "MzansiAnswers",
                column: "QuestionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MzansiConversations_AppUserId",
                table: "MzansiConversations",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MzansiConversations_FaqCategoryId",
                table: "MzansiConversations",
                column: "FaqCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MzansiQuestions_ConversationId",
                table: "MzansiQuestions",
                column: "ConversationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MzansiAnswerCitations");

            migrationBuilder.DropTable(
                name: "MzansiAnswers");

            migrationBuilder.DropTable(
                name: "MzansiQuestions");

            migrationBuilder.DropTable(
                name: "MzansiConversations");
        }
    }
}
