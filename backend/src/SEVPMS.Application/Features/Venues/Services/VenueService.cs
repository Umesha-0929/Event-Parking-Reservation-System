using SEVPMS.Application.Common.Exceptions;
using SEVPMS.Application.Features.Venues.DTOs;
using SEVPMS.Application.Features.Venues.Interfaces;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Venues;

namespace SEVPMS.Application.Features.Venues.Services;

public sealed class VenueService(
    IVenueRepository venueRepository)
    : IVenueService
{
    public async Task<IReadOnlyList<VenueResponse>>
        GetActiveVenuesAsync(
            CancellationToken cancellationToken = default)
    {
        var venues =
            await venueRepository.GetAllAsync(
                cancellationToken);

        return venues
            .Where(x => x.IsActive)
            .Select(Map)
            .ToList();
    }

    public async Task<VenueResponse> GetByIdAsync(
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var venue =
            await venueRepository.GetByIdAsync(
                venueId,
                cancellationToken);

        if (venue is null || !venue.IsActive)
        {
            throw new KeyNotFoundException(
                "Venue was not found.");
        }

        return Map(venue);
    }

    public async Task<IReadOnlyList<VenueResponse>>
        GetMyVenuesAsync(
            Guid ownerUserId,
            CancellationToken cancellationToken = default)
    {
        var venues =
            await venueRepository.GetByOwnerUserIdAsync(
                ownerUserId,
                cancellationToken);

        return venues
            .Select(Map)
            .ToList();
    }

    public async Task<VenueResponse> CreateAsync(
        Guid ownerUserId,
        CreateVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(
            request.Name,
            request.AddressLine1,
            request.City,
            request.District,
            request.Country,
            request.Capacity);

        var venue =
            new Venue
            {
                OwnerUserId = ownerUserId,
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 =
                    string.IsNullOrWhiteSpace(request.AddressLine2)
                        ? null
                        : request.AddressLine2.Trim(),
                City = request.City.Trim(),
                District = request.District.Trim(),
                Country = request.Country.Trim(),
                Capacity = request.Capacity,
                ContactPhone = request.ContactPhone?.Trim(),
                ContactEmail = request.ContactEmail?.Trim(),
                IsActive = true
            };

        await venueRepository.AddAsync(
            venue,
            cancellationToken);

        await venueRepository.SaveChangesAsync(
            cancellationToken);

        return Map(venue);
    }

    public async Task<VenueResponse> UpdateAsync(
        Guid ownerUserId,
        Guid venueId,
        UpdateVenueRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(
            request.Name,
            request.AddressLine1,
            request.City,
            request.District,
            request.Country,
            request.Capacity);

        var venue =
            await GetOwnedVenueAsync(
                ownerUserId,
                venueId,
                cancellationToken);

        venue.Name = request.Name.Trim();
        venue.Description = request.Description.Trim();
        venue.AddressLine1 = request.AddressLine1.Trim();
        venue.AddressLine2 =
            string.IsNullOrWhiteSpace(request.AddressLine2)
                ? null
                : request.AddressLine2.Trim();
        venue.City = request.City.Trim();
        venue.District = request.District.Trim();
        venue.Country = request.Country.Trim();
        venue.Capacity = request.Capacity;
        venue.ContactPhone = request.ContactPhone?.Trim();
        venue.ContactEmail = request.ContactEmail?.Trim();
        venue.UpdatedAtUtc = DateTime.UtcNow;

        await venueRepository.SaveChangesAsync(
            cancellationToken);

        return Map(venue);
    }

    public async Task DeactivateAsync(
        Guid ownerUserId,
        Guid venueId,
        CancellationToken cancellationToken = default)
    {
        var venue =
            await GetOwnedVenueAsync(
                ownerUserId,
                venueId,
                cancellationToken);

        venue.IsActive = false;
        venue.UpdatedAtUtc = DateTime.UtcNow;

        await venueRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Venue> GetOwnedVenueAsync(
        Guid ownerUserId,
        Guid venueId,
        CancellationToken cancellationToken)
    {
        var venue =
            await venueRepository.GetByIdAsync(
                venueId,
                cancellationToken);

        if (venue is null)
        {
            throw new KeyNotFoundException(
                "Venue was not found.");
        }

        if (venue.OwnerUserId != ownerUserId)
        {
            throw new ForbiddenAccessException(
                "You do not have permission to manage this venue.");
        }

        return venue;
    }

    private static void Validate(
        string name,
        string addressLine1,
        string city,
        string district,
        string country,
        int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Venue name is required.");
        }

        if (string.IsNullOrWhiteSpace(addressLine1))
        {
            throw new ArgumentException(
                "Venue address is required.");
        }

        if (string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(district) ||
            string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException(
                "Venue location details are required.");
        }

        if (capacity <= 0)
        {
            throw new ArgumentException(
                "Venue capacity must be greater than zero.");
        }
    }

    private static VenueResponse Map(
        Venue venue)
    {
        return new VenueResponse
        {
            VenueId = venue.Id,
            OwnerUserId = venue.OwnerUserId,
            Name = venue.Name,
            Description = venue.Description,
            AddressLine1 = venue.AddressLine1,
            AddressLine2 = venue.AddressLine2,
            City = venue.City,
            District = venue.District,
            Country = venue.Country,
            Capacity = venue.Capacity,
            ContactPhone = venue.ContactPhone,
            ContactEmail = venue.ContactEmail,
            IsActive = venue.IsActive,
            CreatedAtUtc = venue.CreatedAtUtc,
            UpdatedAtUtc = venue.UpdatedAtUtc
        };
    }
}