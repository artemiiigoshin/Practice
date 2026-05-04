using Practice.Models;

namespace Practice.Controllers.DTO
{
    public record BookingReadDto(
        Guid Id,
        Guid EventId,
        string Status,
        DateTime CreatedAt,
        DateTime? ProcessedAt
        );
}
