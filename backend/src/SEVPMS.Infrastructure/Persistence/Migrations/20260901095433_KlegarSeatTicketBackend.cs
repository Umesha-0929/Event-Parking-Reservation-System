using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class KlegarSeatTicketBackend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScannedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Gate = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ScannedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeatHolds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoldToken = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatHolds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowLabel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SeatNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    X = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Y = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TicketTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAccessible = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    SeatViewAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeatViewAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MediaUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ViewerType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DefaultYaw = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    DefaultPitch = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    DefaultFov = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    IsRepresentative = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatViewAssets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TicketNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    QrTokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckedInAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_EventId_ScannedAtUtc",
                table: "CheckIns",
                columns: new[] { "EventId", "ScannedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_TicketId_ScannedAtUtc",
                table: "CheckIns",
                columns: new[] { "TicketId", "ScannedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_HoldToken",
                table: "SeatHolds",
                column: "HoldToken");

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_SeatId_ExpiresAtUtc_Status",
                table: "SeatHolds",
                columns: new[] { "SeatId", "ExpiresAtUtc", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatHolds_SeatId_Status",
                table: "SeatHolds",
                columns: new[] { "SeatId", "Status" },
                unique: true,
                filter: "[Status] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_EventId_SectionId_RowLabel_SeatNumber",
                table: "Seats",
                columns: new[] { "EventId", "SectionId", "RowLabel", "SeatNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_EventId_SectionId_Status",
                table: "Seats",
                columns: new[] { "EventId", "SectionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatViewAssets_EventId_SeatId",
                table: "SeatViewAssets",
                columns: new[] { "EventId", "SeatId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatViewAssets_EventId_SectionId",
                table: "SeatViewAssets",
                columns: new[] { "EventId", "SectionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_BookingId",
                table: "Tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QrTokenHash",
                table: "Tickets",
                column: "QrTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketNo",
                table: "Tickets",
                column: "TicketNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "SeatHolds");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "SeatViewAssets");

            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
