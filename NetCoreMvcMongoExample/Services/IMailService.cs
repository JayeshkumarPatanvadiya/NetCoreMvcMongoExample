using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net;
using NetCoreMvcMongoExample.Models;

namespace NetCoreMvcMongoExample.Services
{
  public  interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest, Submission submission);
    }
}
