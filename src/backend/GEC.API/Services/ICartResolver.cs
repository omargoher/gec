namespace GEC.API.Services;

public interface ICartResolver
{
    Task<Guid> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
}