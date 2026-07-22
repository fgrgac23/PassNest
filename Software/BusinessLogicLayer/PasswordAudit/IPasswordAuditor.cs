using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordAudit
{
    public interface IPasswordAuditor
    {
        WeakPasswordEntry[] AuditPasswords();
    }
}
