using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedTransportationCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TollFee",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelCost",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ParkingFee",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RentalCost",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TollFee",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "FuelCost",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ParkingFee",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "RentalCost",
                table: "Flights");
        }
    }
}
