using GEC.ApplicationCore.DTOs.Profile;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.ApplicationCore.Interfaces.Services;

namespace GEC.ApplicationCore.Services;

public class ProfileService : IProfileService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;

    public ProfileService(
        ICustomerRepository customerRepository,
        IIdentityService identityService, IUnitOfWork unitOfWork, IAuthenticationService authenticationService)
    {
        _customerRepository = customerRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<ProfileDto> GetProfileAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {

        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer");

        return new ProfileDto(
            customer.Name,
            customer.Email
            );
    }

    public async Task UpdateProfileAsync(Guid customerId, ProfileRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customer is null)
            throw new NotFoundException("Customer");

        customer.Name = request.FullName;

        await _unitOfWork.SaveChangesAsync(cancellationToken);


    }

    public async Task ChangePasswordAsync(string identityUserId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        await _identityService.ChangePassword(identityUserId, request.CurrentPassword, request.NewPassword);

        await _authenticationService.LogoutAllAsync(identityUserId, cancellationToken);
    }
}