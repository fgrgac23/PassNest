using BusinessLogicLayer.AccountManagement;
using BusinessLogicLayer.PasswordGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.PasswordAudit
{
    public class PasswordAuditor : IPasswordAuditor
    {
        private readonly IAccountStore accountStore;
        private readonly IPasswordGenerator passwordGenerator;

        public PasswordAuditor(IAccountStore accountStore, IPasswordGenerator passwordGenerator)
        {
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;
        }

        public WeakPasswordEntry[] AuditPasswords()
        {
            var weakEntires = new List<WeakPasswordEntry>();

            foreach(var credentials in accountStore.GetAllCredentials())
            {
                var strength = passwordGenerator.EvaluateStrength(credentials.Password);

                if(strength is PasswordStrengthLevel.VrloSlaba or PasswordStrengthLevel.Slaba)
                {
                    weakEntires.Add(new WeakPasswordEntry
                    {
                        AccountId = credentials.AccountId,
                        ServiceName = credentials.ServiceName,
                        Reason = strength == PasswordStrengthLevel.VrloSlaba ? "Vrlo slaba lozinka" : "Slaba lozinka"
                    });
                }
            }

            return weakEntires.ToArray();
        }
    }
}
