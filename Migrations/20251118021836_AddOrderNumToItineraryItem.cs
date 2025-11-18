using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderNumToItineraryItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ScheduleItems",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ScheduleItems",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderNum",
                table: "ItineraryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "OrderNum",
                table: "ItineraryItems");
        }
    }
}
