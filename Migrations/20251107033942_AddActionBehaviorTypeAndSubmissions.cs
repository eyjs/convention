using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddActionBehaviorTypeAndSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BehaviorType",
                table: "ConventionActions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetModuleId",
                table: "ConventionActions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActionSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionActionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SubmissionDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionSubmissions_ConventionActions_ConventionActionId",
                        column: x => x.ConventionActionId,
                        principalTable: "ConventionActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionSubmissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionSubmission_ConventionActionId",
                table: "ActionSubmissions",
                column: "ConventionActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionSubmission_UserId",
                table: "ActionSubmissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_ActionSubmission_UserId_ConventionActionId",
                table: "ActionSubmissions",
                columns: new[] { "UserId", "ConventionActionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionSubmissions");

            migrationBuilder.DropColumn(
                name: "BehaviorType",
                table: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "TargetModuleId",
                table: "ConventionActions");
        }
    }
}
