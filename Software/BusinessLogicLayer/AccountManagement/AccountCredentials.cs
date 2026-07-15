using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.AccountManagement
{
    public class AccountCredentials
    {
        public int AccountId { get; init; }
        public string ServiceName { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
