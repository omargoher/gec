using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class AdminRepository : BaseRepository<Admin>, IAdminRepository
{
    public readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;

    }

    public async Task<Admin?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId, cancellationToken);
    }
}