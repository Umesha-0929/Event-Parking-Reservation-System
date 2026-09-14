using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Application.Features.Parking.Services;
using SEVPMS.Domain.Entities.Parking;
using Xunit;

namespace SEVPMS.UnitTests.Parking;

public sealed class ParkingRouteServiceTests
{
    [Fact]
    public async Task FindRouteAsync_ReturnsShortestRoute()
    {
        var venueId = Guid.NewGuid();

        var nodeA = CreateNode(venueId, "A");
        var nodeB = CreateNode(venueId, "B");
        var nodeC = CreateNode(venueId, "C");

        var repository = new FakeParkingRouteRepository
        {
            Nodes = [nodeA, nodeB, nodeC],
            Edges =
            [
                CreateEdge(
                    venueId,
                    nodeA.Id,
                    nodeB.Id,
                    10),

                CreateEdge(
                    venueId,
                    nodeB.Id,
                    nodeC.Id,
                    5),

                CreateEdge(
                    venueId,
                    nodeA.Id,
                    nodeC.Id,
                    30)
            ]
        };

        var service = new ParkingRouteService(repository);

        var result = await service.FindRouteAsync(
            venueId,
            nodeA.Id,
            nodeC.Id,
            false);

        Assert.NotNull(result);

        Assert.Equal(15, result.TotalCost);
        Assert.Equal(3, result.Nodes.Count);

        Assert.Equal(nodeA.Id, result.Nodes[0].Id);
        Assert.Equal(nodeB.Id, result.Nodes[1].Id);
        Assert.Equal(nodeC.Id, result.Nodes[2].Id);
    }

    [Fact]
    public async Task FindRouteAsync_IgnoresBlockedEdges()
    {
        var venueId = Guid.NewGuid();

        var nodeA = CreateNode(venueId, "A");
        var nodeB = CreateNode(venueId, "B");
        var nodeC = CreateNode(venueId, "C");

        var blockedEdge = CreateEdge(
            venueId,
            nodeA.Id,
            nodeC.Id,
            1);

        blockedEdge.IsBlocked = true;

        var repository = new FakeParkingRouteRepository
        {
            Nodes = [nodeA, nodeB, nodeC],
            Edges =
            [
                blockedEdge,

                CreateEdge(
                    venueId,
                    nodeA.Id,
                    nodeB.Id,
                    5),

                CreateEdge(
                    venueId,
                    nodeB.Id,
                    nodeC.Id,
                    5)
            ]
        };

        var service = new ParkingRouteService(repository);

        var result = await service.FindRouteAsync(
            venueId,
            nodeA.Id,
            nodeC.Id,
            false);

        Assert.NotNull(result);

        Assert.Equal(10, result.TotalCost);
        Assert.Equal(3, result.Nodes.Count);
        Assert.Equal(nodeB.Id, result.Nodes[1].Id);
    }

    [Fact]
    public async Task FindRouteAsync_WhenAccessibleOnly_UsesAccessibleEdges()
    {
        var venueId = Guid.NewGuid();

        var nodeA = CreateNode(venueId, "A");
        var nodeB = CreateNode(venueId, "B");
        var nodeC = CreateNode(venueId, "C");

        var nonAccessibleEdge = CreateEdge(
            venueId,
            nodeA.Id,
            nodeC.Id,
            1);

        nonAccessibleEdge.IsAccessible = false;

        var accessibleFirst = CreateEdge(
            venueId,
            nodeA.Id,
            nodeB.Id,
            4);

        accessibleFirst.IsAccessible = true;

        var accessibleSecond = CreateEdge(
            venueId,
            nodeB.Id,
            nodeC.Id,
            4);

        accessibleSecond.IsAccessible = true;

        var repository = new FakeParkingRouteRepository
        {
            Nodes = [nodeA, nodeB, nodeC],
            Edges =
            [
                nonAccessibleEdge,
                accessibleFirst,
                accessibleSecond
            ]
        };

        var service = new ParkingRouteService(repository);

        var result = await service.FindRouteAsync(
            venueId,
            nodeA.Id,
            nodeC.Id,
            true);

        Assert.NotNull(result);

        Assert.Equal(8, result.TotalCost);
        Assert.Equal(3, result.Nodes.Count);
        Assert.Equal(nodeB.Id, result.Nodes[1].Id);
    }

    [Fact]
    public async Task FindRouteAsync_WhenNoRouteExists_ReturnsNull()
    {
        var venueId = Guid.NewGuid();

        var nodeA = CreateNode(venueId, "A");
        var nodeB = CreateNode(venueId, "B");

        var repository = new FakeParkingRouteRepository
        {
            Nodes = [nodeA, nodeB],
            Edges = []
        };

        var service = new ParkingRouteService(repository);

        var result = await service.FindRouteAsync(
            venueId,
            nodeA.Id,
            nodeB.Id,
            false);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindRouteAsync_WhenStartEqualsEnd_ReturnsZeroCost()
    {
        var venueId = Guid.NewGuid();

        var node = CreateNode(
            venueId,
            "ENTRY");

        var repository = new FakeParkingRouteRepository
        {
            Nodes = [node],
            Edges = []
        };

        var service = new ParkingRouteService(repository);

        var result = await service.FindRouteAsync(
            venueId,
            node.Id,
            node.Id,
            false);

        Assert.NotNull(result);

        Assert.Equal(0, result.TotalCost);

        var routeNode =
            Assert.Single(result.Nodes);

        Assert.Equal(node.Id, routeNode.Id);
    }

    private static ParkingNode CreateNode(
        Guid venueId,
        string nodeCode)
    {
        return new ParkingNode
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            NodeCode = nodeCode,
            X = 0,
            Y = 0,
            NodeType = "Intersection"
        };
    }

    private static ParkingEdge CreateEdge(
        Guid venueId,
        Guid fromNodeId,
        Guid toNodeId,
        decimal cost)
    {
        return new ParkingEdge
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            FromNodeId = fromNodeId,
            ToNodeId = toNodeId,
            Cost = cost,
            IsBidirectional = false,
            IsAccessible = true,
            IsBlocked = false
        };
    }

    private sealed class FakeParkingRouteRepository
        : IParkingRouteRepository
    {
        public IReadOnlyList<ParkingNode> Nodes { get; init; }
            = [];

        public IReadOnlyList<ParkingEdge> Edges { get; init; }
            = [];

        public Task<IReadOnlyList<ParkingNode>> GetNodesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ParkingNode> result = Nodes
                .Where(node => node.VenueId == venueId)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<ParkingEdge>> GetEdgesByVenueAsync(
            Guid venueId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ParkingEdge> result = Edges
                .Where(edge => edge.VenueId == venueId)
                .ToList();

            return Task.FromResult(result);
        }
    }
}