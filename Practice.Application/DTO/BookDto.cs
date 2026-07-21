namespace Practice.Application.DTO;

public record BookingReadDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
    );
