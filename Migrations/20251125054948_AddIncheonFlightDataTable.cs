using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddIncheonFlightDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncheonFlightData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Airline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Airport = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AirportCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FlightType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduleDate = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    ScheduleTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EstimatedTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Terminal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CheckInCounter = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MasterFlightId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncheonFlightData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncheonFlightData_FlightId",
                table: "IncheonFlightData",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_IncheonFlightData_FlightId_ScheduleDate_FlightType",
                table: "IncheonFlightData",
                columns: new[] { "FlightId", "ScheduleDate", "FlightType" });

            migrationBuilder.CreateIndex(
                name: "IX_IncheonFlightData_ScheduleDate",
                table: "IncheonFlightData",
                column: "ScheduleDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncheonFlightData");
        }
    }
}
