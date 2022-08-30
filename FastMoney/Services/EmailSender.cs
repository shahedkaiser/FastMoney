using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace FastMoney.Services
{
    public class EmailSender : IEmailSender
    {

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("mail.valuationascensionbank.com")
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("mail@valuationascensionbank.com", "wb7AVEg44@HyPr9XH")
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress("mail@valuationascensionbank.com", "Valuation Ascension Bank")
            };
            mailMessage.To.Add(email);
            mailMessage.Subject = subject;
            mailMessage.Body = htmlMessage;
            mailMessage.IsBodyHtml = true;
            return client.SendMailAsync(mailMessage);
        }

    }
}
