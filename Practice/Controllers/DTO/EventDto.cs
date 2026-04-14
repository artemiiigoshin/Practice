using System.ComponentModel.DataAnnotations;

namespace Practice.Controllers.DTO
{
    public record EventCreateAndUpdateDto
    {
        [Required(ErrorMessage = "Title обязателен")]
        public required string Title { get; init; }
        public string? Description { get; init; }
        [Required(ErrorMessage = "StartAt обязателен")]
        public DateTime StartAt { get; init; }
        [Required(ErrorMessage = "EndAt обязателен")]
        public DateTime EndAt { get; init; }
    }

    public record EventReadDto(
        Guid Id,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt
    );
}
