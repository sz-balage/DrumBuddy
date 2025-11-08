using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrumBuddy.Endpoint.Migrations
{
    /// <inheritdoc />
    public partial class AddSheetNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sheets_UserId",
                table: "Sheets");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Sheets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_UserId_Name",
                table: "Sheets",
                columns: new[] { "UserId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sheets_UserId_Name",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Sheets");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_UserId",
                table: "Sheets",
                column: "UserId");
        }
    }
}
