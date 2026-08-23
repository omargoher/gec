namespace GEC.Domain.Entities;

public class Category : BaseEntity
{
    public Guid? ParentId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Stores Lucide icon name, e.g. "smartphone", "shirt", "gamepad-2"
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    // Products
}