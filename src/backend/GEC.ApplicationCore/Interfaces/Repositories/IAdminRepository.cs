using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IAdminRepository : IBaseRepository<Admin>
{
    Task<Admin?> GetByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);
}