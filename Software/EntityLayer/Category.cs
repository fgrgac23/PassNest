using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSystemdefined { get; set; }
        public string Color { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}
