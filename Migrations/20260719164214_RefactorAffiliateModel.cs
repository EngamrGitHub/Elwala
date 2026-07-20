using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elwala.Migrations
{
    /// <inheritdoc />
    public partial class RefactorAffiliateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BusinessActivity",
                table: "AffiliateRequests");

            migrationBuilder.DropColumn(
                name: "CountryOfResidence",
                table: "AffiliateRequests");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "AffiliateRequests");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "AffiliateRequests");

            migrationBuilder.RenameColumn(
                name: "NumberOfPartners",
                table: "AffiliateRequests",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "Nationality",
                table: "AffiliateRequests",
                newName: "Slug");

            migrationBuilder.AddColumn<string>(
                name: "PlatformUrlsJson",
                table: "AffiliateRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatformUrlsJson",
                table: "AffiliateRequests");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "AffiliateRequests",
                newName: "NumberOfPartners");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "AffiliateRequests",
                newName: "Nationality");

            migrationBuilder.AddColumn<string>(
                name: "BusinessActivity",
                table: "AffiliateRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryOfResidence",
                table: "AffiliateRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "AffiliateRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "AffiliateRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
