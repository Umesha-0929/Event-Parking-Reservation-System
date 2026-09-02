using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    public partial class AddEventFoundation : Migration
    {
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    OrganizerUserId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    VenueId = table.Column<Guid>(
                        type: "uniqueidentifier",
                        nullable: false),

                    Title = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false),

                    Description = table.Column<string>(
                        type: "nvarchar(3000)",
                        maxLength: 3000,
                        nullable: false),

                    Category = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false),

                    StartAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    EndAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    Status = table.Column<string>(
                        type: "nvarchar(30)",
                        maxLength: 30,
                        nullable: false),

                    CreatedAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    UpdatedAtUtc = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_Events",
                        x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_OrganizerUserId",
                table: "Events",
                column: "OrganizerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartAtUtc",
                table: "Events",
                column: "StartAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Status",
                table: "Events",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");
        }

        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}