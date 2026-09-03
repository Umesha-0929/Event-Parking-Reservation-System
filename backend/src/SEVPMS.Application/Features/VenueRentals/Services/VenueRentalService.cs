using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Notifications.Interfaces;
using SEVPMS.Application.Features.VenueRentals.DTOs;
using SEVPMS.Application.Features.VenueRentals.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.VenueRentals;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.VenueRentals.Services;

public sealed class VenueRentalService(
    IVenueRentalRepository rentalRepository,
    IVenueRepository venueRepository,
    INotificationService notificationService,
    IVenueMarketplaceRepository? marketplaceRepository = null)
    : IVenueRentalService
{
    public async Task<IReadOnlyList<VenueRentalResponse>> GetMineAsync(
        Guid organizerUserId,
        CancellationToken cancellationToken = default)
    {
        var rentals = await rentalRepository.GetByOrganizerAsync(
            organizerUserId,
            cancellationToken);

        return rentals.Select(Map).ToList();
    }

    public async Task<IReadOnlyList<VenueRentalResponse>> GetIncomingAsync(
        Guid venueOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var venues = await venueRepository.GetByOwnerUserIdAsync(
            venueOwnerUserId,
            cancellationToken);

        var venueIds = venues.Select(x => x.Id).ToArray();

        var rentals = await rentalRepository.GetByVenueIdsAsync(
            venueIds,
            cancellationToken);

        return rentals.Select(Map).ToList();
    }

    public async Task<VenueRentalResponse> CreateAsync(
        Guid organizerUserId,
        CreateVenueRentalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.VenueId == Guid.Empty)
            throw new ArgumentException("Venue is required.");

        if (request.StartAtUtc == default ||
            request.EndAtUtc == default ||
            request.EndAtUtc <= request.StartAtUtc)
        {
            throw new ArgumentException(
                "Rental end time must be later than start time.");
        }

        if (string.IsNullOrWhiteSpace(request.Purpose))
            throw new ArgumentException("Rental purpose is required.");

        if (request.OfferedAmount < 0)
            throw new ArgumentException("Offered amount cannot be negative.");

        var venue = await venueRepository.GetByIdAsync(
            request.VenueId,
            cancellationToken);

        if (venue is null || !venue.IsActive)
            throw new ArgumentException("Venue does not exist or is inactive.");

        if (marketplaceRepository is not null &&
            await marketplaceRepository.HasBlockingAvailabilityAsync(
                request.VenueId,
                request.StartAtUtc,
                request.EndAtUtc,
                cancellationToken))
        {
            throw new InvalidOperationException(
                "The venue is blocked or under maintenance during the requested time.");
        }

        var rental = new VenueRentalRequest
        {
            OrganizerUserId = organizerUserId,
            VenueId = request.VenueId,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            Purpose = request.Purpose.Trim(),
            OfferedAmount = request.OfferedAmount,
            Status = RentalRequestStatus.Pending
        };

        await rentalRepository.AddAsync(rental, cancellationToken);
        await rentalRepository.SaveChangesAsync(cancellationToken);
        return Map(rental);
    }

    public async Task<VenueRentalResponse> UpdateStatusAsync(
        Guid venueOwnerUserId,
        Guid rentalId,
        UpdateVenueRentalStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Status is RentalRequestStatus.Pending or RentalRequestStatus.Cancelled)
        {
            throw new ArgumentException(
                "Venue owner can set only Accepted, Rejected, or Negotiating.");
        }

        var rental = await rentalRepository.GetByIdAsync(rentalId, cancellationToken)
            ?? throw new KeyNotFoundException("Venue rental request was not found.");

        var venue = await venueRepository.GetByIdAsync(rental.VenueId, cancellationToken)
            ?? throw new KeyNotFoundException("Venue was not found.");

        if (venue.OwnerUserId != venueOwnerUserId)
            throw new ForbiddenAccessException("You do not own this venue.");

        if (rental.Status is RentalRequestStatus.Cancelled or RentalRequestStatus.Rejected)
            throw new InvalidOperationException("This rental request can no longer be updated.");

        if (request.Status == RentalRequestStatus.Accepted)
        {
            var hasConflict = await rentalRepository.HasAcceptedOverlapAsync(
                rental.VenueId,
                rental.StartAtUtc,
                rental.EndAtUtc,
                rental.Id,
                cancellationToken);

            if (hasConflict)
            {
                throw new InvalidOperationException(
                    "The venue already has an accepted rental during this time.");
            }

            if (marketplaceRepository is not null &&
                await marketplaceRepository.HasBlockingAvailabilityAsync(
                    rental.VenueId,
                    rental.StartAtUtc,
                    rental.EndAtUtc,
                    cancellationToken))
            {
                throw new InvalidOperationException(
                    "The venue is blocked or under maintenance during this time.");
            }
        }

        rental.Status = request.Status;
        rental.OwnerMessage =
            string.IsNullOrWhiteSpace(request.OwnerMessage)
                ? null
                : request.OwnerMessage.Trim();
        rental.UpdatedAtUtc = DateTime.UtcNow;

        await rentalRepository.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            rental.OrganizerUserId,
            "Venue rental updated",
            $"Your venue rental request is now {rental.Status}.",
            "VenueRental",
            cancellationToken);

        return Map(rental);
    }

    public async Task<VenueRentalResponse> CancelAsync(
        Guid organizerUserId,
        Guid rentalId,
        CancellationToken cancellationToken = default)
    {
        var rental = await rentalRepository.GetByIdAsync(rentalId, cancellationToken)
            ?? throw new KeyNotFoundException("Venue rental request was not found.");

        if (rental.OrganizerUserId != organizerUserId)
            throw new ForbiddenAccessException("You do not own this rental request.");

        if (rental.Status is RentalRequestStatus.Cancelled or RentalRequestStatus.Rejected)
            return Map(rental);

        rental.Status = RentalRequestStatus.Cancelled;
        rental.UpdatedAtUtc = DateTime.UtcNow;

        await rentalRepository.SaveChangesAsync(cancellationToken);
        return Map(rental);
    }

    private static VenueRentalResponse Map(VenueRentalRequest rental) => new()
    {
        RentalRequestId = rental.Id,
        OrganizerUserId = rental.OrganizerUserId,
        VenueId = rental.VenueId,
        StartAtUtc = rental.StartAtUtc,
        EndAtUtc = rental.EndAtUtc,
        Purpose = rental.Purpose,
        OfferedAmount = rental.OfferedAmount,
        Status = rental.Status,
        OwnerMessage = rental.OwnerMessage,
        CreatedAtUtc = rental.CreatedAtUtc,
        UpdatedAtUtc = rental.UpdatedAtUtc
    };
}
