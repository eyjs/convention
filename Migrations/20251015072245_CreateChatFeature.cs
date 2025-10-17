using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class CreateChatFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConventionChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConventionChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConventionChatMessages_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConventionChatMessages_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_ConventionId",
                table: "ConventionChatMessages",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_CreatedAt",
                table: "ConventionChatMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConventionChatMessages_GuestId",
                table: "ConventionChatMessages",
                column: "GuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConventionChatMessages");
        }
    }
}
