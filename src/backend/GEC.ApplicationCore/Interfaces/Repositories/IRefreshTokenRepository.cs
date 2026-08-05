using GEC.Domain.Entities;

namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    void Update(
        RefreshToken refreshToken);

    Task RevokeAllForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken = default);
}