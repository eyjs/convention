using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddActionProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ConventionActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconClass",
                table: "ConventionActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                table: "ConventionActions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TemplateId",
                table: "ConventionActions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActionTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultRoute = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequiredFields = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    OrderNum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VectorDataEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmbeddingData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VectorDataEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConventionActions_TemplateId",
                table: "ConventionActions",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_VectorDataEntries_ConventionId",
                table: "VectorDataEntries",
                column: "ConventionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConventionActions_ActionTemplates_TemplateId",
                table: "ConventionActions",
                column: "TemplateId",
                principalTable: "ActionTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConventionActions_ActionTemplates_TemplateId",
                table: "ConventionActions");

            migrationBuilder.DropTable(
                name: "ActionTemplates");

            migrationBuilder.DropTable(
                name: "VectorDataEntries");

            migrationBuilder.DropIndex(
                name: "IX_ConventionActions_TemplateId",
                table: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "IconClass",
                table: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                table: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "ConventionActions");
        }
    }
}
