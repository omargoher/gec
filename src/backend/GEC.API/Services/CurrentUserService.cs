using System.Security.Claims;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Persistence;

namespace GEC.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private Guid? _cachedCustomerId;
    private Guid? _cachedAdminId;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public async Task<Guid> GetCustomerIdAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedCustomerId.HasValue)
        {
            return _cachedCustomerId.Value;
        }

        var customer = await _unitOfWork.Customer.GetByIdentityUserIdAsync(UserId, CancellationToken.None);

        if (customer is null)
            throw new NotFoundException("Customer");

        _cachedCustomerId = customer.Id;
        return customer.Id;
    }

    public async Task<Guid> GetCustomerIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customer.GetByEmailAsync(email, cancellationToken); 
        if (customer is null)
            throw new NotFoundException("Customer");

        return customer.Id;
    }
    
    public async Task<Guid> GetAdminIdAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedAdminId.HasValue)
        {
            return _cachedAdminId.Value;
        }

        var admin = await _unitOfWork.Admin.GetByIdentityUserIdAsync(UserId, CancellationToken.None);

        if (admin is null)
            throw new NotFoundException("Admin");

        _cachedAdminId = admin.Id;
        return admin.Id;
    }
}