using GEC.ApplicationCore.DTOs.Profile;

namespace GEC.ApplicationCore.Interfaces.Services;

public interface IProfileService
{
    Task<ProfileDto> GetProfileAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task UpdateProfileAsync(
        Guid customerId,
       ProfileRequest request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        string identityUserId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}