namespace SharedModels;

public class Category
{
    public Guid CategoryId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is Category other)
        {
            return this.CategoryId == other.CategoryId;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return CategoryId.GetHashCode();
    }

    public override string? ToString()
    {
        return Name;
    }
}