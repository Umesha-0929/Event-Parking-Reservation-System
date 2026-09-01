using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Application.Features.Seats.Services;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;
using Xunit;

namespace SEVPMS.UnitTests.Seats;

public sealed class SeatingLayoutServiceTests
{
    [Fact]
    public async Task ConfigureLayout_SavesSelectedStageAndDimensions()
    {
        var repository = new FakeSeatingLayoutRepository();
        var service = CreateService(repository);

        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        var request = new ConfigureSeatingLayoutRequest(
            StageType.ThrustStage,
            12,
            20,
            1200,
            800,
            300,
            40,
            600,
            100);

        var result = await service.ConfigureLayoutAsync(
            eventId,
            organizerId,
            request);

        Assert.Equal(eventId, result.EventId);
        Assert.Equal(StageType.ThrustStage, result.StageType);
        Assert.Equal(12, result.RowCount);
        Assert.Equal(20, result.ColumnCount);
        Assert.False(result.IsPublished);
    }

    [Fact]
    public async Task GenerateSeats_RespectsGapsAccessibilityAndUnavailablePositions()
    {
        var repository = new FakeSeatingLayoutRepository();

        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        var layout = new SeatingLayout
        {
            EventId = eventId,
            StageType = StageType.ArenaStage,
            RowCount = 2,
            ColumnCount = 4,
            CanvasWidth = 1200,
            CanvasHeight = 800,
            StageWidth = 500,
            StageHeight = 100
        };

        await repository.AddLayoutAsync(layout);

        var section = new SeatSection
        {
            EventId = eventId,
            SeatingLayoutId = layout.Id,
            Name = "Premium",
            Code = "PREM",
            RowCount = 2,
            ColumnCount = 4,
            Width = 600,
            Height = 300,
            IsEnabled = true
        };

        await repository.UpsertSectionAsync(section);

        var category = new SeatCategory
        {
            EventId = eventId,
            SeatingLayoutId = layout.Id,
            Name = "Premium",
            Code = "PREM",
            Price = 7500,
            IsActive = true
        };

        await repository.UpsertCategoryAsync(category);

        var service = CreateService(repository);

        var request = new GenerateSeatsRequest(
            section.Id,
            category.Id,
            2,
            4,
            "A",
            1,
            100,
            150,
            50,
            50,
            new[]
            {
                new SeatPositionRequest(2, 4)
            },
            new[]
            {
                new SeatPositionRequest(1, 3)
            },
            new[]
            {
                new LayoutGapRequest(1, 2, 1)
            });

        var result = await service.GenerateSeatsAsync(
            eventId,
            organizerId,
            request);

        Assert.Equal(7, result.Count);

        Assert.DoesNotContain(
            repository.Seats,
            x => x.RowNumber == 1 &&
                 x.ColumnNumber == 2);

        var accessibleSeat = Assert.Single(
            repository.Seats.Where(
                x => x.RowNumber == 1 &&
                     x.ColumnNumber == 3));

        Assert.True(accessibleSeat.IsAccessible);

        var unavailableSeat = Assert.Single(
            repository.Seats.Where(
                x => x.RowNumber == 2 &&
                     x.ColumnNumber == 4));

        Assert.Equal(
            SeatStatus.Blocked,
            unavailableSeat.Status);

        Assert.All(
            repository.Seats,
            x => Assert.Equal(
                category.Id,
                x.SeatCategoryId));
    }

    [Fact]
    public async Task PublishLayout_MakesConfiguredLayoutVisibleAsPublished()
    {
        var repository = new FakeSeatingLayoutRepository();

        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        var layout = new SeatingLayout
        {
            EventId = eventId,
            StageType = StageType.ProsceniumTheatreStage,
            RowCount = 5,
            ColumnCount = 10,
            CanvasWidth = 1200,
            CanvasHeight = 800,
            StageWidth = 500,
            StageHeight = 100
        };

        await repository.AddLayoutAsync(layout);

        var section = new SeatSection
        {
            EventId = eventId,
            SeatingLayoutId = layout.Id,
            Name = "Standard",
            Code = "STD",
            RowCount = 5,
            ColumnCount = 10,
            Width = 700,
            Height = 400,
            IsEnabled = true
        };

        await repository.UpsertSectionAsync(section);

        await repository.ReplaceSectionSeatsAsync(
            eventId,
            section.Id,
            new[]
            {
                new Seat
                {
                    EventId = eventId,
                    SeatingLayoutId = layout.Id,
                    SectionId = section.Id,
                    RowLabel = "A",
                    RowNumber = 1,
                    ColumnNumber = 1,
                    SeatNumber = "1",
                    Status = SeatStatus.Available
                }
            });

        var service = CreateService(repository);

        var result = await service.PublishLayoutAsync(
            eventId,
            organizerId,
            new PublishSeatingLayoutRequest(true));

        Assert.True(result.IsPublished);
        Assert.NotNull(result.PublishedAtUtc);

        var published =
            await repository.GetPublishedLayoutByEventAsync(eventId);

        Assert.NotNull(published);
        Assert.True(published.IsPublished);
    }

