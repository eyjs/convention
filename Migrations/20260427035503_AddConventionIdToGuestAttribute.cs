using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddConventionIdToGuestAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_GuestAttributes_UserId_AttributeKey",
                table: "GuestAttributes");

            migrationBuilder.AddColumn<int>(
                name: "ConventionId",
                table: "GuestAttributes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestAttributes_UserId",
                table: "GuestAttributes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_GuestAttributes_ConventionId_UserId_AttributeKey",
                table: "GuestAttributes",
                columns: new[] { "ConventionId", "UserId", "AttributeKey" },
                unique: true,
                filter: "[ConventionId] IS NOT NULL");

            // 기존 데이터 보정: UserConvention 기반으로 ConventionId 채우기
            // 한 사용자가 여러 행사에 참여한 경우 가장 최근 행사로 배정
            migrationBuilder.Sql(@"
                UPDATE ga
                SET ga.ConventionId = uc.ConventionId
                FROM GuestAttributes ga
                INNER JOIN (
                    SELECT UserId, MAX(ConventionId) AS ConventionId
                    FROM UserConventions
                    GROUP BY UserId
                ) uc ON ga.UserId = uc.UserId
                WHERE ga.ConventionId IS NULL
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestAttributes_Conventions_ConventionId",
                table: "GuestAttributes",
                column: "ConventionId",
                principalTable: "Conventions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestAttributes_Conventions_ConventionId",
                table: "GuestAttributes");

            migrationBuilder.DropIndex(
                name: "IX_GuestAttributes_UserId",
                table: "GuestAttributes");

            migrationBuilder.DropIndex(
                name: "UQ_GuestAttributes_ConventionId_UserId_AttributeKey",
                table: "GuestAttributes");

            migrationBuilder.DropColumn(
                name: "ConventionId",
                table: "GuestAttributes");

            migrationBuilder.CreateIndex(
                name: "UQ_GuestAttributes_UserId_AttributeKey",
                table: "GuestAttributes",
                columns: new[] { "UserId", "AttributeKey" },
                unique: true);
        }
    }
}
