using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Asp.NetCoreIdentityServer.Helper
{
    public static class PasswordReset
    {
        public static void PasswordResetSendEmail(string link,string email)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            mail.From = new MailAddress("bertictest@gmail.com");
            mail.To.Add(email);

            mail.Subject = "Şifre Sıfırlama";
            mail.Body = "<h2>Şifrenizi yenilemek için lütfen aşağıdaki linke tıklayınız.</h2><hr/>";
            mail.Body += $"<a href='{link}'> şifre yenileme linki</a>";
            mail.IsBodyHtml = true;
            smtpClient.Credentials = new NetworkCredential("bertictest@gmail.com", "Batu..16");

            smtpClient.Port = 587;
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.EnableSsl = true;
            smtpClient.Send(mail);
        }
    }
}
