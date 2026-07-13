using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string MasterPasswordHash { get; set; }
        public string MasterPasswordSalt { get; set; }
        public int AutoLockMinutes { get; set; }
        public bool Is2FAEnabled { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
