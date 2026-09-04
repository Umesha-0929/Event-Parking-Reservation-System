using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWaitlistEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WaitlistEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EligibleAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LeftAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConvertedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaitlistEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_CustomerUserId",
                table: "WaitlistEntries",
                column: "CustomerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_EventId",
                table: "WaitlistEntries",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_EventId_CustomerUserId",
                table: "WaitlistEntries",
                columns: new[] { "EventId", "CustomerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaitlistEntries_EventId_Status_CreatedAtUtc",
                table: "WaitlistEntries",
                columns: new[] { "EventId", "Status", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WaitlistEntries");
        }
    }
}
