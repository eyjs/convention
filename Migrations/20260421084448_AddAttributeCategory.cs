using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttributeDefinitions");

            migrationBuilder.DropTable(
                name: "AttributeTemplates");

            migrationBuilder.CreateTable(
                name: "AttributeCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeCategories_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeCategoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttributeCategoryId = table.Column<int>(type: "int", nullable: false),
                    AttributeKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNum = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeCategoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeCategoryItems_AttributeCategories_AttributeCategoryId",
                        column: x => x.AttributeCategoryId,
                        principalTable: "AttributeCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeCategories_ConventionId",
                table: "AttributeCategories",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeCategoryItems_AttributeCategoryId",
                table: "AttributeCategoryItems",
                column: "AttributeCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttributeCategoryItems");

            migrationBuilder.DropTable(
                name: "AttributeCategories");

            migrationBuilder.CreateTable(
                name: "AttributeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    AttributeKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderNum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeDefinitions_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttributeTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    AttributeKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AttributeValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderNum = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttributeTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttributeTemplates_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinition_ConventionId",
                table: "AttributeDefinitions",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "UQ_AttributeDefinition_ConventionId_AttributeKey",
                table: "AttributeDefinitions",
                columns: new[] { "ConventionId", "AttributeKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeTemplate_ConventionId",
                table: "AttributeTemplates",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "UQ_AttributeTemplate_ConventionId_AttributeKey",
                table: "AttributeTemplates",
                columns: new[] { "ConventionId", "AttributeKey" },
                unique: true);
        }
    }
}
