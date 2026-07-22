using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordAudit
{
    public class WeakPasswordEntry
    {
        public int AccountId { get; init; }
        public string ServiceName { get; init; } = string.Empty;
        public string Reason { get; init; } = string.Empty;
    }
}
