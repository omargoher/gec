using System;
using System.Threading;
using System.Threading.Tasks;
using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface ICartRepository : IBaseRepository<Cart>
{
    Task<Cart?> GetByCustomerIdWithItemsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<Cart?> GetByIdWithItemsAsync(Guid cartId, CancellationToken cancellationToken = default);
}
