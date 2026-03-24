using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleItemId = table.Column<int>(type: "int", nullable: true),
                    OptionTourId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleImages_ScheduleItems_ScheduleItemId",
                        column: x => x.ScheduleItemId,
                        principalTable: "ScheduleItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScheduleImages_OptionTours_OptionTourId",
                        column: x => x.OptionTourId,
                        principalTable: "OptionTours",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleImages_ScheduleItemId",
                table: "ScheduleImages",
                column: "ScheduleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleImages_OptionTourId",
                table: "ScheduleImages",
                column: "OptionTourId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleImages");
        }
    }
}
