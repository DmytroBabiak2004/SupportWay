using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportWay.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHelpRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProfileBadges_ProfileId",
                table: "ProfileBadges");

            migrationBuilder.AddColumn<string>(
                name: "DonationNotes",
                table: "Posts",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonationPaymentLink",
                table: "Posts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonationRecipientCardNumber",
                table: "Posts",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonationRecipientIban",
                table: "Posts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DonationRecipientName",
                table: "Posts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredDonationMethod",
                table: "Posts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileBadges_ProfileId_BadgeId",
                table: "ProfileBadges",
                columns: new[] { "ProfileId", "BadgeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProfileBadges_ProfileId_BadgeId",
                table: "ProfileBadges");

            migrationBuilder.DropColumn(
                name: "DonationNotes",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DonationPaymentLink",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DonationRecipientCardNumber",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DonationRecipientIban",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DonationRecipientName",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PreferredDonationMethod",
                table: "Posts");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileBadges_ProfileId",
                table: "ProfileBadges",
                column: "ProfileId");
        }
    }
}
