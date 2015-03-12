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
    public class QuestionsController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

        // GET: Questions
        public ActionResult Index()
        {
            var question = db.Question.Include(q => q.Node);
            return View(question.ToList());
        }




        // GET: Questions/Create
        public ActionResult Create()
        {
            ViewBag.NodeId = new SelectList(db.Node, "Id", "Name");
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,NodeId,Enunciation")] Question question)
        {
            if (ModelState.IsValid)
            {
                db.Question.Add(question);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.NodeId = new SelectList(db.Node, "Id", "Name", question.NodeId);
            return View(question);
        }



        /************************************************/
        // OPERAÇÕES COM O MODELO DE PERGUNTAS DE UM NODE DETERMINADO
        /************************************************/
        public ActionResult ListNodeQuestions(int nodeId)
        {
            ViewBag.NodeId = nodeId;
            List<Question> resultado = (from q in db.Question
                                        where q.NodeId == nodeId
                                        select q).ToList();

            return View(resultado);
        }

        // GET: Questions/Create
        public ActionResult CreateQuestionToNode(int nodeId)
        {
            ViewBag.NodeId = nodeId;
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateQuestionToNode(Question question, int nodeId)
        {
            ViewBag.NodeId = nodeId;
            if (ModelState.IsValid)
            {
                db.Question.Add(question);
                db.SaveChanges();
                return RedirectToAction("ListNodeQuestions", new { nodeId = nodeId });
            }

    
            return View(question);
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int? id, int nodeId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Question.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }

            ViewBag.NodeId = nodeId;
            ViewBag.NodeIdSelectList = new SelectList(db.Node, "Id", "Name", question.NodeId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Question question, int nodeId)
        {
            ViewBag.NodeId = nodeId;

            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ListNodeQuestions", new { nodeId = nodeId });
            }
            ViewBag.NodeIdSelectList = new SelectList(db.Node, "Id", "Name", question.NodeId);
            return View(question);
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int? id, int nodeId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.NodeId = nodeId;
            Question question = db.Question.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int nodeId)
        {
            ViewBag.NodeId = nodeId;
            Question question = db.Question.Find(id);
            List<AnswerOption> answerOptionsToDelete = question.Answer.ToList();

            foreach(AnswerOption a in answerOptionsToDelete)
            {
                db.AnswerOption.Remove(a);
            }
            db.Question.Remove(question);
            db.SaveChanges();
            return RedirectToAction("ListNodeQuestions", new { nodeId = nodeId });
        }

        // GET: Questions/Details/5
        public ActionResult Details(int? id, int nodeId)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.NodeId = nodeId;
            Question question = db.Question.Find(id);
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
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
