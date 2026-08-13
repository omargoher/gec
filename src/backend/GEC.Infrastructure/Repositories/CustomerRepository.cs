using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;

    }

    public async Task<Customer?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId, cancellationToken);
    }
}