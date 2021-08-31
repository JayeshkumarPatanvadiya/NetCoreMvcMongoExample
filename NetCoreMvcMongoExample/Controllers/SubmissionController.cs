using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NetCoreMvcMongoExample.Models;
using NetCoreMvcMongoExample.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NetCoreMvcMongoExample.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly SubmissionService _subSvc;
        public SubmissionController(SubmissionService submissionService)
        {
            _subSvc = submissionService;
            var client = new MongoClient("mongodb+srv://sa:Jayesh%40123@cluster0.qwyip.mongodb.net/test?retryWrites=true&w=majority");
            var dbs = client.ListDatabaseNames().ToListAsync();
            Console.WriteLine(dbs);
        }

        [AllowAnonymous]
        public ActionResult<IList<Submission>> Index() => View(_subSvc.Read());

        [HttpGet]
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult<Submission> Create(Submission submission)
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
            }
            catch(Exception e)
            {

            }
            //if (ModelState.IsValid)
            //{
               
            //}
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
                //if (User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value != submission.UserId)
                //{
                //    return Unauthorized();
                //}
                _subSvc.Update(submission);
                return RedirectToAction("Index");
            }
            return View(submission);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            _subSvc.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
