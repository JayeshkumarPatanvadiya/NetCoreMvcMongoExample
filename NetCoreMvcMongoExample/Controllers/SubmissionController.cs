using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using System.Net.Http.Headers;
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

        public IActionResult ShowpopUp(string id, IFormCollection form)
        {
            string firstName = HttpContext.Request.Form["Subject"];
            string lastName = HttpContext.Request.Form["Body"];
            var registerViewModel = _subSvc.Find(id);
            return View(registerViewModel);
        }


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
                _mailService.SendEmailAsync(request, submission);
            }
            catch (Exception e)
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
        [HttpPost]

        public string id(string id1)
        {
            return (id1);

        }
        public ActionResult SendMail(Submission submission,  MailRequest request, string id, IFormCollection form)
        {
          
            string Subject = HttpContext.Request.Form["Subject"];           
            string MailCC = HttpContext.Request.Form["MailCC"];
            string MailBCC = HttpContext.Request.Form["MailBCC"];
            IFormFile postedFile = form.Files["Attachments"];
            if (HttpContext.Request.Form.Files != null)
            {
                var fileName = string.Empty;
                string PathDB = string.Empty;
                var files = HttpContext.Request.Form.Files;

                foreach (var file1 in files)
                {
                    if (file1.Length > 0)
                    {
                        //Getting FileName
                        fileName = ContentDispositionHeaderValue.Parse(file1.ContentDisposition).FileName.Trim('"');

                        //Assigning Unique Filename (Guid)
                        var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                        //Getting file Extension
                        var FileExtension = Path.GetExtension(fileName);

                        // concating  FileName + FileExtension
                        var newFileName = myUniqueFileName + FileExtension;

                        // Combines two strings into a path.
                        fileName = Path.Combine(webHostEnvironment.WebRootPath, "Attachments") + $@"\{newFileName}";
                        ViewBag.filename = newFileName;
                        // if you want to store path of folder in database
                        PathDB = "Attachments/" + newFileName;

                        using (FileStream fs = System.IO.File.Create(fileName))
                        {
                            file1.CopyTo(fs);
                            fs.Flush();
                            fs.Dispose();
                            fs.Close();
                           
                        }
                    }
                }
            }
           
            var registerViewModel = _subSvc.Find(id);
            submission = registerViewModel;
            submission.Subject = Subject;
            submission.MailBCC = MailBCC;
            submission.MailCC = MailCC;
            submission.Attachments = ViewBag.filename;
            _mailService.SendEmailAsync(request, submission);
            TempData["mailsent"] = "Mail Sent Successfully!!!";
            return RedirectToAction("Index");
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
