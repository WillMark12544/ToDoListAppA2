using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAppA2.Data;
using ToDoListAppA2.DataAccess.Repository.IRepository;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.DataAccess.Repository
{
    public class SharedToDoListRepository : Repository<ToDoListShare>, ISharedToDoListRepository
    {
        private readonly ApplicationDbContext _context;

        public SharedToDoListRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<List<ToDoList?>> GetSharedToDoListsForUserAsync(string userId)
        {
            return await _context.ToDoListShares
                .Where(ts => ts.SharedWithUserId == userId)
                .Include(ts => ts.ToDoList)
                .ThenInclude(t => t.SharedWith)
                .ThenInclude(t => t.SharedWithUser)
                .Select(ts => ts.ToDoList)
                .ToListAsync();
        }

        public async Task<List<ToDoList?>> GetUnarchivedSharedToDoListsForUserAsync(string userId)
        {
            return await _context.ToDoListShares
                .Where(ts => ts.SharedWithUserId == userId && !ts.ToDoList.Archived)
                .Include(ts => ts.ToDoList)
                .ThenInclude(t => t.SharedWith)
                .ThenInclude(t => t.SharedWithUser)
                .Select(ts => ts.ToDoList)
                .ToListAsync();
        }

        public async Task<List<ToDoList?>> GetArchivedSharedToDoListsForUserAsync(string userId)
        {
            return await _context.ToDoListShares
                .Where(ts => ts.SharedWithUserId == userId && ts.ToDoList.Archived)
                .Include(ts => ts.ToDoList)
                .ThenInclude(t => t.SharedWith)
                .ThenInclude(t => t.SharedWithUser)
                .Select(ts => ts.ToDoList)
                .ToListAsync();
        }

        public async Task<ToDoListShare?> GetShareEntriesForToDoListAsync(int toDoListId, string userId)
        {
            return await _context.ToDoListShares
                .FirstOrDefaultAsync(ts => ts.ToDoListId == toDoListId && ts.SharedWithUserId == userId);
        }
    }
}
