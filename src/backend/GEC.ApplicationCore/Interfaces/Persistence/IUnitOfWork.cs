
using GEC.ApplicationCore.Interfaces.Repositories;

namespace GEC.ApplicationCore.Interfaces.Persistence;

public interface IUnitOfWork : IDisposable
{
    ITestUserRepository TestUser { get; }
    Task<int> SaveChangesAsync();
}