using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elwala.Migrations
{
    /// <inheritdoc />
    public partial class MoveStatusAndCountToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AffiliateRequests");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "AffiliatePayments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "AffiliatePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "AffiliatePayments");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AffiliateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "AffiliatePayments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
