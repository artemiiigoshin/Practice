namespace Practice.Controllers.DTO
{
    public class PaginatedResult<T>
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int ItemsCount { get; set; }
        public List<T> Items { get; set; } = new();
    }

    public class EventQueryParameters
    {
        public string? Title { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
