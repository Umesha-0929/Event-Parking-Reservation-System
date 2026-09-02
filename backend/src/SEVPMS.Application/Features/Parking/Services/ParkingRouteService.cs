using SEVPMS.Application.Features.Parking.DTOs;
using SEVPMS.Application.Features.Parking.Interfaces;
using SEVPMS.Domain.Entities.Parking;

namespace SEVPMS.Application.Features.Parking.Services;

public sealed class ParkingRouteService(
    IParkingRouteRepository repository) : IParkingRouteService
{
    public async Task<ParkingRouteDto?> FindRouteAsync(
        Guid venueId,
        Guid startNodeId,
        Guid endNodeId,
        bool accessibleOnly,
        CancellationToken cancellationToken = default)
    {
        var nodes = await repository.GetNodesByVenueAsync(
            venueId,
            cancellationToken);

        var edges = await repository.GetEdgesByVenueAsync(
            venueId,
            cancellationToken);

        var nodesById = nodes.ToDictionary(node => node.Id);

        if (!nodesById.ContainsKey(startNodeId) ||
            !nodesById.ContainsKey(endNodeId))
        {
            return null;
        }

        if (startNodeId == endNodeId)
        {
            return new ParkingRouteDto
            {
                StartNodeId = startNodeId,
                EndNodeId = endNodeId,
                TotalCost = 0,
                Nodes =
                [
                    ToNodeDto(nodesById[startNodeId])
                ]
            };
        }

        var adjacency = nodes.ToDictionary(
            node => node.Id,
            _ => new List<RouteEdge>());

        foreach (var edge in edges)
        {
            if (edge.IsBlocked)
            {
                continue;
            }

            if (accessibleOnly && !edge.IsAccessible)
            {
                continue;
            }

            if (!nodesById.ContainsKey(edge.FromNodeId) ||
                !nodesById.ContainsKey(edge.ToNodeId))
            {
                continue;
            }

            adjacency[edge.FromNodeId].Add(
                new RouteEdge(
                    edge.ToNodeId,
                    edge.Cost));

            if (edge.IsBidirectional)
            {
                adjacency[edge.ToNodeId].Add(
                    new RouteEdge(
                        edge.FromNodeId,
                        edge.Cost));
            }
        }

        var distances = nodes.ToDictionary(
            node => node.Id,
            _ => decimal.MaxValue);

        var previous = new Dictionary<Guid, Guid>();

        var queue = new PriorityQueue<Guid, decimal>();

        distances[startNodeId] = 0;

        queue.Enqueue(
            startNodeId,
            0);

        while (queue.Count > 0)
        {
            var currentNodeId =
                queue.Dequeue();

            var currentDistance =
                distances[currentNodeId];

            if (currentNodeId == endNodeId)
            {
                break;
            }

            foreach (var routeEdge in adjacency[currentNodeId])
            {
                var newDistance =
                    currentDistance + routeEdge.Cost;

                if (newDistance >= distances[routeEdge.ToNodeId])
                {
                    continue;
                }

                distances[routeEdge.ToNodeId] =
                    newDistance;

                previous[routeEdge.ToNodeId] =
                    currentNodeId;

                queue.Enqueue(
                    routeEdge.ToNodeId,
                    newDistance);
            }
        }

        if (distances[endNodeId] == decimal.MaxValue)
        {
            return null;
        }

        var routeNodeIds = new List<Guid>
        {
            endNodeId
        };

        var currentId = endNodeId;

        while (currentId != startNodeId)
        {
            if (!previous.TryGetValue(
                    currentId,
                    out var previousId))
            {
                return null;
            }

            currentId = previousId;
            routeNodeIds.Add(currentId);
        }

        routeNodeIds.Reverse();

        return new ParkingRouteDto
        {
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            TotalCost = distances[endNodeId],
            Nodes = routeNodeIds
                .Select(id => ToNodeDto(nodesById[id]))
                .ToList()
        };
    }

    private static ParkingNodeDto ToNodeDto(
        ParkingNode node)
    {
        return new ParkingNodeDto
        {
            Id = node.Id,
            VenueId = node.VenueId,
            LayoutId = node.LayoutId,
            NodeCode = node.NodeCode,
            X = node.X,
            Y = node.Y,
            NodeType = node.NodeType
        };
    }

    private sealed record RouteEdge(
        Guid ToNodeId,
        decimal Cost);
}