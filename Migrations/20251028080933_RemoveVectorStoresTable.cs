using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVectorStoresTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VectorStores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VectorStores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConventionId = table.Column<int>(type: "int", nullable: false),
                    RegDtm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceTable = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VectorStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VectorStores_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VectorStore_ConventionId",
                table: "VectorStores",
                column: "ConventionId");

            migrationBuilder.CreateIndex(
                name: "IX_VectorStore_SourceType",
                table: "VectorStores",
                column: "SourceType");
        }
    }
}
