using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddPassportRejection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_SeatingLayouts_SeatingLayoutId",
                table: "ScheduleItems");

            migrationBuilder.DropTable(
                name: "SeatingLayouts");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItem_SeatingLayoutId",
                table: "ScheduleItems");

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportRejectedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportRejectionReason",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassportRejectedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PassportRejectionReason",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "SeatingLayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    BackgroundImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CanvasHeight = table.Column<int>(type: "int", nullable: false),
                    CanvasWidth = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LayoutJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatingLayouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeatingLayouts_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItem_SeatingLayoutId",
                table: "ScheduleItems",
                column: "SeatingLayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_SeatingLayout_ConventionId",
                table: "SeatingLayouts",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_SeatingLayout_IsDeleted",
                table: "SeatingLayouts",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_SeatingLayouts_SeatingLayoutId",
                table: "ScheduleItems",
                column: "SeatingLayoutId",
                principalTable: "SeatingLayouts",
                principalColumn: "Id");
        }
    }
}
