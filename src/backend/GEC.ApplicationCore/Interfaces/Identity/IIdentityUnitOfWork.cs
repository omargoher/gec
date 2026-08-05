namespace GEC.ApplicationCore.Interfaces.Identity;

public interface IIdentityUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}