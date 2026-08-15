namespace GEC.Domain.Entities;

public class Admin : BaseEntity
{
    public string IdentityUserId { get; set; } = default!;

    public string Name { get; set; }

    public string Email { get; set; }
}