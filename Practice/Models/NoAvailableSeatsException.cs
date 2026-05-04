namespace Practice.Models
{
    public class NoAvailableSeatsException : Exception
    {
        public NoAvailableSeatsException()
            : base("No available seats for this event")
        {
        }
    }
}
