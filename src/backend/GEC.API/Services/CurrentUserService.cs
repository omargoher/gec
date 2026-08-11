using System.Security.Claims;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Repositories;

namespace GEC.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICustomerRepository _customerRepository;
    private Guid? _cachedCustomerId;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ICustomerRepository customerRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _customerRepository = customerRepository;
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

        var customer = await _customerRepository.GetByIdentityUserIdAsync(UserId, CancellationToken.None);

        if (customer is null)
            throw new NotFoundException("Customer");

        _cachedCustomerId = customer.Id;
        return customer.Id;
    }
}