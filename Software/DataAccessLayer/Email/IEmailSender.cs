using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Email
{
    public interface IEmailSender
    {
        void SendEmail(string to, string subject, string body);
    }
}
