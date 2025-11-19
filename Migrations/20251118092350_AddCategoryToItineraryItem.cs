using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToItineraryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ItineraryItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "ItineraryItems");
        }
    }
}
