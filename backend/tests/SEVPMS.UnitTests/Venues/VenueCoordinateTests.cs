using SEVPMS.Application.Features.Venues.DTOs;
using SEVPMS.Application.Features.Venues.Services;
using SEVPMS.Application.Interfaces.Repositories;
using SEVPMS.Domain.Entities.Venues;
using Xunit;

namespace SEVPMS.UnitTests.Venues;

public sealed class VenueCoordinateTests
{
    [Fact]
    public async Task Create_stores_valid_coordinates()
    {
        var repository = new FakeVenueRepository();

        var service = new VenueService(repository);

        var request = new CreateVenueRequest
        {
            Name = "Colombo Event Hall",
            Description = "Test venue",
            AddressLine1 = "123 Main Road",
            City = "Colombo",
            District = "Colombo",
            Country = "Sri Lanka",
            Latitude = 6.927079m,
            Longitude = 79.861244m,
            Capacity = 500
        };

        var result = await service.CreateAsync(
            Guid.NewGuid(),
            request);

        Assert.NotNull(repository.AddedVenue);

        Assert.Equal(
            6.927079m,
            repository.AddedVenue!.Latitude);

        Assert.Equal(
            79.861244m,
            repository.AddedVenue.Longitude);

        Assert.Equal(
            6.927079m,
            result.Latitude);

        Assert.Equal(
            79.861244m,
            result.Longitude);
    }

    [Fact]
    public async Task Create_rejects_invalid_latitude()
    {
        var service =
            new VenueService(
                new FakeVenueRepository());

        var request = new CreateVenueRequest
        {
            Name = "Invalid Venue",
            Description = "Test",
            AddressLine1 = "Test Road",
            City = "Colombo",
            District = "Colombo",
            Country = "Sri Lanka",
            Latitude = 91m,
            Longitude = 79m,
            Capacity = 100
        };

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    Guid.NewGuid(),
                    request));

        Assert.Contains(
            "Latitude",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_rejects_invalid_longitude()
    {
        var service =
            new VenueService(
                new FakeVenueRepository());

        var request = new CreateVenueRequest
        {
            Name = "Invalid Venue",
            Description = "Test",
            AddressLine1 = "Test Road",
            City = "Colombo",
            District = "Colombo",
            Country = "Sri Lanka",
            Latitude = 7m,
            Longitude = 181m,
            Capacity = 100
        };

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    Guid.NewGuid(),
                    request));

        Assert.Contains(
            "Longitude",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_requires_coordinates_to_be_provided_together()
    {
        var service =
            new VenueService(
                new FakeVenueRepository());

        var request = new CreateVenueRequest
        {
            Name = "Partial Coordinate Venue",
            Description = "Test",
            AddressLine1 = "Test Road",
            City = "Colombo",
            District = "Colombo",
            Country = "Sri Lanka",
            Latitude = 7m,
            Longitude = null,
            Capacity = 100
        };

        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.CreateAsync(
                    Guid.NewGuid(),
                    request));

        Assert.Contains(
            "provided together",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    private sealed class FakeVenueRepository
        : IVenueRepository
    {
        public Venue? AddedVenue
        {
            get;
            private set;
        }

        public Task<IReadOnlyList<Venue>>
            GetAllAsync(
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Venue>>(
                    Array.Empty<Venue>());

        public Task<IReadOnlyList<Venue>>
            GetByOwnerUserIdAsync(
                Guid ownerUserId,
                CancellationToken cancellationToken = default)
            => Task.FromResult<
                IReadOnlyList<Venue>>(
                    Array.Empty<Venue>());

        public Task<Venue?> GetByIdAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<Venue?>(null);

        public Task AddAsync(
            Venue venue,
            CancellationToken cancellationToken = default)
        {
            AddedVenue = venue;

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}