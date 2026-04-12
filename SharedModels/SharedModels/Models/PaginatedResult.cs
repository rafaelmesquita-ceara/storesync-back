namespace SharedModels;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}
