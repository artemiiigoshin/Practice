using System.ComponentModel.DataAnnotations;

namespace Practice.Controllers.DTO
{
    public record EventCreateAndUpdateDto(
        [Required(ErrorMessage = "Title обязателен")]
        string Title,
        string? Description,
        [Required(ErrorMessage = "StartAt обязателен")]
        DateTime StartAt,
        [Required(ErrorMessage = "EndAt обязателен")]
        DateTime EndAt
    );

    public record EventReadDto(
        Guid Id,
        string Title,
        string? Description,
        DateTime StartAt,
        DateTime EndAt
    );
}
