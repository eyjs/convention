using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddShareableLinkToPersonalTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "PersonalTrips",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "PersonalTrips",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UQ_PersonalTrip_ShareToken",
                table: "PersonalTrips",
                column: "ShareToken",
                unique: true,
                filter: "[ShareToken] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_PersonalTrip_ShareToken",
                table: "PersonalTrips");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "PersonalTrips");

            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "PersonalTrips");
        }
    }
}
