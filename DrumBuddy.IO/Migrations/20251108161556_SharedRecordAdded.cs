using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DrumBuddy.IO.Migrations
{
    /// <inheritdoc />
    public partial class SharedRecordAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sheets_CreatedAt",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Sheets");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Sheets",
                newName: "LastSyncedAt");

            migrationBuilder.RenameColumn(
                name: "ContentBytes",
                table: "Sheets",
                newName: "MeasureBytes");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Sheets",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Sheets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Tempo",
                table: "Sheets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_LastSyncedAt",
                table: "Sheets",
                column: "LastSyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_UserId_Id",
                table: "Sheets",
                columns: new[] { "UserId", "Id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sheets_LastSyncedAt",
                table: "Sheets");

            migrationBuilder.DropIndex(
                name: "IX_Sheets_UserId_Id",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Sheets");

            migrationBuilder.DropColumn(
                name: "Tempo",
                table: "Sheets");

            migrationBuilder.RenameColumn(
                name: "MeasureBytes",
                table: "Sheets",
                newName: "ContentBytes");

            migrationBuilder.RenameColumn(
                name: "LastSyncedAt",
                table: "Sheets",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Sheets",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Sheets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Sheets_CreatedAt",
                table: "Sheets",
                column: "CreatedAt");
        }
    }
}
