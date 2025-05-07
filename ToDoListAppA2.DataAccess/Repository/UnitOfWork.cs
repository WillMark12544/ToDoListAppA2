using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoListAppA2.Data;
using ToDoListAppA2.DataAccess.Repository.IRepository;

namespace ToDoListAppA2.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IToDoListNodeRepository toDoListNodes { get; private set; }

        public IMyToDoListRepository myToDoLists { get; private set; }

        public ISharedToDoListRepository sharedToDoLists { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            toDoListNodes = new ToDoListNodeRepository(_context);
            myToDoLists = new MyToDoListRepository(_context);
            sharedToDoLists = new SharedToDoListRepository(_context);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
