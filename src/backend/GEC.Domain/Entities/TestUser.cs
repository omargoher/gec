using GEC.Domain.Enums;

namespace GEC.Domain.Entities;

public class TestUser : BaseEntity
{
    public string Name { get; set; } = null!;
    public Gender Gender { get; set; }
}