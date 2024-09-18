using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mail = "keketsokeke03@gmail.com";
            var password = "znacxwyzhjlltezs";
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new System.Net.NetworkCredential(mail, password),
                EnableSsl = true
            };
            return client.SendMailAsync(new MailMessage(from: mail, to: email, subject: subject, body: htmlMessage));
        }
    }
}
