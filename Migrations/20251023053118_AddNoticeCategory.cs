using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddNoticeCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoticeCategoryId",
                table: "Notices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NoticeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeCategories_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notice_NoticeCategoryId",
                table: "Notices",
                column: "NoticeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeCategory_ConventionId",
                table: "NoticeCategories",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeCategory_ConventionId_Name",
                table: "NoticeCategories",
                columns: new[] { "ConventionId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Notices_NoticeCategories_NoticeCategoryId",
                table: "Notices",
                column: "NoticeCategoryId",
                principalTable: "NoticeCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notices_NoticeCategories_NoticeCategoryId",
                table: "Notices");

            migrationBuilder.DropTable(
                name: "NoticeCategories");

            migrationBuilder.DropIndex(
                name: "IX_Notice_NoticeCategoryId",
                table: "Notices");

            migrationBuilder.DropColumn(
                name: "NoticeCategoryId",
                table: "Notices");
        }
    }
}
