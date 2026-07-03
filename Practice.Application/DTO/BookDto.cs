namespace Practice.Application.DTO;

public record BookingReadDto(
    Guid Id,
    Guid EventId,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
    );
