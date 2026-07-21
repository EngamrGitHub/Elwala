using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elwala.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSortFromAffiliateRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sort",
                table: "AffiliateRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sort",
                table: "AffiliateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
