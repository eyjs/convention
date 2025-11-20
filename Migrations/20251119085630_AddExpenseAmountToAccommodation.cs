using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseAmountToAccommodation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExpenseAmount",
                table: "Accommodations",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "FuelCost",
            //     table: "Flights");

            // migrationBuilder.DropColumn(
            //     name: "ParkingFee",
            //     table: "Flights");

            // migrationBuilder.DropColumn(
            //     name: "RentalCost",
            //     table: "Flights");

            // migrationBuilder.DropColumn(
            //     name: "TollFee",
            //     table: "Flights");

            migrationBuilder.DropColumn(
                name: "ExpenseAmount",
                table: "Accommodations");
        }
    }
}
