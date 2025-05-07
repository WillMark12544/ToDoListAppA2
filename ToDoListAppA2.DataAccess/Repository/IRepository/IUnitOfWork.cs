using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListAppA2.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IToDoListNodeRepository toDoListNodes {  get; }
        IMyToDoListRepository myToDoLists { get; }
        ISharedToDoListRepository sharedToDoLists { get; }

        Task<int> SaveAsync();
    }
}
