using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<Customer?> GetByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default);
}