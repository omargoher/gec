using GEC.ApplicationCore.Interfaces.Identity;
using GEC.Infrastructure.Identity;

namespace GEC.Infrastructure.UnitOfWork;

public class IdentityUnitOfWork : IIdentityUnitOfWork
{
    public readonly AppIdentityDbContext _context;

    public IdentityUnitOfWork(AppIdentityDbContext context)
    {
        _context = context;
    }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}