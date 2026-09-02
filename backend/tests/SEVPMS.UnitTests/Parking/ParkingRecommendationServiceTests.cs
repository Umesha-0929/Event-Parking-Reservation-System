using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Services;
using Xunit;

namespace SEVPMS.UnitTests.Parking;

public sealed class ParkingRecommendationServiceTests
{
    [Fact]
    public void RecommendBestSlot_ReturnsNearestEligibleSlot()
    {
        var service = new ParkingRecommendationService();

        var candidates = new List<ParkingRecommendationCandidateDto>
        {
            CreateCandidate("A-01", 20),
            CreateCandidate("A-02", 10)
        };

        var result = service.RecommendBestSlot(
            candidates,
            false);

        Assert.NotNull(result);
        Assert.Equal("A-02", result.SlotCode);
        Assert.Equal(10, result.DistanceCost);
    }

    [Fact]
    public void RecommendBestSlot_IgnoresUnavailableSlots()
    {
        var service = new ParkingRecommendationService();

        var unavailable = CreateCandidate("A-01", 5);
        unavailable.IsAvailable = false;

        var available = CreateCandidate("A-02", 15);

        var result = service.RecommendBestSlot(
            [unavailable, available],
            false);

        Assert.NotNull(result);
        Assert.Equal("A-02", result.SlotCode);
    }

    [Fact]
    public void RecommendBestSlot_IgnoresVehicleUnsuitableSlots()
    {
        var service = new ParkingRecommendationService();

        var unsuitable = CreateCandidate("A-01", 5);
        unsuitable.IsVehicleSuitable = false;

        var suitable = CreateCandidate("A-02", 15);

        var result = service.RecommendBestSlot(
            [unsuitable, suitable],
            false);

        Assert.NotNull(result);
        Assert.Equal("A-02", result.SlotCode);
    }

    [Fact]
    public void RecommendBestSlot_WhenAccessibleRequired_UsesAccessibleSlot()
    {
        var service = new ParkingRecommendationService();

        var normal = CreateCandidate("A-01", 5);
        normal.IsAccessible = false;

        var accessible = CreateCandidate("A-02", 20);
        accessible.IsAccessible = true;

        var result = service.RecommendBestSlot(
            [normal, accessible],
            true);

        Assert.NotNull(result);
        Assert.Equal("A-02", result.SlotCode);
        Assert.True(result.IsAccessible);
    }

    [Fact]
    public void RecommendBestSlot_WhenNoEligibleSlot_ReturnsNull()
    {
        var service = new ParkingRecommendationService();

        var candidate = CreateCandidate("A-01", 5);
        candidate.IsAvailable = false;

        var result = service.RecommendBestSlot(
            [candidate],
            false);

        Assert.Null(result);
    }

    [Fact]
    public void RecommendBestSlot_WhenDistanceEqual_UsesSlotCodeAsTieBreaker()
    {
        var service = new ParkingRecommendationService();

        var candidates = new List<ParkingRecommendationCandidateDto>
        {
            CreateCandidate("B-02", 10),
            CreateCandidate("B-01", 10)
        };

        var result = service.RecommendBestSlot(
            candidates,
            false);

        Assert.NotNull(result);
        Assert.Equal("B-01", result.SlotCode);
    }

    private static ParkingRecommendationCandidateDto CreateCandidate(
        string slotCode,
        decimal distanceCost)
    {
        return new ParkingRecommendationCandidateDto
        {
            ParkingSlotId = Guid.NewGuid(),
            ParkingZoneId = Guid.NewGuid(),
            SlotCode = slotCode,
            DistanceCost = distanceCost,
            IsAvailable = true,
            IsAccessible = true,
            IsVehicleSuitable = true
        };
    }
}