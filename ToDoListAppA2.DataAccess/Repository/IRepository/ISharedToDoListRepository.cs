using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.DataAccess.Repository.IRepository
{
    public interface ISharedToDoListRepository : IRepository<ToDoListShare>
    {
        Task<List<ToDoList?>> GetSharedToDoListsForUserAsync(string userId);
        Task<ToDoListShare?> GetShareEntriesForToDoListAsync(int toDoListId, string userId);
    }
}
