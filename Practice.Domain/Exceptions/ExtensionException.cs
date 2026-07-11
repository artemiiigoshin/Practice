namespace Practice.Domain.Exceptions;

public class ExtensionException : Exception
{
    public ExtensionException()
        : base("No available seats for this event")
    {
    }
}

public class PastEventBookingException : Exception
{
    public PastEventBookingException()
        : base("Cannot book an event that has already started or finished.")
    {
    }
}

public class ActiveBookingLimitExceededException : Exception
{
    public ActiveBookingLimitExceededException()
        : base("Active booking limit exceeded.")
    {
    }
}

public class OperationForbiddenException : Exception
{
    public OperationForbiddenException()
        : base("User does not have permission to perform this operation.")
    {
    }
}
