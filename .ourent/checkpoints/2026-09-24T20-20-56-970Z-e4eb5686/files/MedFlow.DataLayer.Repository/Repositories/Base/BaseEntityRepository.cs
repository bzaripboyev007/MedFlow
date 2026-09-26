using MedFlow.DataLayer.Repository.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MedFlow.DataLayer.Repository.Repositories.Base;

public class BaseEntityRepository<T, TKey> where T : class
{
    protected readonly MedFlowDbContext Context;
    private readonly DbSet<T> _set;

    public BaseEntityRepository(MedFlowDbContext context)
    {
        Context = context;
        _set = context.Set<T>();
    }

    public IQueryable<T> Query() => _set.AsQueryable();

    public async Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default) => await _set.FindAsync(new object?[] { id }, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);
}
