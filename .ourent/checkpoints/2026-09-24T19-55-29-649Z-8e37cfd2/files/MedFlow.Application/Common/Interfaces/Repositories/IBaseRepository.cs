namespace MedFlow.Application.Common.Interfaces.Repositories;

public interface IBaseRepository<T, TKey> where T : class
{
    IQueryable<T> Query();
    Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
