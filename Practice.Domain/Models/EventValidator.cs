namespace Practice.Domain.Models;

public static class EventValidator
{
    public static bool CheckTime(DateTime startAt, DateTime endAt, out string? errorMessage)
    {
        if (endAt <= startAt)
        {
            errorMessage = "EndAt должен быть позже StartAt";
            return false;
        }
        errorMessage = null;
        return true;
    }
}
