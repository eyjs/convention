using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGuestResidentNumberField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CorpHrId 컬럼 이름을 ResidentNumber로 변경
            migrationBuilder.RenameColumn(
                name: "CorpHrId",
                table: "Guests",
                newName: "ResidentNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 롤백: ResidentNumber를 CorpHrId로 되돌림
            migrationBuilder.RenameColumn(
                name: "ResidentNumber",
                table: "Guests",
                newName: "CorpHrId");
        }
    }
}
