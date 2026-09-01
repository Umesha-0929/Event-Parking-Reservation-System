using SEVPMS.Application.Features.Seats.DTOs;
using SEVPMS.Application.Features.Seats.Interfaces;
using SEVPMS.Domain.Entities.Seats;
using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Seats.Services;

public sealed class SeatingLayoutService(
    ISeatingLayoutRepository repository,
    ISeatService seatService,
    TimeProvider timeProvider) : ISeatingLayoutService
{
    public async Task<SeatingLayoutDto?> GetOrganizerLayoutAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.", nameof(eventId));

        var layout = await repository.GetLayoutByEventAsync(
            eventId,
            cancellationToken);

        if (layout is null)
            return null;

        return await MapLayoutAsync(layout, cancellationToken);
    }

    public async Task<SeatingLayoutDto> ConfigureLayoutAsync(
        Guid eventId,
        Guid organizerUserId,
        ConfigureSeatingLayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOrganizerRequest(eventId, organizerUserId);
        ValidateLayoutRequest(request);

        var layout = await repository.GetLayoutByEventAsync(
            eventId,
            cancellationToken);

        if (layout is null)
        {
            layout = new SeatingLayout
            {
                EventId = eventId,
                StageType = request.StageType,
                RowCount = request.RowCount,
                ColumnCount = request.ColumnCount,
                CanvasWidth = request.CanvasWidth,
                CanvasHeight = request.CanvasHeight,
                StageX = request.StageX,
                StageY = request.StageY,
                StageWidth = request.StageWidth,
                StageHeight = request.StageHeight,
                IsPublished = false
            };

            layout = await repository.AddLayoutAsync(
                layout,
                cancellationToken);
        }
        else
        {
            EnsureEditable(layout);

            layout.StageType = request.StageType;
            layout.RowCount = request.RowCount;
            layout.ColumnCount = request.ColumnCount;
            layout.CanvasWidth = request.CanvasWidth;
            layout.CanvasHeight = request.CanvasHeight;
            layout.StageX = request.StageX;
            layout.StageY = request.StageY;
            layout.StageWidth = request.StageWidth;
            layout.StageHeight = request.StageHeight;

            await repository.UpdateLayoutAsync(
                layout,
                cancellationToken);
        }

        return await MapLayoutAsync(layout, cancellationToken);
    }

    public async Task<SeatSectionDto> UpsertSectionAsync(
        Guid eventId,
        Guid organizerUserId,
        UpsertSeatSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOrganizerRequest(eventId, organizerUserId);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Section name is required.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Section code is required.");

        if (request.RowCount <= 0 || request.ColumnCount <= 0)
            throw new ArgumentException(
                "Section rows and columns must be greater than zero.");

        if (request.Width <= 0 || request.Height <= 0)
            throw new ArgumentException(
                "Section width and height must be greater than zero.");

        var layout = await RequireLayoutAsync(
            eventId,
            cancellationToken);

        EnsureEditable(layout);

        if (request.Id.HasValue)
        {
            var sections = await repository.GetSectionsAsync(
                layout.Id,
                cancellationToken);

            if (!sections.Any(x => x.Id == request.Id.Value))
            {
                throw new InvalidOperationException(
                    "The requested seat section does not belong to this layout.");
            }
        }

        var section = new SeatSection
        {
            Id = request.Id ?? Guid.NewGuid(),
            EventId = eventId,
            SeatingLayoutId = layout.Id,
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            RowCount = request.RowCount,
            ColumnCount = request.ColumnCount,
            X = request.X,
            Y = request.Y,
            Width = request.Width,
            Height = request.Height,
            DisplayOrder = request.DisplayOrder,
            IsAccessibleSection = request.IsAccessibleSection,
            IsEnabled = request.IsEnabled
        };

        var saved = await repository.UpsertSectionAsync(
            section,
            cancellationToken);

        return MapSection(saved);
    }

    public async Task<SeatCategoryDto> UpsertCategoryAsync(
        Guid eventId,
        Guid organizerUserId,
        UpsertSeatCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOrganizerRequest(eventId, organizerUserId);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Seat category name is required.");

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Seat category code is required.");

        if (request.Price < 0)
            throw new ArgumentException(
                "Seat category price cannot be negative.");

        var layout = await RequireLayoutAsync(
            eventId,
            cancellationToken);

        EnsureEditable(layout);

        if (request.Id.HasValue)
        {
            var categories = await repository.GetCategoriesAsync(
                layout.Id,
                cancellationToken);

            if (!categories.Any(x => x.Id == request.Id.Value))
            {
                throw new InvalidOperationException(
                    "The requested category does not belong to this layout.");
            }
        }

        var category = new SeatCategory
        {
            Id = request.Id ?? Guid.NewGuid(),
            EventId = eventId,
            SeatingLayoutId = layout.Id,
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            Price = request.Price,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        var saved = await repository.UpsertCategoryAsync(
            category,
            cancellationToken);

        return MapCategory(saved);
    }

    public async Task<IReadOnlyCollection<SeatAvailabilityDto>> GenerateSeatsAsync(
        Guid eventId,
        Guid organizerUserId,
        GenerateSeatsRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOrganizerRequest(eventId, organizerUserId);

        if (request.SectionId == Guid.Empty)
            throw new ArgumentException("Section id is required.");

        if (request.RowCount <= 0 || request.ColumnCount <= 0)
            throw new ArgumentException(
                "Seat generation rows and columns must be greater than zero.");

        if (request.StartingSeatNumber <= 0)
            throw new ArgumentException(
                "Starting seat number must be greater than zero.");

        if (request.HorizontalSpacing <= 0 ||
            request.VerticalSpacing <= 0)
        {
            throw new ArgumentException(
                "Seat spacing must be greater than zero.");
        }

        var layout = await RequireLayoutAsync(
            eventId,
            cancellationToken);

        EnsureEditable(layout);

        var sections = await repository.GetSectionsAsync(
            layout.Id,
            cancellationToken);

        var section = sections.SingleOrDefault(
            x => x.Id == request.SectionId);

        if (section is null || !section.IsEnabled)
            throw new InvalidOperationException(
                "The requested seat section is unavailable.");

        if (request.RowCount > section.RowCount ||
            request.ColumnCount > section.ColumnCount)
        {
            throw new ArgumentException(
                "Generated rows or columns exceed the section configuration.");
        }

        if (request.SeatCategoryId.HasValue)
        {
            var categories = await repository.GetCategoriesAsync(
                layout.Id,
                cancellationToken);

            if (!categories.Any(x =>
                    x.Id == request.SeatCategoryId.Value &&
                    x.IsActive))
            {
                throw new InvalidOperationException(
                    "The requested seat category is unavailable.");
            }
        }

        var firstRowNumber = ParseRowLabel(request.StartingRowLabel);

        var unavailable = (request.UnavailablePositions ??
                           Array.Empty<SeatPositionRequest>())
            .Select(x => (x.RowNumber, x.ColumnNumber))
            .ToHashSet();

        var accessible = (request.AccessiblePositions ??
                          Array.Empty<SeatPositionRequest>())
            .Select(x => (x.RowNumber, x.ColumnNumber))
            .ToHashSet();

        var gaps = request.Gaps ??
                   Array.Empty<LayoutGapRequest>();

        var seats = new List<Seat>();

        for (var row = 1; row <= request.RowCount; row++)
        {
            var rowLabel = FormatRowLabel(
                firstRowNumber + row - 1);

            for (var column = 1;
                 column <= request.ColumnCount;
                 column++)
            {
                var isGap = gaps.Any(gap =>
                    gap.RowNumber == row &&
                    column >= gap.StartColumn &&
                    column < gap.StartColumn + gap.ColumnSpan);

                if (isGap)
                    continue;

                var position = (row, column);

                var isUnavailable =
                    unavailable.Contains(position);

                var isAccessible =
                    section.IsAccessibleSection ||
                    accessible.Contains(position);

                seats.Add(new Seat
                {
                    EventId = eventId,
                    SeatingLayoutId = layout.Id,
                    SectionId = section.Id,
                    SeatCategoryId = request.SeatCategoryId,
                    RowLabel = rowLabel,
                    RowNumber = row,
                    ColumnNumber = column,
                    SeatNumber =
                        (request.StartingSeatNumber + column - 1)
                        .ToString(),
                    X = request.StartX +
                        ((column - 1) * request.HorizontalSpacing),
                    Y = request.StartY +
                        ((row - 1) * request.VerticalSpacing),
                    IsAccessible = isAccessible,
                    Status = isUnavailable
                        ? SeatStatus.Blocked
                        : SeatStatus.Available
                });
            }
        }

        if (seats.Count == 0)
            throw new InvalidOperationException(
                "Seat generation produced no seats.");

        await repository.ReplaceSectionSeatsAsync(
            eventId,
            section.Id,
            seats,
            cancellationToken);

        return seats
            .Select(MapGeneratedSeat)
            .ToArray();
    }

    public async Task<SeatingLayoutDto> PublishLayoutAsync(
        Guid eventId,
        Guid organizerUserId,
        PublishSeatingLayoutRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureOrganizerRequest(eventId, organizerUserId);

        var layout = await RequireLayoutAsync(
            eventId,
            cancellationToken);

        if (request.Publish)
        {
            var sections = await repository.GetSectionsAsync(
                layout.Id,
                cancellationToken);

            if (!sections.Any(x => x.IsEnabled))
                throw new InvalidOperationException(
                    "At least one enabled seat section is required before publishing.");

            var seats = await repository.GetSeatsAsync(
                eventId,
                cancellationToken);

            if (seats.Count == 0)
                throw new InvalidOperationException(
                    "Generate seats before publishing the seating layout.");

            layout.IsPublished = true;
            layout.PublishedAtUtc =
                timeProvider.GetUtcNow().UtcDateTime;
        }
        else
        {
            layout.IsPublished = false;
            layout.PublishedAtUtc = null;
        }

        await repository.UpdateLayoutAsync(
            layout,
            cancellationToken);

        return await MapLayoutAsync(
            layout,
            cancellationToken);
    }

    public async Task<PublishedSeatingLayoutDto?> GetPublishedLayoutAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException(
                "Event id is required.",
                nameof(eventId));

        var layout =
            await repository.GetPublishedLayoutByEventAsync(
                eventId,
                cancellationToken);

        if (layout is null)
            return null;

        var sections =
            await repository.GetSectionsAsync(
                layout.Id,
                cancellationToken);

        var categories =
            await repository.GetCategoriesAsync(
                layout.Id,
                cancellationToken);

        var persistedSeats =
            await repository.GetSeatsAsync(
                eventId,
                cancellationToken);

        var availability =
            await seatService.GetAvailabilityAsync(
                eventId,
                null,
                cancellationToken);

        var availabilityBySeat =
            availability.ToDictionary(
                x => x.SeatId);

        var categoryById =
            categories.ToDictionary(
                x => x.Id);

        var publishedSeats =
            persistedSeats
                .Select(seat =>
                {
                    availabilityBySeat.TryGetValue(
                        seat.Id,
                        out var liveState);

                    SeatCategory? category = null;

                    if (seat.SeatCategoryId.HasValue)
                    {
                        categoryById.TryGetValue(
                            seat.SeatCategoryId.Value,
                            out category);
                    }

                    var state =
                        liveState?.State ??
                        seat.Status switch
                        {
                            SeatStatus.Booked => "Booked",
                            SeatStatus.Blocked => "Unavailable",
                            _ => "Available"
                        };

                    return new PublishedSeatDto(
                        seat.Id,
                        seat.EventId,
                        seat.SectionId,
                        seat.SeatCategoryId,
                        category?.Name,
                        category?.Code,
                        category?.Price,
                        seat.RowLabel,
                        seat.RowNumber,
                        seat.ColumnNumber,
                        seat.SeatNumber,
                        seat.X,
                        seat.Y,
                        seat.IsAccessible,
                        state,
                        liveState?.HeldUntilUtc);
                })
                .ToArray();

        return new PublishedSeatingLayoutDto(
            layout.Id,
            layout.EventId,
            layout.StageType,
            layout.CanvasWidth,
            layout.CanvasHeight,
            layout.StageX,
            layout.StageY,
            layout.StageWidth,
            layout.StageHeight,
            sections
                .Where(x => x.IsEnabled)
                .Select(MapSection)
                .ToArray(),
            categories
                .Where(x => x.IsActive)
                .Select(MapCategory)
                .ToArray(),
            publishedSeats);
    }
    private async Task<SeatingLayout> RequireLayoutAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return await repository.GetLayoutByEventAsync(
                   eventId,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "Configure the seating layout first.");
    }

    private async Task<SeatingLayoutDto> MapLayoutAsync(
        SeatingLayout layout,
        CancellationToken cancellationToken)
    {
        var sections = await repository.GetSectionsAsync(
            layout.Id,
            cancellationToken);

        var categories = await repository.GetCategoriesAsync(
            layout.Id,
            cancellationToken);

        return new SeatingLayoutDto(
            layout.Id,
            layout.EventId,
            layout.StageType,
            layout.RowCount,
            layout.ColumnCount,
            layout.CanvasWidth,
            layout.CanvasHeight,
            layout.StageX,
            layout.StageY,
            layout.StageWidth,
            layout.StageHeight,
            layout.IsPublished,
            layout.PublishedAtUtc,
            sections.Select(MapSection).ToArray(),
            categories.Select(MapCategory).ToArray());
    }

    private static SeatSectionDto MapSection(
        SeatSection section)
    {
        return new SeatSectionDto(
            section.Id,
            section.SeatingLayoutId,
            section.Name,
            section.Code,
            section.RowCount,
            section.ColumnCount,
            section.X,
            section.Y,
            section.Width,
            section.Height,
            section.DisplayOrder,
            section.IsAccessibleSection,
            section.IsEnabled);
    }

    private static SeatCategoryDto MapCategory(
        SeatCategory category)
    {
        return new SeatCategoryDto(
            category.Id,
            category.Name,
            category.Code,
            category.Price,
            category.DisplayOrder,
            category.IsActive);
    }

    private static SeatAvailabilityDto MapGeneratedSeat(
        Seat seat)
    {
        var state = seat.Status switch
        {
            SeatStatus.Booked => "Booked",
            SeatStatus.Blocked => "Unavailable",
            _ => "Available"
        };

        return new SeatAvailabilityDto(
            seat.Id,
            seat.EventId,
            seat.SectionId,
            seat.RowLabel,
            seat.SeatNumber,
            seat.X,
            seat.Y,
            seat.TicketTypeId,
            seat.IsAccessible,
            state,
            null);
    }

    private static void EnsureOrganizerRequest(
        Guid eventId,
        Guid organizerUserId)
    {
        if (eventId == Guid.Empty)
            throw new ArgumentException("Event id is required.");

        if (organizerUserId == Guid.Empty)
            throw new ArgumentException(
                "Organizer user id is required.");
    }

    private static void EnsureEditable(
        SeatingLayout layout)
    {
        if (layout.IsPublished)
        {
            throw new InvalidOperationException(
                "Unpublish the seating layout before modifying it.");
        }
    }

    private static void ValidateLayoutRequest(
        ConfigureSeatingLayoutRequest request)
    {
        if (!Enum.IsDefined(
                typeof(StageType),
                request.StageType))
        {
            throw new ArgumentException(
                "Unsupported stage type.");
        }

        if (request.RowCount <= 0 ||
            request.ColumnCount <= 0)
        {
            throw new ArgumentException(
                "Layout rows and columns must be greater than zero.");
        }

        if (request.CanvasWidth <= 0 ||
            request.CanvasHeight <= 0)
        {
            throw new ArgumentException(
                "Canvas dimensions must be greater than zero.");
        }

        if (request.StageWidth <= 0 ||
            request.StageHeight <= 0)
        {
            throw new ArgumentException(
                "Stage dimensions must be greater than zero.");
        }
    }

    private static int ParseRowLabel(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(
                "Starting row label is required.");

        var label = value.Trim().ToUpperInvariant();

        var number = 0;

        foreach (var character in label)
        {
            if (character < 'A' || character > 'Z')
                throw new ArgumentException(
                    "Row labels must contain letters only.");

            number = checked(
                (number * 26) +
                (character - 'A' + 1));
        }

        return number;
    }

    private static string FormatRowLabel(
        int number)
    {
        if (number <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(number));

        var characters = new Stack<char>();

        while (number > 0)
        {
            number--;

            characters.Push(
                (char)('A' + (number % 26)));

            number /= 26;
        }

        return new string(characters.ToArray());
    }
}

