using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLegalDocumentUniqueIndexToShortNameYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LegalDocuments_ActNumber_Year",
                table: "LegalDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_ShortName_Year",
                table: "LegalDocuments",
                columns: new[] { "ShortName", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LegalDocuments_ShortName_Year",
                table: "LegalDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_LegalDocuments_ActNumber_Year",
                table: "LegalDocuments",
                columns: new[] { "ActNumber", "Year" },
                unique: true);
        }
    }
}
