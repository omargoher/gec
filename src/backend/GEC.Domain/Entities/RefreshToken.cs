namespace GEC.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public Guid FamilyId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsRevoked => RevokedAtUtc.HasValue;

    public bool IsActive => !IsExpired && !IsRevoked;
}