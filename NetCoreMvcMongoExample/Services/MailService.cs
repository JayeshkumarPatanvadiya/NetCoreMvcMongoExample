//using MailKit.Net.Smtp;
//using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
//using MimeKit;
using NetCoreMvcMongoExample.Models;
using NetCoreMvcMongoExample.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NetCoreMvcMongoExample.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        public MailService(IOptions<MailSettings> mailSettings, IWebHostEnvironment hostEnvironment)
        {
            _mailSettings = mailSettings.Value;
            webHostEnvironment = hostEnvironment;
        }

        public Task SendEmailAsync(MailRequest mailRequest, Submission submission)
        {
            #region Send Email
          
            var fromMail = new MailAddress(_mailSettings.Mail, _mailSettings.DisplayName); // set your email    
            var fromEmailpassword = _mailSettings.Password; // Set your password     
            var toEmail = new MailAddress(submission.Email);
            var smtp = new SmtpClient();
            smtp.Host = _mailSettings.Host;
            smtp.Port = _mailSettings.Port;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            // Add a carbon copy recipient.
            MailAddress copy = new MailAddress(_mailSettings.MailCC);
            Message.CC.Add(copy);
            MailAddress Bcopy = new MailAddress(_mailSettings.MailBCC);
            Message.Bcc.Add(Bcopy);
            Message.Subject = "Registration Completed" + "-" + submission.UserName;

            //Fetching Email Body Text from EmailTemplate File.
            var fileName = Path.GetFileName("SignUp.html");
            string FilePath = Path.Combine(webHostEnvironment.WebRootPath, "EmailTemplates", fileName);

            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(FilePath))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }
            string messageBody = string.Format(builder.HtmlBody,
                        Message.Subject,
                        String.Format("{0:dddd, d MMMM yyyy}", DateTime.Now),
                        submission.UserName + "&nbsp;&nbsp;" + submission.LastName,
                        submission.Email,
                        submission.Phone,
                        Message,
                        submission.UserName
                        );
          
            Message.Body = messageBody;
            Message.IsBodyHtml = true;
            smtp.Send(Message);
            return Task.FromResult<object>(null);
            #endregion
            

        }
    }
}
