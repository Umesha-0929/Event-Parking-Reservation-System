using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Validators;
using Xunit;

namespace SEVPMS.IntegrationTests.Parking;

public sealed class ParkingReservationsControllerTests
{
    [Fact]
    public async Task Create_WhenParkingSlotConflict_Returns409Conflict()
    {
        var controller = CreateController(
            new ConflictParkingReservationService(),
            Guid.NewGuid());

        var request = new CreateParkingReservationRequest
        {
            BookingId = Guid.NewGuid(),
            ParkingSlotId = Guid.NewGuid(),
            VehicleRegistration = "CAB-1234"
        };

        var result = await controller.Create(
            request,
            CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(
            result.Result);
    }

    private static ParkingReservationsController CreateController(
        IParkingReservationService service,
        Guid userId)
    {
        var controller =
            new ParkingReservationsController(service);

        var identity =
            new ClaimsIdentity(
                [
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.ToString())
                ],
                "Test");

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext =
                    new DefaultHttpContext
                    {
                        User =
                            new ClaimsPrincipal(identity)
                    }
            };

        return controller;
    }

    private sealed class ConflictParkingReservationService
        : IParkingReservationService
    {
        public Task<ParkingReservationDto?> GetByIdAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ParkingReservationDto?>(
                null);
        }

        public Task<ParkingReservationDto> CreateAsync(
            Guid userId,
            CreateParkingReservationRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new ParkingReservationConflictException(
                "Parking slot is no longer available.");
        }

        public Task<ParkingReservationDto> MarkEnteredAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> MarkParkedAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> MarkExitedAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> CancelAsync(
            Guid userId,
            Guid reservationId,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ParkingReservationDto> ScanAsync(
            Guid userId,
            ParkingQrScanRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}