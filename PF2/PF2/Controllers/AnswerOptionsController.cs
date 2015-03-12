using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PF2.Models;

namespace PF2.Controllers
{
    public class AnswerOptionsController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

        // GET: AnswerOptions
        public ActionResult Index(int questionId)
        {
            /**
            if (QuestionId == null)
            {
                var answerOption = db.AnswerOption.Include(a => a.Question);
                return View(answerOption.ToList());
            }
            **/

            ViewBag.QuestionId = questionId;
            
            List<AnswerOption> resultado = (from a in db.AnswerOption
                                            where a.QuestionId == questionId
                                            select a).ToList();
            return View(resultado);
        }

        // GET: AnswerOptions/Details/5
        public ActionResult Details(int? id, int questionId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnswerOption answerOption = db.AnswerOption.Find(id);
            if (answerOption == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionId = questionId;
            return View(answerOption);
        }
        
        // GET: AnswerOptions/Create
        public ActionResult Create(int questionId)
        {
            ViewBag.QuestionId = questionId;
            ViewBag.QuestionIdSelectList = new SelectList(db.Question, "Id", "Enunciation");
            return View();
        }

        // POST: AnswerOptions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,QuestionId,Answer,IsCorrect")] AnswerOption answerOption, int questionId)
        {
            ViewBag.QuestionId = questionId;
            if (ModelState.IsValid)
            {
                db.AnswerOption.Add(answerOption);
                db.SaveChanges();
                return RedirectToAction("Index", new { questionId = questionId });
            }

            
            ViewBag.QuestionIdSelectList = new SelectList(db.Question, "Id", "Enunciation", answerOption.QuestionId);
            return View(answerOption);
        }

        // GET: AnswerOptions/Edit/5
        public ActionResult Edit(int? id, int questionId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnswerOption answerOption = db.AnswerOption.Find(id);
            if (answerOption == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionId = questionId;
            ViewBag.QuestionIdSelectList = new SelectList(db.Question, "Id", "Enunciation", answerOption.QuestionId);
            return View(answerOption);
        }

        // POST: AnswerOptions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,QuestionId,Answer,IsCorrect")] AnswerOption answerOption, int questionId)
        {
            ViewBag.QuestionId = questionId;
            if (ModelState.IsValid)
            {
                db.Entry(answerOption).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { questionId = questionId });
            }
           
            ViewBag.QuestionIdSelectList = new SelectList(db.Question, "Id", "Enunciation", answerOption.QuestionId);
            return View(answerOption);
        }

        // GET: AnswerOptions/Delete/5
        public ActionResult Delete(int? id, int questionId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AnswerOption answerOption = db.AnswerOption.Find(id);
            if (answerOption == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionId = questionId;
            return View(answerOption);
        }

        // POST: AnswerOptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int questionId)
        {
            AnswerOption answerOption = db.AnswerOption.Find(id);
            db.AnswerOption.Remove(answerOption);
            db.SaveChanges();
            return RedirectToAction("Index", new{questionId = questionId});
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
