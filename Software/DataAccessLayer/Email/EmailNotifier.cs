using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Email
{
    public class EmailNotifier : IEmailSender
    {
        private readonly string SmtpHost;
        private readonly int SmtpPort;
        private readonly string SenderEmail;
        private readonly string SenderPassword;

        public EmailNotifier(string smtpHost, int smtpPort, string senderEmail, string senderPassword)
        {
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SenderEmail = senderEmail;
            SenderPassword = senderPassword;
        }
        public void SendEmail(string to, string subject, string body)
        {
            using var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = new NetworkCredential(SenderEmail, SenderPassword),
                EnableSsl = true
            };

            using var message = new MailMessage(SenderEmail, to, subject, body);
            client.Send(message);
        }
    }
}
