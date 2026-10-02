using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ATS.Application.Abstract.Repository;
using ATS.Domain.Entities.Base;
using ATS.Infrastructure.Persistence.Context.Mssql;
using Microsoft.EntityFrameworkCore;

namespace ATS.Infrastructure.Concrete.Repository;

public class RepositoryBase<T> : IRepositoryBase<T> where T : class, IEntity, new()
{
	private readonly ApplicationDbContext _context;

	private readonly DbSet<T> _dbSet;

	public RepositoryBase(ApplicationDbContext context)
	{
		_context = context;
		_dbSet = _context.Set<T>();
	}

	public async Task<T?> GetByIdAsync(int id)
	{
		return await _dbSet.FindAsync(id);
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		return await _dbSet.ToListAsync();
	}

	public async Task AddAsync(T entity)
	{
		await _dbSet.AddAsync(entity);
	}

	public Task UpdateAsync(T entity)
	{
		_dbSet.Update(entity);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(T entity)
	{
		_dbSet.Remove(entity);
		return Task.CompletedTask;
	}

	public async Task<T?> GetWithIncludesAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
	{
		IQueryable<T> query = _dbSet;
		foreach (Expression<Func<T, object>> include in includes)
		{
			query = query.Include(include);
		}
		return await query.FirstOrDefaultAsync(predicate);
	}

	public async Task<T?> GetWithIncludePathsAsync(Expression<Func<T, bool>> predicate, params string[] includePaths)
	{
		IQueryable<T> query = _dbSet;
		foreach (string includePath in includePaths)
		{
			query = query.Include(includePath);
		}
		return await query.FirstOrDefaultAsync(predicate);
	}

	public async Task<int> SaveChangesAsync()
	{
		return await _context.SaveChangesAsync();
	}
}
