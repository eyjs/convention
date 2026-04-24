using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestScheduleTemplateConventionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GuestScheduleTemplates",
                table: "GuestScheduleTemplates");

            migrationBuilder.AddColumn<int>(
                name: "ConventionId",
                table: "GuestScheduleTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill: ScheduleTemplate.ConventionId에서 가져오기
            migrationBuilder.Sql(@"
                UPDATE gst SET gst.ConventionId = st.ConventionId
                FROM GuestScheduleTemplates gst
                INNER JOIN ScheduleTemplates st ON gst.ScheduleTemplateId = st.Id
                WHERE gst.ConventionId = 0;
            ");

            // Backfill 실패한 고아 데이터 삭제 (존재하지 않는 ScheduleTemplate)
            migrationBuilder.Sql(@"
                DELETE FROM GuestScheduleTemplates WHERE ConventionId = 0;
            ");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuestScheduleTemplates",
                table: "GuestScheduleTemplates",
                columns: new[] { "UserId", "ScheduleTemplateId", "ConventionId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestScheduleTemplate_ConventionId",
                table: "GuestScheduleTemplates",
                column: "ConventionId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestScheduleTemplates_Conventions_ConventionId",
                table: "GuestScheduleTemplates",
                column: "ConventionId",
                principalTable: "Conventions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestScheduleTemplates_Conventions_ConventionId",
                table: "GuestScheduleTemplates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuestScheduleTemplates",
                table: "GuestScheduleTemplates");

            migrationBuilder.DropIndex(
                name: "IX_GuestScheduleTemplate_ConventionId",
                table: "GuestScheduleTemplates");

            migrationBuilder.DropColumn(
                name: "ConventionId",
                table: "GuestScheduleTemplates");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuestScheduleTemplates",
                table: "GuestScheduleTemplates",
                columns: new[] { "UserId", "ScheduleTemplateId" });
        }
    }
}
