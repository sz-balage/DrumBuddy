using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DrumBuddy.IO.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLastSyncedAtFromDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sheets_LastSyncedAt",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "Sheets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "Sheets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_LastSyncedAt",
                table: "Sheets",
                column: "LastSyncedAt");
        }
    }
}
