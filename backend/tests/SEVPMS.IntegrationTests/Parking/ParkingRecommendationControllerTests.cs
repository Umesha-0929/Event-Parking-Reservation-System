using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using Xunit;

namespace SEVPMS.IntegrationTests.Parking;

public sealed class ParkingRecommendationControllerTests
{
    [Fact]
    public async Task Recommend_WhenRecommendationExists_ReturnsOk()
    {
        var userId = Guid.NewGuid();

        var request = new ParkingRecommendationRequest
        {
            VenueId = Guid.NewGuid(),
            EntranceNodeId = Guid.NewGuid(),
            RequiresAccessibleParking = true
        };

        var candidates = new List<ParkingRecommendationCandidateDto>
        {
            new()
            {
                ParkingSlotId = Guid.NewGuid(),
                ParkingZoneId = Guid.NewGuid(),
                SlotCode = "A-01",
                DistanceCost = 10,
                IsAvailable = true,
                IsAccessible = true,
                IsVehicleSuitable = true
            }
        };

        var recommendation = new ParkingRecommendationDto
        {
            ParkingSlotId = candidates[0].ParkingSlotId,
            ParkingZoneId = candidates[0].ParkingZoneId,
            SlotCode = "A-01",
            DistanceCost = 10,
            IsAccessible = true,
            Reason = "Nearest available accessible slot."
        };

        var provider = new FakeCandidateProvider
        {
            Candidates = candidates
        };

        var service = new FakeRecommendationService
        {
            Result = recommendation
        };

        var controller = CreateController(
            provider,
            service,
            userId);

        var result = await controller.Recommend(
            request,
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(
            result.Result);

        Assert.Same(recommendation, ok.Value);
        Assert.Equal(userId, provider.LastUserId);
        Assert.Same(request, provider.LastRequest);
        Assert.True(service.LastRequiresAccessibleParking);
    }

    [Fact]
    public async Task Recommend_WhenNoRecommendationExists_ReturnsNotFound()
    {
        var provider = new FakeCandidateProvider
        {
            Candidates = []
        };

        var service = new FakeRecommendationService
        {
            Result = null
        };

        var controller = CreateController(
            provider,
            service,
            Guid.NewGuid());

        var result = await controller.Recommend(
            new ParkingRecommendationRequest
            {
                VenueId = Guid.NewGuid(),
                EntranceNodeId = Guid.NewGuid()
            },
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(
            result.Result);
    }

    [Fact]
    public async Task Recommend_WhenUserIdClaimMissing_ReturnsUnauthorized()
    {
        var provider = new FakeCandidateProvider();
        var service = new FakeRecommendationService();

        var controller = CreateController(
            provider,
            service,
            null);

        var result = await controller.Recommend(
            new ParkingRecommendationRequest
            {
                VenueId = Guid.NewGuid(),
                EntranceNodeId = Guid.NewGuid()
            },
            CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(
            result.Result);

        Assert.Equal(0, provider.CallCount);
    }

    private static ParkingRecommendationController CreateController(
        IParkingRecommendationCandidateProvider provider,
        IParkingRecommendationService service,
        Guid? userId)
    {
        var controller =
            new ParkingRecommendationController(
                provider,
                service);

        ClaimsIdentity identity;

        if (userId.HasValue)
        {
            identity = new ClaimsIdentity(
                [
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.Value.ToString())
                ],
                "TestAuth");
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        controller.ControllerContext =
            new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

        return controller;
    }

    private sealed class FakeCandidateProvider
        : IParkingRecommendationCandidateProvider
    {
        public IReadOnlyList<ParkingRecommendationCandidateDto> Candidates
        {
            get;
            set;
        } = [];

        public ParkingRecommendationRequest? LastRequest
        {
            get;
            private set;
        }

        public Guid LastUserId
        {
            get;
            private set;
        }

        public int CallCount
        {
            get;
            private set;
        }

        public Task<IReadOnlyList<ParkingRecommendationCandidateDto>>
            GetCandidatesAsync(
                ParkingRecommendationRequest request,
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            LastUserId = userId;
            CallCount++;

            return Task.FromResult(Candidates);
        }
    }

    private sealed class FakeRecommendationService
        : IParkingRecommendationService
    {
        public ParkingRecommendationDto? Result
        {
            get;
            set;
        }

        public bool LastRequiresAccessibleParking
        {
            get;
            private set;
        }

        public ParkingRecommendationDto? RecommendBestSlot(
            IReadOnlyList<ParkingRecommendationCandidateDto> candidates,
            bool requiresAccessibleParking)
        {
            LastRequiresAccessibleParking =
                requiresAccessibleParking;

            return Result;
        }
    }
}