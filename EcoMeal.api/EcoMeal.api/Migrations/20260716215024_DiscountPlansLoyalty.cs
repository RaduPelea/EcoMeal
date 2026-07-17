using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoMeal.api.Migrations
{
    /// <inheritdoc />
    public partial class DiscountPlansLoyalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountHoursBeforeEnd",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyDiscountPercent",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasActiveLoyaltyDiscount",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LoyaltyClaimedRewards",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountHoursBeforeEnd",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "LoyaltyDiscountPercent",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HasActiveLoyaltyDiscount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LoyaltyClaimedRewards",
                table: "AspNetUsers");
        }
    }
}
