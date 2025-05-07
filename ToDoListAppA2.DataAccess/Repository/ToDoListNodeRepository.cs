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
    public class ToDoListNodeRepository : Repository<ToDoListNode>, IToDoListNodeRepository
    {
        private readonly ApplicationDbContext _context;
        public ToDoListNodeRepository(ApplicationDbContext context) 
            : base(context)
        {
            _context = context;
        }
    }
}
