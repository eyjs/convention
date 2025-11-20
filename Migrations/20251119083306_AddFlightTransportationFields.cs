using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightTransportationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Flights",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "항공편");

            migrationBuilder.AddColumn<int>(
                name: "ItineraryItemId",
                table: "Flights",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_ItineraryItemId",
                table: "Flights",
                column: "ItineraryItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_ItineraryItems_ItineraryItemId",
                table: "Flights",
                column: "ItineraryItemId",
                principalTable: "ItineraryItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_ItineraryItems_ItineraryItemId",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_ItineraryItemId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "ItineraryItemId",
                table: "Flights");
        }
    }
}
