namespace GEC.Domain.Entities;

public class Customer : BaseEntity
{
    public string IdentityUserId { get; set; } = default!;

    public string Name { get; set; }

    public string Email { get; set; }

    public ICollection<Address> Addresses { get; set; } = [];

    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;
}