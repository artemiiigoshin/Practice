using Practice.Controllers.DTO;

namespace Practice.Models
{
    public static class EventValidator
    {
        public static bool CheckTime(EventCreateAndUpdateDto info, out string? errorMessage)
        {
            if (info.EndAt <= info.StartAt)
            {
                errorMessage = "EndAt должен быть позже StartAt";
                return false;
            }
            errorMessage = null;
            return true;
        }
    }
}
