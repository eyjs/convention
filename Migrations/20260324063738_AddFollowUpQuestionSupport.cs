using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowUpQuestionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentOptionId",
                table: "SurveyQuestions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestions_ParentOptionId",
                table: "SurveyQuestions",
                column: "ParentOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyQuestions_QuestionOptions_ParentOptionId",
                table: "SurveyQuestions",
                column: "ParentOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyQuestions_QuestionOptions_ParentOptionId",
                table: "SurveyQuestions");

            migrationBuilder.DropIndex(
                name: "IX_SurveyQuestions_ParentOptionId",
                table: "SurveyQuestions");

            migrationBuilder.DropColumn(
                name: "ParentOptionId",
                table: "SurveyQuestions");
        }
    }
}
