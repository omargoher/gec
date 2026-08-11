using GEC.ApplicationCore.DTOs;
using GEC.ApplicationCore.DTOs.Auth;
using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Identity;
using GEC.ApplicationCore.Interfaces.Persistence;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.ApplicationCore.Interfaces.Services;
using GEC.ApplicationCore.Options;
using GEC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GEC.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly JwtOptions _jwtOptions;
    private readonly IOtpManager _otpManager;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _context;

    public AuthenticationService(
        IIdentityService identityService,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IIdentityUnitOfWork unitOfWork,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthenticationService> logger,
        IOtpManager otpManager,
        ICustomerRepository customerRepository,
        IUnitOfWork context)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtOptions = jwtOptions.Value;
        _otpManager = otpManager;
        _logger = logger;
        _customerRepository = customerRepository;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _identityService.FindByEmailAsync(request.Email);

        if (user is null)
            throw new UnauthorizedException("Invalid email or password.");

        if (await _identityService.IsLockedOutAsync(user.Id))
            // Deliberately the same exception/status as invalid credentials below.
            // Returning 403 here would let a caller distinguish "this account
            // exists and is locked" from "wrong password" / "no such account",
            // which is an account-enumeration and lockout-state leak.
            throw new UnauthorizedException("Invalid email or password.");

        if (await _identityService.IsEmailConfirmedAsync(user.Id) == false)
            throw new UnauthorizedException("Email is not confirmed.");

        var passwordValid =
            await _identityService.CheckPasswordAsync(user.Id, request.Password);

        if (!passwordValid)
        {
            await _identityService.AccessFailedAsync(user.Id);

            throw new UnauthorizedException("Invalid email or password.");
        }

        await _identityService.ResetAccessFailedCountAsync(user.Id);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var rawRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashToken(rawRefreshToken);

        var refreshToken = CreateRefreshToken(
            user.Id,
            refreshTokenHash,
            Guid.NewGuid(),
            DateTime.UtcNow);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, rawRefreshToken);
    }

    public async Task<AuthResponse> RefreshAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await FindRefreshTokenAsync(
            rawRefreshToken,
            cancellationToken);

        if (refreshToken is null)
            throw new UnauthorizedException("Invalid refresh token.");

        if (refreshToken.IsExpired)
            throw new UnauthorizedException("Invalid refresh token.");

        if (refreshToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeFamilyAsync(
                refreshToken.FamilyId,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Invalid refresh token.");
        }

        var user = await _identityService.FindByIdAsync(refreshToken.UserId);

        if (user is null)
            throw new UnauthorizedException("Invalid refresh token.");

        var accessToken = _tokenService.GenerateAccessToken(user);

        var newRawRefreshToken = _tokenService.GenerateRefreshToken();

        var newRefreshTokenHash =
            _tokenService.HashToken(newRawRefreshToken);

        var now = DateTime.UtcNow;

        refreshToken.RevokedAtUtc = now;
        refreshToken.ReplacedByTokenHash = newRefreshTokenHash;

        var newRefreshToken = CreateRefreshToken(
            user.Id,
            newRefreshTokenHash,
            refreshToken.FamilyId,
            now);

        await _refreshTokenRepository.AddAsync(
            newRefreshToken,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else (another request, or a replay) already rotated
            // this exact refresh token first. Reject this attempt rather
            // than let two children spawn from the same parent token.
            throw new UnauthorizedException("Invalid refresh token.");
        }

        return new AuthResponse(
            accessToken,
            newRawRefreshToken);
    }

    public async Task LogoutAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await FindRefreshTokenAsync(
            rawRefreshToken,
            cancellationToken);

        if (refreshToken is null || refreshToken.IsRevoked)
            return;

        refreshToken.RevokedAtUtc = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LogoutAllAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        await _refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<RefreshToken?> FindRefreshTokenAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawRefreshToken);

        var tokenHash = _tokenService.HashToken(rawRefreshToken);

        return await _refreshTokenRepository.GetByHashAsync(
            tokenHash,
            cancellationToken);
    }

    private RefreshToken CreateRefreshToken(
        string userId,
        string tokenHash,
        Guid familyId,
        DateTime now)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenDays),
            FamilyId = familyId
        };
    }

    /*
     * TODO: Create Transactional for create email and assign role
     */
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingUser = await _identityService.FindByEmailAsync(
            request.Email);

        if (existingUser is not null)
            throw new ConflictException("Email already exists.");

        string? userId = null;

        try
        {
            // 1. Create Identity user
            var result = await _identityService.CreateUserAsync(
                request.Email,
                request.Password);

            if (!result.Succeeded)
            {
                throw new InvalidRequestException(
                    string.Join(", ", result.Errors));
            }

            userId = result.UserId;

            // 2. Assign default role
            await _identityService.AddToRoleAsync(
                userId,
                "Customer");

            // 3. Create Customer
            var customer = new Customer
            {
                IdentityUserId = userId,
                Name = request.Name,
                Email = request.Email,
                Addresses = []
            };

            _customerRepository.Add(customer);

            // 4. Save Customer
            await _context.SaveChangesAsync(
                cancellationToken);

            // TODO convert it to background job
            try
            {
                await _otpManager.SendOtpAsync(request.Email, OtpPurpose.EmailVerification, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification OTP to {Email} during registration.", request.Email);
            }

            // 5. Everything succeeded
            return new RegisterResponse(
                userId,
                request.Email,
                request.Name,
                ["Customer"]);
        }
        catch
        {
            // Identity user was created, but something later failed.
            if (userId is not null)
            {
                try
                {
                    await _identityService.DeleteUserAsync(userId);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Orphan Identity user created. Failed to delete UserId: {UserId}", userId);
                }
            }

            throw;
        }
    }
}