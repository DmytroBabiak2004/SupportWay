using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportWay.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorConfigurationsAndAddVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerificationRequests_UserId",
                table: "VerificationRequests");

            migrationBuilder.AlterColumn<bool>(
                name: "IsVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrivate",
                table: "Chats",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_UserId_Status",
                table: "VerificationRequests",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VerificationRequests_UserId_Status",
                table: "VerificationRequests");

            migrationBuilder.AlterColumn<bool>(
                name: "IsVerified",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPrivate",
                table: "Chats",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_UserId",
                table: "VerificationRequests",
                column: "UserId");
        }
    }
}
