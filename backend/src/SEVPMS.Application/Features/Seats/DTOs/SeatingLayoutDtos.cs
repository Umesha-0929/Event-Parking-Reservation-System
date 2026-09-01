using SEVPMS.Domain.Enums;

namespace SEVPMS.Application.Features.Seats.DTOs;

public sealed record SeatingLayoutDto(
    Guid Id,
    Guid EventId,
    StageType StageType,
    int RowCount,
    int ColumnCount,
    decimal CanvasWidth,
    decimal CanvasHeight,
    decimal StageX,
    decimal StageY,
    decimal StageWidth,
    decimal StageHeight,
    bool IsPublished,
    DateTime? PublishedAtUtc,
    IReadOnlyCollection<SeatSectionDto> Sections,
    IReadOnlyCollection<SeatCategoryDto> Categories);

public sealed record ConfigureSeatingLayoutRequest(
    StageType StageType,
    int RowCount,
    int ColumnCount,
    decimal CanvasWidth,
    decimal CanvasHeight,
    decimal StageX,
    decimal StageY,
    decimal StageWidth,
    decimal StageHeight);

public sealed record SeatSectionDto(
    Guid Id,
    Guid SeatingLayoutId,
    string Name,
    string Code,
    int RowCount,
    int ColumnCount,
    decimal X,
    decimal Y,
    decimal Width,
    decimal Height,
    int DisplayOrder,
    bool IsAccessibleSection,
    bool IsEnabled);

public sealed record UpsertSeatSectionRequest(
    Guid? Id,
    string Name,
    string Code,
    int RowCount,
    int ColumnCount,
    decimal X,
    decimal Y,
    decimal Width,
    decimal Height,
    int DisplayOrder,
    bool IsAccessibleSection,
    bool IsEnabled);

public sealed record SeatCategoryDto(
    Guid Id,
    string Name,
    string Code,
    decimal Price,
    int DisplayOrder,
    bool IsActive);

public sealed record UpsertSeatCategoryRequest(
    Guid? Id,
    string Name,
    string Code,
    decimal Price,
    int DisplayOrder,
    bool IsActive);

public sealed record GenerateSeatsRequest(
    Guid SectionId,
    Guid? SeatCategoryId,
    int RowCount,
    int ColumnCount,
    string StartingRowLabel,
    int StartingSeatNumber,
    decimal StartX,
    decimal StartY,
    decimal HorizontalSpacing,
    decimal VerticalSpacing,
    IReadOnlyCollection<SeatPositionRequest> UnavailablePositions,
    IReadOnlyCollection<SeatPositionRequest> AccessiblePositions,
    IReadOnlyCollection<LayoutGapRequest> Gaps);

public sealed record SeatPositionRequest(
    int RowNumber,
    int ColumnNumber);

public sealed record LayoutGapRequest(
    int RowNumber,
    int StartColumn,
    int ColumnSpan);

public sealed record PublishSeatingLayoutRequest(
    bool Publish);

public sealed record PublishedSeatingLayoutDto(
    Guid LayoutId,
    Guid EventId,
    StageType StageType,
    decimal CanvasWidth,
    decimal CanvasHeight,
    decimal StageX,
    decimal StageY,
    decimal StageWidth,
    decimal StageHeight,
    IReadOnlyCollection<SeatSectionDto> Sections,
    IReadOnlyCollection<SeatCategoryDto> Categories,
    IReadOnlyCollection<SeatAvailabilityDto> Seats);
