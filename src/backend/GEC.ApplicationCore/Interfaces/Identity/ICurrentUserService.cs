namespace GEC.ApplicationCore.Interfaces.Identity;

public interface ICurrentUserService
{
    string? UserId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    Task<Guid> GetCustomerIdAsync(CancellationToken cancellationToken = default);
    Task<Guid> GetCustomerIdByEmailAsync(string email, CancellationToken cancellationToken = default);
}