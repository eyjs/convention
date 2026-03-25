using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyTypeAndOptionTourLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId",
                table: "SurveyResponses");

            migrationBuilder.AddColumn<int>(
                name: "SurveyType",
                table: "Surveys",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptionTourId",
                table: "QuestionOptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId_UserId",
                table: "SurveyResponses",
                columns: new[] { "SurveyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_OptionTourId",
                table: "QuestionOptions",
                column: "OptionTourId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionOptions_OptionTours_OptionTourId",
                table: "QuestionOptions",
                column: "OptionTourId",
                principalTable: "OptionTours",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionOptions_OptionTours_OptionTourId",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_SurveyResponses_SurveyId_UserId",
                table: "SurveyResponses");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_OptionTourId",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "SurveyType",
                table: "Surveys");

            migrationBuilder.DropColumn(
                name: "OptionTourId",
                table: "QuestionOptions");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponses_SurveyId",
                table: "SurveyResponses",
                column: "SurveyId");
        }
    }
}
