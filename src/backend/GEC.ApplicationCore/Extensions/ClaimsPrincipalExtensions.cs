using System;
using System.Security.Claims;

namespace GEC.ApplicationCore.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetCustomerIdOrNull(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        if (idClaim != null && Guid.TryParse(idClaim.Value, out var customerId))
        {
            return customerId;
        }
        return null;
    }
}
