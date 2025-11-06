using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_ConventionActions_ConventionActionId",
                table: "SurveyResponses");

            migrationBuilder.DropTable(
                name: "SurveyResponseAnswers");

            migrationBuilder.RenameColumn(
                name: "ConventionActionId",
                table: "SurveyResponses",
                newName: "SurveyId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyResponse_UserId",
                table: "SurveyResponses",
                newName: "IX_SurveyResponses_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyResponse_ConventionActionId",
                table: "SurveyResponses",
                newName: "IX_SurveyResponses_SurveyId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "SurveyResponses",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getdate()");

            migrationBuilder.CreateTable(
                name: "Surveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConventionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Surveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Surveys_Conventions_ConventionId",
                        column: x => x.ConventionId,
                        principalTable: "Conventions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SurveyQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyQuestions_Surveys_SurveyId",
                        column: x => x.SurveyId,
                        principalTable: "Surveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_SurveyQuestions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "SurveyQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResponseDetails",
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
                name: "IX_QuestionOptions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_SurveyQuestions_SurveyId",
                table: "SurveyQuestions",
                column: "SurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_Surveys_ConventionId",
                table: "Surveys",
                column: "ConventionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_Surveys_SurveyId",
                table: "SurveyResponses",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyResponses_Surveys_SurveyId",
                table: "SurveyResponses");

            migrationBuilder.DropTable(
                name: "ResponseDetails");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "SurveyQuestions");

            migrationBuilder.DropTable(
                name: "Surveys");

            migrationBuilder.RenameColumn(
                name: "SurveyId",
                table: "SurveyResponses",
                newName: "ConventionActionId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyResponses_UserId",
                table: "SurveyResponses",
                newName: "IX_SurveyResponse_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_SurveyResponses_SurveyId",
                table: "SurveyResponses",
                newName: "IX_SurveyResponse_ConventionActionId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SubmittedAt",
                table: "SurveyResponses",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "SurveyResponseAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurveyResponseId = table.Column<int>(type: "int", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponseAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SurveyResponseAnswers_SurveyResponses_SurveyResponseId",
                        column: x => x.SurveyResponseId,
                        principalTable: "SurveyResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SurveyResponseAnswers_SurveyResponseId",
                table: "SurveyResponseAnswers",
                column: "SurveyResponseId");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyResponses_ConventionActions_ConventionActionId",
                table: "SurveyResponses",
                column: "ConventionActionId",
                principalTable: "ConventionActions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
