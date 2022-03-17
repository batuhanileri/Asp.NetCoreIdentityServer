using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer.Helper
{
    public static class EmailConfirmation
    {
        public static void SendEmail(string link, string email)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            mail.From = new MailAddress("bertictest@gmail.com");
            mail.To.Add(email);

            mail.Subject = "Email Doğrulama";
            mail.Body = "<h2>Email Adresinizi Doğrulamak için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'> Email Doğrulama linki</a>";
            mail.IsBodyHtml = true;
            smtpClient.Credentials = new NetworkCredential("bertictest@gmail.com", "****");

            smtpClient.Port = 587;
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.EnableSsl = true;
            smtpClient.Send(mail);
        }
    }
}
