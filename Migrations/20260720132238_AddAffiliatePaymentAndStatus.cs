using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elwala.Migrations
{
    /// <inheritdoc />
    public partial class AddAffiliatePaymentAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Count",
                table: "AffiliateRequests",
                newName: "Status");

            migrationBuilder.CreateTable(
                name: "AffiliatePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffiliateRequestId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffiliatePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffiliatePayments_AffiliateRequests_AffiliateRequestId",
                        column: x => x.AffiliateRequestId,
                        principalTable: "AffiliateRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AffiliatePayments_AffiliateRequestId",
                table: "AffiliatePayments",
                column: "AffiliateRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffiliatePayments");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "AffiliateRequests",
                newName: "Count");
        }
    }
}