    [Fact]
    public async Task GetPublishedLayout_ReturnsNullWhenLayoutIsNotPublished()
    {
        var repository = new FakeSeatingLayoutRepository();

        var eventId = Guid.NewGuid();

        await repository.AddLayoutAsync(
            new SeatingLayout
            {
                EventId = eventId,
                StageType = StageType.EndOnStage,
                RowCount = 6,
                ColumnCount = 10,
                CanvasWidth = 1200,
                CanvasHeight = 800,
                StageWidth = 500,
                StageHeight = 100,
                IsPublished = false
            });

        var service = CreateService(repository);

        var result = await service.GetPublishedLayoutAsync(eventId);

        Assert.Null(result);
    }

    [Fact]
    public async Task PublishedLayout_CannotBeModifiedUntilUnpublished()
    {
        var repository = new FakeSeatingLayoutRepository();

        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();

        await repository.AddLayoutAsync(
            new SeatingLayout
            {
                EventId = eventId,
                StageType = StageType.InTheRoundStage,
                RowCount = 8,
                ColumnCount = 12,
                CanvasWidth = 1200,
                CanvasHeight = 800,
                StageWidth = 400,
                StageHeight = 400,
                IsPublished = true,
                PublishedAtUtc = DateTime.UtcNow
            });

        var service = CreateService(repository);

        var request = new ConfigureSeatingLayoutRequest(
            StageType.ArenaStage,
            10,
            14,
            1200,
            800,
            300,
            100,
            500,
            150);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.ConfigureLayoutAsync(
                    eventId,
                    organizerId,
                    request));

        Assert.Contains(
            "Unpublish",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }
    private static SeatingLayoutService CreateService(
        FakeSeatingLayoutRepository repository)
    {
        return new SeatingLayoutService(
            repository,
            new FakeSeatService(),
            TimeProvider.System);
    }

    private sealed class FakeSeatingLayoutRepository
        : ISeatingLayoutRepository
    {
        public SeatingLayout? Layout { get; private set; }

        public List<SeatSection> Sections { get; } = new();

        public List<SeatCategory> Categories { get; } = new();

        public List<Seat> Seats { get; } = new();

        public Task<SeatingLayout?> GetLayoutByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Layout?.EventId == eventId
                    ? Layout
                    : null);
        }

        public Task<SeatingLayout?> GetPublishedLayoutByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Layout?.EventId == eventId &&
                Layout.IsPublished
                    ? Layout
                    : null);
        }

        public Task<IReadOnlyCollection<SeatSection>> GetSectionsAsync(
            Guid seatingLayoutId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<SeatSection> result =
                Sections
                    .Where(x => x.SeatingLayoutId == seatingLayoutId)
                    .ToArray();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<SeatCategory>> GetCategoriesAsync(
            Guid seatingLayoutId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<SeatCategory> result =
                Categories
                    .Where(x => x.SeatingLayoutId == seatingLayoutId)
                    .ToArray();

            return Task.FromResult(result);
        }

        public Task<IReadOnlyCollection<Seat>> GetSeatsAsync(
            Guid eventId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<Seat> result =
                Seats
                    .Where(x => x.EventId == eventId)
                    .ToArray();

            return Task.FromResult(result);
        }

        public Task<SeatingLayout> AddLayoutAsync(
            SeatingLayout layout,
            CancellationToken cancellationToken = default)
        {
            Layout = layout;

            return Task.FromResult(layout);
        }

        public Task UpdateLayoutAsync(
            SeatingLayout layout,
            CancellationToken cancellationToken = default)
        {
            Layout = layout;

            return Task.CompletedTask;
        }

        public Task<SeatSection> UpsertSectionAsync(
            SeatSection section,
            CancellationToken cancellationToken = default)
        {
            Sections.RemoveAll(x => x.Id == section.Id);
            Sections.Add(section);

            return Task.FromResult(section);
        }

        public Task<SeatCategory> UpsertCategoryAsync(
            SeatCategory category,
            CancellationToken cancellationToken = default)
        {
            Categories.RemoveAll(x => x.Id == category.Id);
            Categories.Add(category);

            return Task.FromResult(category);
        }

        public Task ReplaceSectionSeatsAsync(
            Guid eventId,
            Guid sectionId,
            IReadOnlyCollection<Seat> seats,
            CancellationToken cancellationToken = default)
        {
            Seats.RemoveAll(
                x => x.EventId == eventId &&
                     x.SectionId == sectionId);

            Seats.AddRange(seats);

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSeatService : ISeatService
    {
        public Task<IReadOnlyList<SeatAvailabilityDto>> GetAvailabilityAsync(
            Guid eventId,
            Guid? sectionId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<SeatAvailabilityDto> result =
                Array.Empty<SeatAvailabilityDto>();

            return Task.FromResult(result);
        }

        public Task<SeatHoldResponse> HoldAsync(
            Guid eventId,
            Guid userId,
            CreateSeatHoldRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ReleaseHoldAsync(
            string holdToken,
            Guid userId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> CommitHoldAsync(
            string holdToken,
            Guid userId,
            Guid bookingId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatAvailabilityDto> UpsertSeatAsync(
            Guid eventId,
            UpsertSeatRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatViewAssetDto?> GetSeatViewAsync(
            Guid eventId,
            Guid seatId,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<SeatViewAssetDto> UpsertSeatViewAsync(
            Guid eventId,
            UpsertSeatViewAssetRequest request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}

