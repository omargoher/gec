using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IAddressRepository : IBaseRepository<Address>
{
    Task<IReadOnlyList<Address>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Address?> GetByIdAndCustomerIdAsync(
        Guid addressId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Address?> GetDefaultAddressAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task SetDefaultAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForCustomerAsync(
        Guid addressId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<bool> HasAnyAddressAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}