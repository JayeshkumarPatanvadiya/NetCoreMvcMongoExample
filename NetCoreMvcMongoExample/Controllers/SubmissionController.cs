using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MongoDB.Driver;
using NetCoreMvcMongoExample.Models;
using NetCoreMvcMongoExample.Services;
using NetCoreMvcMongoExample.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreMvcMongoExample.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly SubmissionService _subSvc;
        private readonly IMailService _mailService;
        private readonly MailSettings mailsettings;
        private readonly IWebHostEnvironment webHostEnvironment;
        public SubmissionController(SubmissionService submissionService, IWebHostEnvironment hostEnvironment, IMailService mailService, IOptions<MailSettings> mailSettings)
        {
            _subSvc = submissionService;           
            mailsettings = mailSettings.Value;
            webHostEnvironment = hostEnvironment;
            _mailService = mailService;
        }

        [AllowAnonymous]
        public ActionResult<IList<Submission>> Index() => View(_subSvc.Read());

        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult<Submission> Create(Submission submission, MailRequest request)
        {
            submission.Created = submission.LastUpdated = DateTime.Now;
            Guid obj = Guid.NewGuid();
            submission.UserId = Convert.ToString(obj);
            Guid obj2 = Guid.NewGuid();
            submission.Id = Convert.ToString(obj2);
            submission.UserName = submission.UserName;

            try
            {
                _subSvc.Create(submission);              
                _mailService.SendEmailAsync(request,submission);
            }
            catch(Exception e)
            {               
                throw;
            }
            
            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult<Submission> Edit(string id) =>
            View(_subSvc.Find(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Submission submission)
        {
            submission.LastUpdated = DateTime.Now;
            submission.Created = submission.Created.ToLocalTime();
            if (ModelState.IsValid)
            {
                
                _subSvc.Update(submission);
                return RedirectToAction("Index");
            }
            return View(submission);
        }
               
        public ActionResult SendMail(Submission submission, MailRequest request,string id)
        {
        
            if (ModelState.IsValid)
            {
                var registerViewModel = _subSvc.Find(id);
              
                submission = registerViewModel;
                _mailService.SendEmailAsync(request, submission);
                TempData["mailsent"] = "Mail Sent Successfully!!!";
                return RedirectToAction("Index");
            }
            TempData["Emailsend"] = false;
            return View(_subSvc.Find(id));
        }
       
        [HttpGet]
        public ActionResult Delete(string id)
        {
            _subSvc.Delete(id);
             TempData["TDFriend"] = "Record Deleted SuccessFully!!!";
            return RedirectToAction("Index");
           
        }
    }
}
