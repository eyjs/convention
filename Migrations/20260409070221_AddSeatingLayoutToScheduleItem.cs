using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatingLayoutToScheduleItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeatingLayoutId",
                table: "ScheduleItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleItem_SeatingLayoutId",
                table: "ScheduleItems",
                column: "SeatingLayoutId");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleItems_SeatingLayouts_SeatingLayoutId",
                table: "ScheduleItems",
                column: "SeatingLayoutId",
                principalTable: "SeatingLayouts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleItems_SeatingLayouts_SeatingLayoutId",
                table: "ScheduleItems");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleItem_SeatingLayoutId",
                table: "ScheduleItems");

            migrationBuilder.DropColumn(
                name: "SeatingLayoutId",
                table: "ScheduleItems");
        }
    }
}
