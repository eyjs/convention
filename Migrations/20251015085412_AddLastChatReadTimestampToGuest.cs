using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddLastChatReadTimestampToGuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastChatReadTimestamp",
                table: "Guests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastChatReadTimestamp",
                table: "Guests");
        }
    }
}
