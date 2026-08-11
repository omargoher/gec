using GEC.ApplicationCore.Exceptions;
using GEC.ApplicationCore.Interfaces.Repositories;
using GEC.Domain.Entities;
using GEC.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GEC.Infrastructure.Repositories;

public class AddressRepository : BaseRepository<Address>, IAddressRepository
{
    private readonly ApplicationDbContext _context;

    public AddressRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Address>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Addresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Address?> GetByIdAndCustomerIdAsync(Guid addressId, Guid customerId, CancellationToken ct = default)
    {
        return await _context
            .Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == customerId, ct);
    }

    public async Task<Address?> GetDefaultAddressAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(
                a => a.CustomerId == customerId &&
                     a.IsDefault,
                cancellationToken);
    }

    public async Task SetDefaultAsync(
        Guid customerId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var addressExists = await _context.Addresses
            .AnyAsync(
                a => a.Id == addressId &&
                     a.CustomerId == customerId,
                cancellationToken);

        if (!addressExists)
            throw new NotFoundException("Address not found.");

        // EnableRetryOnFailure is configured on this context, so a bare
        // BeginTransactionAsync here would throw ("execution strategy does
        // not support user-initiated transactions"). Both updates need to
        // land together — otherwise a crash between them leaves the
        // customer with zero default addresses — so we go through the
        // execution strategy and let it retry the whole block atomically.
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            await _context.Addresses
                .Where(a => a.CustomerId == customerId && a.IsDefault)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(a => a.IsDefault, false),
                    cancellationToken);

            await _context.Addresses
                .Where(a => a.Id == addressId && a.CustomerId == customerId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(a => a.IsDefault, true),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task<bool> ExistsForCustomerAsync(Guid addressId, Guid customerId, CancellationToken ct = default)
    {
        return await _context.Addresses
            .AnyAsync(a => a.Id == addressId && a.CustomerId == customerId, ct);
    }

    public async Task<bool> HasAnyAddressAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Addresses
            .AnyAsync(a => a.CustomerId == customerId, ct);
    }
}