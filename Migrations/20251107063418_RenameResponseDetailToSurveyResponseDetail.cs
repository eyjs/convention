using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class RenameResponseDetailToSurveyResponseDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResponseDetails");

            migrationBuilder.DropIndex(
                name: "IX_ConventionAction_ActionType",
                table: "ConventionActions");

            migrationBuilder.DropIndex(
                name: "UQ_ConventionAction_ConventionId_ActionType",
                table: "ConventionActions");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "ConventionActions");

            migrationBuilder.CreateTable(
                name: "SurveyResponseDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyResponseDetails_QuestionOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SurveyResponseDetails_SurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "SurveyQuestions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SurveyResponseDetails_SurveyResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "SurveyResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseDetails_QuestionId",
                table: "SurveyResponseDetails",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseDetails_ResponseId",
                table: "SurveyResponseDetails",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseDetails_SelectedOptionId",
                table: "SurveyResponseDetails",
                column: "SelectedOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SurveyResponseDetails");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "ConventionActions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ResponseDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ResponseId = table.Column<int>(type: "int", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: true),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseDetails_QuestionOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResponseDetails_SurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "SurveyQuestions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResponseDetails_SurveyResponses_ResponseId",
                        column: x => x.ResponseId,
                        principalTable: "SurveyResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConventionAction_ActionType",
                table: "ConventionActions",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "UQ_ConventionAction_ConventionId_ActionType",
                table: "ConventionActions",
                columns: new[] { "ConventionId", "ActionType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseDetails_QuestionId",
                table: "ResponseDetails",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseDetails_ResponseId",
                table: "ResponseDetails",
                column: "ResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseDetails_SelectedOptionId",
                table: "ResponseDetails",
                column: "SelectedOptionId");
        }
    }
}
