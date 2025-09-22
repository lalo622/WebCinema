using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace WebCinema.Helper
{
    public static class EmailHelper
    {
        public static void SendBookingSuccessEmail(string toEmail, string subject, string body)
        {
            var fromEmail = ConfigurationManager.AppSettings["EmailFrom"];
            var fromPassword = ConfigurationManager.AppSettings["EmailPassword"];

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            var message = new MailMessage(fromEmail, toEmail, subject, body);
            message.IsBodyHtml = true;
            smtp.Send(message);
        }
    }
}