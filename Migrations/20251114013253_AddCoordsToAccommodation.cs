using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordsToAccommodation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Accommodations",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Accommodations",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Accommodations");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Accommodations");
        }
    }
}
