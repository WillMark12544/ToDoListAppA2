using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAppA2.Data;
using ToDoListAppA2.DataAccess.Repository.IRepository;

namespace ToDoListAppA2.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context; 
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T item)
        {
            await _dbSet.AddAsync(item);
        }

        public Task DeleteAsync(T item)
        {
            _dbSet.Remove(item);
            return Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var item = await _dbSet.FindAsync(id);
            return item != null;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public Task UpdateAsync(T item)
        {
            _dbSet.Update(item);
            return Task.CompletedTask;
        }
    }
}
