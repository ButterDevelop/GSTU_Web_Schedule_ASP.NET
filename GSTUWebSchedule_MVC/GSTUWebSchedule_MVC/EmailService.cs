using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC
{
    public class EmailService
    {
        public static async Task SendEmailAsync(string lang, string email, string subject, string firsttitle, string name, string username, string firstinfo, string firsthref, string firstbutton, string secondhref)
        {
            return;
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(Translate.Tr("GSTUWebSchedule System", lang), ConfigurationManager.AppSettings["EmailUsername"]));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = Translate.Tr(subject, lang);

                string message = Properties.Resources.email;
                message = message.Replace("%FirstTitle%", Translate.Tr(firsttitle, lang));
                message = message.Replace("%Name%", name);
                message = message.Replace("%UserName%", username);
                message = message.Replace("%FirstInfo%", Translate.Tr(firstinfo, lang));
                message = message.Replace("%FirstHref%", firsthref);
                message = message.Replace("%FirstButton%", Translate.Tr(firstbutton, lang));

                message = message.Replace("%SecondTitle%", Translate.Tr("Entertainment heading: random link", lang));
                message = message.Replace("%SecondInfo%", Translate.Tr("To make the letter not so boring, in this heading you can test the strength of a stable cryptographic generator, and see where it takes you.", lang));
                message = message.Replace("%SecondHref%", secondhref);
                int rand = new Random(Guid.NewGuid().GetHashCode()).Next() % 1000000000;
                message = message.Replace("%SecondButton%", Translate.Tr("Button #", lang) + rand.ToString());

                string contentId = Guid.NewGuid().GetHashCode().ToString();//MimeUtils.GenerateMessageId();
                var builder = new BodyBuilder();
                var image = builder.LinkedResources.Add("GSTUWebSchedule.png", Properties.Resources.GSTUWebSchedule);
                image.ContentId = contentId;
                message = message.Replace("%ImgID%", image.ContentId);
                builder.HtmlBody = message;
                emailMessage.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.CheckCertificateRevocation = false;
                    await client.ConnectAsync(ConfigurationManager.AppSettings["EmailHost"], int.Parse(ConfigurationManager.AppSettings["EmailPort"]), true);
                    await client.AuthenticateAsync(ConfigurationManager.AppSettings["EmailUsername"], ConfigurationManager.AppSettings["EmailPassword"]);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            } catch { }
        }
    }
}
