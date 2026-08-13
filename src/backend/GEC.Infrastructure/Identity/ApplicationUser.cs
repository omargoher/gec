using GEC.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace GEC.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}