using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations;

public partial class BoundOperationalTextColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        Alter(migrationBuilder, "EventFoodStalls", "StallName", 160, false);
        Alter(migrationBuilder, "FoodOrders", "FulfillmentType", 40, false);
        Alter(migrationBuilder, "FoodOrders", "SeatLabelSnapshot", 120, true);
        Alter(migrationBuilder, "FoodOrderItems", "ItemNameSnapshot", 200, false);
        Alter(migrationBuilder, "FoodVendors", "Description", 2000, false);
        Alter(migrationBuilder, "FoodVendors", "Name", 160, false);
        Alter(migrationBuilder, "FoodVendors", "Status", 40, false);
        Alter(migrationBuilder, "MenuItems", "Currency", 10, false);
        Alter(migrationBuilder, "MenuItems", "Description", 2000, false);
        Alter(migrationBuilder, "MenuItems", "ImageUrl", 1000, false);
        Alter(migrationBuilder, "MenuItems", "Name", 200, false);
        Alter(migrationBuilder, "ParkingNodes", "NodeCode", 80, false);
        Alter(migrationBuilder, "ParkingNodes", "NodeType", 40, false);
        Alter(migrationBuilder, "ParkingReservations", "VehicleRegSnapshot", 40, false);
        Alter(migrationBuilder, "ParkingSlots", "SlotCode", 40, false);
        Alter(migrationBuilder, "ParkingSlots", "Status", 30, false);
        Alter(migrationBuilder, "ParkingZones", "EntranceName", 160, false);
        Alter(migrationBuilder, "ParkingZones", "Level", 80, false);
        Alter(migrationBuilder, "ParkingZones", "Name", 160, false);
        Alter(migrationBuilder, "SavedVehicles", "Nickname", 100, false);
        Alter(migrationBuilder, "SavedVehicles", "RegistrationNo", 40, false);
        Alter(migrationBuilder, "SavedVehicles", "VehicleType", 80, false);

        // Create uniqueness indexes only after their text key columns are bounded.
        migrationBuilder.CreateIndex(
            name: "UX_SavedVehicles_UserId_RegistrationNo",
            table: "SavedVehicles",
            columns: new[] { "UserId", "RegistrationNo" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "UX_ParkingZones_VenueId_Name_Level",
            table: "ParkingZones",
            columns: new[] { "VenueId", "Name", "Level" },
            unique: true);
        migrationBuilder.CreateIndex(
            name: "UX_ParkingSlots_ParkingZoneId_SlotCode",
            table: "ParkingSlots",
            columns: new[] { "ParkingZoneId", "SlotCode" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("UX_ParkingSlots_ParkingZoneId_SlotCode", "ParkingSlots");
        migrationBuilder.DropIndex("UX_ParkingZones_VenueId_Name_Level", "ParkingZones");
        migrationBuilder.DropIndex("UX_SavedVehicles_UserId_RegistrationNo", "SavedVehicles");

        Unbound(migrationBuilder, "EventFoodStalls", "StallName", 160, false);
        Unbound(migrationBuilder, "FoodOrders", "FulfillmentType", 40, false);
        Unbound(migrationBuilder, "FoodOrders", "SeatLabelSnapshot", 120, true);
        Unbound(migrationBuilder, "FoodOrderItems", "ItemNameSnapshot", 200, false);
        Unbound(migrationBuilder, "FoodVendors", "Description", 2000, false);
        Unbound(migrationBuilder, "FoodVendors", "Name", 160, false);
        Unbound(migrationBuilder, "FoodVendors", "Status", 40, false);
        Unbound(migrationBuilder, "MenuItems", "Currency", 10, false);
        Unbound(migrationBuilder, "MenuItems", "Description", 2000, false);
        Unbound(migrationBuilder, "MenuItems", "ImageUrl", 1000, false);
        Unbound(migrationBuilder, "MenuItems", "Name", 200, false);
        Unbound(migrationBuilder, "ParkingNodes", "NodeCode", 80, false);
        Unbound(migrationBuilder, "ParkingNodes", "NodeType", 40, false);
        Unbound(migrationBuilder, "ParkingReservations", "VehicleRegSnapshot", 40, false);
        Unbound(migrationBuilder, "ParkingSlots", "SlotCode", 40, false);
        Unbound(migrationBuilder, "ParkingSlots", "Status", 30, false);
        Unbound(migrationBuilder, "ParkingZones", "EntranceName", 160, false);
        Unbound(migrationBuilder, "ParkingZones", "Level", 80, false);
        Unbound(migrationBuilder, "ParkingZones", "Name", 160, false);
        Unbound(migrationBuilder, "SavedVehicles", "Nickname", 100, false);
        Unbound(migrationBuilder, "SavedVehicles", "RegistrationNo", 40, false);
        Unbound(migrationBuilder, "SavedVehicles", "VehicleType", 80, false);
    }

    private static void Alter(MigrationBuilder migrationBuilder, string table, string column, int maxLength, bool nullable)
        => migrationBuilder.AlterColumn<string>(
            name: column,
            table: table,
            type: $"nvarchar({maxLength})",
            maxLength: maxLength,
            nullable: nullable,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: nullable);

    private static void Unbound(MigrationBuilder migrationBuilder, string table, string column, int oldMaxLength, bool nullable)
        => migrationBuilder.AlterColumn<string>(
            name: column,
            table: table,
            type: "nvarchar(max)",
            nullable: nullable,
            oldClrType: typeof(string),
            oldType: $"nvarchar({oldMaxLength})",
            oldMaxLength: oldMaxLength,
            oldNullable: nullable);
}
