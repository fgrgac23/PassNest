using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Url { get; set; }
        public string EncryptedPassword { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
