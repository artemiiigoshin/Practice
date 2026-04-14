using Practice.Controllers.DTO;
using Practice.Models;

namespace Tests
{
    public class EventValidatorTests
    {
        [Fact]
        public void CheckTime_Should_Return_False_And_ErrorMessage_When_EndAt_Is_Before_StartAt()
        {
            var dto = new EventCreateAndUpdateDto
            {
                Title = "Test Event",
                Description = "Test Description",
                StartAt = new DateTime(2025, 5, 10),
                EndAt = new DateTime(2025, 5, 9)
            };

            var result = EventValidator.CheckTime(dto, out string? errorMessage);

            Assert.False(result);
            Assert.Equal("EndAt должен быть позже StartAt", errorMessage);
        }

        [Fact]
        public void CheckTime_Should_Return_False_And_ErrorMessage_When_EndAt_Is_Equal_To_StartAt()
        {
            var dto = new EventCreateAndUpdateDto
            {
                Title = "Test Event",
                Description = "Test Description",
                StartAt = new DateTime(2025, 5, 10),
                EndAt = new DateTime(2025, 5, 10)
            };

            var result = EventValidator.CheckTime(dto, out string? errorMessage);

            Assert.False(result);
            Assert.Equal("EndAt должен быть позже StartAt", errorMessage);
        }

        [Fact]
        public void CheckTime_Should_Return_True_And_Null_ErrorMessage_When_Dates_Are_Valid()
        {
            var dto = new EventCreateAndUpdateDto
            {
                Title = "Test Event",
                Description = "Test Description",
                StartAt = new DateTime(2025, 5, 10),
                EndAt = new DateTime(2025, 5, 11)
            };

            var result = EventValidator.CheckTime(dto, out string? errorMessage);

            Assert.True(result);
            Assert.Null(errorMessage);
        }
    }
}
