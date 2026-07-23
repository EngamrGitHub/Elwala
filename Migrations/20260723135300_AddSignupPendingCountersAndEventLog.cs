using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elwala.Migrations
{
    /// <inheritdoc />
    public partial class AddSignupPendingCountersAndEventLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PendingCount",
                table: "AffiliateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SignupCount",
                table: "AffiliateRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AffiliateEventLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffiliateRequestId = table.Column<int>(type: "int", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ExternalKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffiliateEventLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffiliateEventLogs_AffiliateRequests_AffiliateRequestId",
                        column: x => x.AffiliateRequestId,
                        principalTable: "AffiliateRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateEventLogs_AffiliateRequestId_Event_ExternalKey",
                table: "AffiliateEventLogs",
                columns: new[] { "AffiliateRequestId", "Event", "ExternalKey" },
                unique: true,
                filter: "[ExternalKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AffiliateEventLogs");

            migrationBuilder.DropColumn(
                name: "PendingCount",
                table: "AffiliateRequests");

            migrationBuilder.DropColumn(
                name: "SignupCount",
                table: "AffiliateRequests");
        }
    }
}
