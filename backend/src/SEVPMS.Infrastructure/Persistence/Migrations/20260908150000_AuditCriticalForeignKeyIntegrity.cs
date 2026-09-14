using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SEVPMS.Infrastructure.Persistence.Migrations;

public partial class AuditCriticalForeignKeyIntegrity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Identity / ownership boundaries.
        migrationBuilder.AddForeignKey("FK_Venues_Users_OwnerUserId", "Venues", "OwnerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Events_Users_OrganizerUserId", "Events", "OrganizerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Events_Venues_VenueId", "Events", "VenueId", "Venues", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

        // Booking / seating integrity.
        migrationBuilder.AddForeignKey("FK_Bookings_Users_CustomerUserId", "Bookings", "CustomerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Bookings_Events_EventId", "Bookings", "EventId", "Events", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_BookingSeats_Bookings_BookingId", "BookingSeats", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_BookingSeats_Seats_SeatId", "BookingSeats", "SeatId", "Seats", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

        // Tickets / admission integrity.
        migrationBuilder.AddForeignKey("FK_Tickets_Bookings_BookingId", "Tickets", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Tickets_Events_EventId", "Tickets", "EventId", "Events", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Tickets_Seats_SeatId", "Tickets", "SeatId", "Seats", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_CheckIns_Tickets_TicketId", "CheckIns", "TicketId", "Tickets", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_CheckIns_Events_EventId", "CheckIns", "EventId", "Events", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_CheckIns_Users_ScannedByUserId", "CheckIns", "ScannedByUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

        // Payment / receipt integrity.
        migrationBuilder.AddForeignKey("FK_Payments_Bookings_BookingId", "Payments", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Payments_Users_CustomerUserId", "Payments", "CustomerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_PaymentTransactions_Payments_PaymentId", "PaymentTransactions", "PaymentId", "Payments", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_PaymentTransactions_Bookings_BookingId", "PaymentTransactions", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_PaymentTransactions_Users_CustomerUserId", "PaymentTransactions", "CustomerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Refunds_Payments_PaymentId", "Refunds", "PaymentId", "Payments", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Refunds_Bookings_BookingId", "Refunds", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Refunds_Users_CustomerUserId", "Refunds", "CustomerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Receipts_Payments_PaymentId", "Receipts", "PaymentId", "Payments", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Receipts_Bookings_BookingId", "Receipts", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_Receipts_Users_CustomerUserId", "Receipts", "CustomerUserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

        // Parking / vehicle integrity.
        migrationBuilder.AddForeignKey("FK_SavedVehicles_Users_UserId", "SavedVehicles", "UserId", "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingZones_Venues_VenueId", "ParkingZones", "VenueId", "Venues", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingZones_Events_EventId", "ParkingZones", "EventId", "Events", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingSlots_ParkingZones_ParkingZoneId", "ParkingSlots", "ParkingZoneId", "ParkingZones", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingSlots_Events_EventId", "ParkingSlots", "EventId", "Events", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingReservations_Bookings_BookingId", "ParkingReservations", "BookingId", "Bookings", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingReservations_ParkingSlots_ParkingSlotId", "ParkingReservations", "ParkingSlotId", "ParkingSlots", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey("FK_ParkingReservations_SavedVehicles_VehicleId", "ParkingReservations", "VehicleId", "SavedVehicles", principalColumn: "Id", onDelete: ReferentialAction.Restrict);

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        string[] foreignKeys =
        {
            "FK_ParkingReservations_SavedVehicles_VehicleId", "FK_ParkingReservations_ParkingSlots_ParkingSlotId", "FK_ParkingReservations_Bookings_BookingId",
            "FK_ParkingSlots_Events_EventId", "FK_ParkingSlots_ParkingZones_ParkingZoneId", "FK_ParkingZones_Events_EventId", "FK_ParkingZones_Venues_VenueId", "FK_SavedVehicles_Users_UserId",
            "FK_Receipts_Users_CustomerUserId", "FK_Receipts_Bookings_BookingId", "FK_Receipts_Payments_PaymentId",
            "FK_Refunds_Users_CustomerUserId", "FK_Refunds_Bookings_BookingId", "FK_Refunds_Payments_PaymentId",
            "FK_PaymentTransactions_Users_CustomerUserId", "FK_PaymentTransactions_Bookings_BookingId", "FK_PaymentTransactions_Payments_PaymentId",
            "FK_Payments_Users_CustomerUserId", "FK_Payments_Bookings_BookingId",
            "FK_CheckIns_Users_ScannedByUserId", "FK_CheckIns_Events_EventId", "FK_CheckIns_Tickets_TicketId",
            "FK_Tickets_Seats_SeatId", "FK_Tickets_Events_EventId", "FK_Tickets_Bookings_BookingId",
            "FK_BookingSeats_Seats_SeatId", "FK_BookingSeats_Bookings_BookingId", "FK_Bookings_Events_EventId", "FK_Bookings_Users_CustomerUserId",
            "FK_Events_Venues_VenueId", "FK_Events_Users_OrganizerUserId", "FK_Venues_Users_OwnerUserId"
        };

        foreach (var fk in foreignKeys)
        {
            var table = fk.Split('_')[1];
            migrationBuilder.DropForeignKey(fk, table);
        }
    }
}
