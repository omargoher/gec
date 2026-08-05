using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppIdentityDbContext _context;

    public RefreshTokenRepository(AppIdentityDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return await _context.RefreshTokens
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash,
                cancellationToken);
    }

    public void Update(RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);

        _context.RefreshTokens.Update(refreshToken);
    }

    public async Task RevokeAllForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        var now = DateTime.UtcNow;

        var activeTokens = await _context.RefreshTokens
            .Where(t =>
                t.UserId == userId &&
                t.RevokedAtUtc == null &&
                t.ExpiresAtUtc > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = now;
        }
    }
    public async Task RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var tokens = await _context.RefreshTokens
            .Where(t =>
                t.FamilyId == familyId &&
                t.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAtUtc = now;
        }
    }
}