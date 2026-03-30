using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportWay.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpRequestProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CollectedAmount",
                table: "Posts",
                type: "numeric(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Posts",
                type: "boolean",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Posts",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Posts",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetAmount",
                table: "Posts",
                type: "numeric(18,2)",
                nullable: true,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_IsActive_CreatedAt",
                table: "Posts",
                columns: new[] { "IsActive", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_IsActive_CreatedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CollectedAmount",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TargetAmount",
                table: "Posts");
        }
    }
}
