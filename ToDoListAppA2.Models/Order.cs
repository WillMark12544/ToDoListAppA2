using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoListAppA2.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ItemName { get; set; } = "Premium";
        public decimal Price { get; set; } = 9.99m;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public bool IsPaid { get; set; } = false;
    }
}
