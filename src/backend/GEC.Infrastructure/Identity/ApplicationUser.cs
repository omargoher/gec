using GEC.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GEC.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}