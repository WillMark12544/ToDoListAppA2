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
    public class MyToDoListRepository : Repository<ToDoList>, IMyToDoListRepository
    {
        private readonly ApplicationDbContext _context;

        public MyToDoListRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<int> CountUserToDoListsAsync(string userId)
        {
            return await _context.ToDoLists
                .CountAsync(t => t.UserId == userId);
        }

        public async Task<List<ToDoList>> GetUserToDoListsAsync(string userId)
        {
            return await _context.ToDoLists
                .Where(t => t.UserId == userId)
                .Include(t => t.User)
                .Include(t => t.SharedWith)
                .ThenInclude(ts => ts.SharedWithUser)
                .Include(t => t.ToDoListNodes)
                .ToListAsync();
        }

        public async Task<ToDoList?> GetNodesForToDoList(int id)
        {
            return await _context.ToDoLists
                .Include(t => t.ToDoListNodes)
                .Include(t => t.SharedWith)
                    .ThenInclude(ts => ts.SharedWithUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }


        public async Task<bool> IsSharedWithUserAsync(int toDoListId, string userId)
        {
            return await _context.ToDoListShares
                .AnyAsync(ts => ts.ToDoListId == toDoListId && ts.SharedWithUserId == userId);
        }
    }
}
