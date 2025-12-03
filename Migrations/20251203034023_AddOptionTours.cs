using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddOptionTours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OptionTours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomOptionId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionTours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionTours_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOptionTours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OptionTourId = table.Column<int>(type: "int", nullable: false),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptionTours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOptionTours_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserOptionTours_OptionTours_OptionTourId",
                        column: x => x.OptionTourId,
                        principalTable: "OptionTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOptionTours_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionTour_ConventionId",
                table: "OptionTours",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionTour_ConventionId_CustomOptionId",
                table: "OptionTours",
                columns: new[] { "ConventionId", "CustomOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_OptionTour_Date",
                table: "OptionTours",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_UserOptionTour_ConventionId",
                table: "UserOptionTours",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOptionTour_OptionTourId",
                table: "UserOptionTours",
                column: "OptionTourId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOptionTour_UserId",
                table: "UserOptionTours",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_UserOptionTour_UserId_ConventionId_OptionTourId",
                table: "UserOptionTours",
                columns: new[] { "UserId", "ConventionId", "OptionTourId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserOptionTours");

            migrationBuilder.DropTable(
                name: "OptionTours");
        }
    }
}
