using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Klegar2DSeatingLayout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RowLabel",
                table: "SeatViewAssets",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Y",
                table: "Seats",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "X",
                table: "Seats",
                type: "decimal(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Seats",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(24)",
                oldMaxLength: 24);

            migrationBuilder.AlterColumn<string>(
                name: "RowLabel",
                table: "Seats",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<int>(
                name: "ColumnNumber",
                table: "Seats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowNumber",
                table: "Seats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SeatCategoryId",
                table: "Seats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SeatingLayoutId",
                table: "Seats",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SeatCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatingLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeatingLayouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ColumnCount = table.Column<int>(type: "int", nullable: false),
                    CanvasWidth = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CanvasHeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    StageX = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    StageY = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    StageWidth = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    StageHeight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatingLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeatSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeatingLayoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RowCount = table.Column<int>(type: "int", nullable: false),
                    ColumnCount = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Y = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Width = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Height = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsAccessibleSection = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeatViewAssets_EventId_SectionId_RowLabel",
                table: "SeatViewAssets",
                columns: new[] { "EventId", "SectionId", "RowLabel" });

            migrationBuilder.CreateIndex(
                name: "IX_Seats_EventId_SectionId_RowNumber_ColumnNumber",
                table: "Seats",
                columns: new[] { "EventId", "SectionId", "RowNumber", "ColumnNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_SeatingLayoutId_SectionId_SeatCategoryId",
                table: "Seats",
                columns: new[] { "SeatingLayoutId", "SectionId", "SeatCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatCategories_EventId_DisplayOrder",
                table: "SeatCategories",
                columns: new[] { "EventId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatCategories_SeatingLayoutId_Code",
                table: "SeatCategories",
                columns: new[] { "SeatingLayoutId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeatingLayouts_EventId",
                table: "SeatingLayouts",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeatingLayouts_EventId_IsPublished",
                table: "SeatingLayouts",
                columns: new[] { "EventId", "IsPublished" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatSections_EventId_DisplayOrder",
                table: "SeatSections",
                columns: new[] { "EventId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SeatSections_SeatingLayoutId_Code",
                table: "SeatSections",
                columns: new[] { "SeatingLayoutId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeatCategories");

            migrationBuilder.DropTable(
                name: "SeatingLayouts");

            migrationBuilder.DropTable(
                name: "SeatSections");

            migrationBuilder.DropIndex(
                name: "IX_SeatViewAssets_EventId_SectionId_RowLabel",
                table: "SeatViewAssets");

            migrationBuilder.DropIndex(
                name: "IX_Seats_EventId_SectionId_RowNumber_ColumnNumber",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Seats_SeatingLayoutId_SectionId_SeatCategoryId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "RowLabel",
                table: "SeatViewAssets");

            migrationBuilder.DropColumn(
                name: "ColumnNumber",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "RowNumber",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "SeatCategoryId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "SeatingLayoutId",
                table: "Seats");

            migrationBuilder.AlterColumn<decimal>(
                name: "Y",
                table: "Seats",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "X",
                table: "Seats",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)",
                oldPrecision: 12,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Seats",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "RowLabel",
                table: "Seats",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
