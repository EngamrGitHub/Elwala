using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elwala.Migrations
{
    /// <inheritdoc />
    public partial class AddSortAndDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "AffiliatePayments");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "AffiliateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sort",
                table: "AffiliateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "AffiliatePayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "AffiliatePayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                table: "AffiliateRequests");

            migrationBuilder.DropColumn(
                name: "Sort",
                table: "AffiliateRequests");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AffiliatePayments");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AffiliatePayments");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "AffiliatePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
