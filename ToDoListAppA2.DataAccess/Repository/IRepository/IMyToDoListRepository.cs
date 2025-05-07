using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAppA2.Models;

namespace ToDoListAppA2.DataAccess.Repository.IRepository
{
    public interface IMyToDoListRepository : IRepository<ToDoList>
    {
        Task<List<ToDoList>> GetUserToDoListsAsync(string userId);
        Task<int> CountUserToDoListsAsync(string userId);
        Task<bool> IsSharedWithUserAsync(int toDoListId, string  userId);
    }
}
