using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicActionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishName",
                table: "Guests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PassportExpiryDate",
                table: "Guests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNumber",
                table: "Guests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VisaDocumentAttachmentId",
                table: "Guests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConventionActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MapsTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OrderNum = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConventionActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConventionActions_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestActionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    ConventionActionId = table.Column<int>(type: "int", nullable: false),
                    IsComplete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestActionStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestActionStatuses_ConventionActions_ConventionActionId",
                        column: x => x.ConventionActionId,
                        principalTable: "ConventionActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestActionStatuses_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConventionAction_ActionType",
                table: "ConventionActions",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_ConventionAction_ConventionId",
                table: "ConventionActions",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "UQ_ConventionAction_ConventionId_ActionType",
                table: "ConventionActions",
                columns: new[] { "ConventionId", "ActionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestActionStatus_ConventionActionId",
                table: "GuestActionStatuses",
                column: "ConventionActionId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestActionStatus_GuestId",
                table: "GuestActionStatuses",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "UQ_GuestActionStatus_GuestId_ConventionActionId",
                table: "GuestActionStatuses",
                columns: new[] { "GuestId", "ConventionActionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestActionStatuses");

            migrationBuilder.DropTable(
                name: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "EnglishName",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "PassportExpiryDate",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "PassportNumber",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "VisaDocumentAttachmentId",
                table: "Guests");
        }
    }
}
