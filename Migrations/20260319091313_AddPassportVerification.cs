using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalRAG.Migrations
{
    /// <inheritdoc />
    public partial class AddPassportVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActionStatuses_Users_UserId",
                table: "UserActionStatuses");

            migrationBuilder.AddColumn<bool>(
                name: "PassportVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportVerifiedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserActionStatuses_Users_UserId",
                table: "UserActionStatuses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserActionStatuses_Users_UserId",
                table: "UserActionStatuses");

            migrationBuilder.DropColumn(
                name: "PassportVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PassportVerifiedAt",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_UserActionStatuses_Users_UserId",
                table: "UserActionStatuses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
