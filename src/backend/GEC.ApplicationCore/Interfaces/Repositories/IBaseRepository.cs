namespace GEC.ApplicationCore.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public void Add(T entity);
    public void Update(T entity);
    public void Remove(T entity);
}