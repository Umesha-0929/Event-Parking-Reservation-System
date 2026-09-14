using Microsoft.AspNetCore.Mvc;
using SEVPMS.Api.Controllers;
using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using Xunit;

namespace SEVPMS.IntegrationTests.Parking;

public sealed class ParkingNavigationControllerTests
{
    [Fact]
    public async Task GetRoute_WhenRouteExists_ReturnsOk()
    {
        var venueId = Guid.NewGuid();
        var startNodeId = Guid.NewGuid();
        var endNodeId = Guid.NewGuid();

        var expectedRoute = new ParkingRouteDto
        {
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            TotalCost = 15,
            Nodes =
            [
                new ParkingNodeDto
                {
                    Id = startNodeId,
                    VenueId = venueId,
                    NodeCode = "ENTRY",
                    X = 0,
                    Y = 0,
                    NodeType = "Gate"
                },
                new ParkingNodeDto
                {
                    Id = endNodeId,
                    VenueId = venueId,
                    NodeCode = "B2-128",
                    X = 10,
                    Y = 5,
                    NodeType = "SlotConnector"
                }
            ]
        };

        var service = new FakeParkingRouteService
        {
            Route = expectedRoute
        };

        var controller =
            new ParkingNavigationController(service);

        var result = await controller.GetRoute(
            venueId,
            startNodeId,
            endNodeId,
            false,
            CancellationToken.None);

        var okResult =
            Assert.IsType<OkObjectResult>(result.Result);

        var route =
            Assert.IsType<ParkingRouteDto>(okResult.Value);

        Assert.Equal(startNodeId, route.StartNodeId);
        Assert.Equal(endNodeId, route.EndNodeId);
        Assert.Equal(15, route.TotalCost);
    }

    [Fact]
    public async Task GetRoute_WhenRouteDoesNotExist_ReturnsNotFound()
    {
        var service = new FakeParkingRouteService();

        var controller =
            new ParkingNavigationController(service);

        var result = await controller.GetRoute(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            false,
            CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetRoute_PassesAccessibleOnlyToService()
    {
        var venueId = Guid.NewGuid();
        var startNodeId = Guid.NewGuid();
        var endNodeId = Guid.NewGuid();

        var service = new FakeParkingRouteService
        {
            Route = new ParkingRouteDto
            {
                StartNodeId = startNodeId,
                EndNodeId = endNodeId,
                TotalCost = 5,
                Nodes = []
            }
        };

        var controller =
            new ParkingNavigationController(service);

        await controller.GetRoute(
            venueId,
            startNodeId,
            endNodeId,
            true,
            CancellationToken.None);

        Assert.True(service.LastAccessibleOnly);
        Assert.Equal(venueId, service.LastVenueId);
        Assert.Equal(startNodeId, service.LastStartNodeId);
        Assert.Equal(endNodeId, service.LastEndNodeId);
    }

    private sealed class FakeParkingRouteService
        : IParkingRouteService
    {
        public ParkingRouteDto? Route { get; init; }

        public Guid LastVenueId { get; private set; }

        public Guid LastStartNodeId { get; private set; }

        public Guid LastEndNodeId { get; private set; }

        public bool LastAccessibleOnly { get; private set; }

        public Task<ParkingRouteDto?> FindRouteAsync(
            Guid venueId,
            Guid startNodeId,
            Guid endNodeId,
            bool accessibleOnly,
            CancellationToken cancellationToken = default)
        {
            LastVenueId = venueId;
            LastStartNodeId = startNodeId;
            LastEndNodeId = endNodeId;
            LastAccessibleOnly = accessibleOnly;

            return Task.FromResult(Route);
        }
    }
}