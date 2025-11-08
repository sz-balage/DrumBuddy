using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrumBuddy.Endpoint.Migrations
{
    /// <inheritdoc />
    public partial class UseProtobufForSheetContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Sheets");

            migrationBuilder.AddColumn<byte[]>(
                name: "ContentBytes",
                table: "Sheets",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentBytes",
                table: "Sheets");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Sheets",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
