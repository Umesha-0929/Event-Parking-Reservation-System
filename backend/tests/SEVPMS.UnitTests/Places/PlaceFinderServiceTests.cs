using SEVPMS.Application.Features.Places.DTOs;
using SEVPMS.Application.Features.Places.Interfaces;
using SEVPMS.Application.Features.Places.Services;
using SEVPMS.Domain.Entities.Places;
using Xunit;

namespace SEVPMS.UnitTests.Places;

public sealed class PlaceFinderServiceTests
{
    [Fact]
    public async Task RecommendAsync_RanksAudienceMatchesByDistance()
    {
        var venueId = Guid.NewGuid();
        var repository = new FakePlaceRepository
        {
            Places =
            [
                CreatePlace(venueId, "Far Cafe", "Cafe", 4m, "Couple,Friends"),
                CreatePlace(venueId, "Near Cafe", "Cafe", 1m, "Couple")
            ]
        };

        var service = new PlaceFinderService(repository);
        var result = await service.RecommendAsync(
            venueId,
            new PlaceFinderRequest { AudienceMode = "Couple" });

        Assert.Equal(2, result.Count);
        Assert.Equal("Near Cafe", result[0].Name);
        Assert.Contains("Couple", result[0].RecommendationReason);
    }

    [Fact]
    public async Task BrowseAsync_AppliesCategoryAndDistanceFilters()
    {
        var venueId = Guid.NewGuid();
        var repository = new FakePlaceRepository
        {
            Places =
            [
                CreatePlace(venueId, "Cafe", "Cafe", 1m, "Individual"),
                CreatePlace(venueId, "Park", "Park", 1m, "FamilyChildren"),
                CreatePlace(venueId, "Far Cafe", "Cafe", 8m, "Individual")
            ]
        };

        var service = new PlaceFinderService(repository);
        var result = await service.BrowseAsync(
            venueId,
            new PlaceFinderRequest
            {
                Category = "Cafe",
                MaxDistanceKm = 5m
            });

        var item = Assert.Single(result);
        Assert.Equal("Cafe", item.Name);
    }

    private static NearbyPlace CreatePlace(
        Guid venueId,
        string name,
        string category,
        decimal distance,
        string modes)
        => new()
        {
            VenueId = venueId,
            Name = name,
            Category = category,
            DistanceKm = distance,
            AudienceModesCsv = modes,
            IsOpen = true,
            IsActive = true
        };

    private sealed class FakePlaceRepository : IPlaceRepository
    {
        public IReadOnlyList<NearbyPlace> Places { get; init; } = [];

        public Task<IReadOnlyList<NearbyPlace>> GetByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<NearbyPlace>>(
                Places.Where(x => x.VenueId == venueId).ToList());

        public Task<NearbyPlace?> GetByIdAsync(
            Guid placeId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Places.FirstOrDefault(x => x.Id == placeId));

        public Task AddAsync(NearbyPlace place, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public void Update(NearbyPlace place) { }
        public void Remove(NearbyPlace place) { }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
