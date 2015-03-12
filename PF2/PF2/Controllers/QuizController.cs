using PF2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Security.Claims;
using System.Data.Entity;
using PF2.Services;

namespace PF2.Controllers
{
    public class QuizController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

                private readonly UserManager<AppUser> userManager;

        public QuizController()
            : this(Startup.UserManagerFactory.Invoke())
        {
        }

        public QuizController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }


        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListQuizOfUser()
        {
           // string currentUserId = User.Identity.GetUserId();
            var user = userManager.FindById(User.Identity.GetUserId());
            int currentUserId = Int32.Parse("1");

            List<Quiz> resultado = (from q in db.Quiz
                                            where q.AspNetUsersId.Equals(user.Id)
                                            select q).ToList();
            return View(resultado);
        }

        [HttpGet]
        public ActionResult ListTrees()
        {
            using (var db = new DefaultConnectionEntities())
            {
                List<Node> resultado = (from c in db.Node
                                        where c.ParentId == null
                                        select c).ToList();
                return View(resultado);
            }
        }

        [HttpGet]
        public ActionResult VisualizeTree(int id)
        {
            return View(GetNodeSons(id));
        }


        public Tree GetNodeSons(int id)
        {
            using (var db = new DefaultConnectionEntities())
            {
                Tree tree = new Tree();
                Node node = db.Node.Find(id);
                tree.node = node;

                List<Node> resultado = (from c in db.Node
                                        where c.ParentId == id
                                        select c).ToList();

                foreach (var n in resultado)
                {
                    tree.sons.Add(GetNodeSons(n.Id));
                }

                return tree;
            }
        }

        // GET: Nodes/Create
        [HttpGet]
        public ActionResult Create(int nodeId)
        {
            Node n = db.Node.Find(nodeId);
            ViewBag.NameNode = n.Name;
            ViewBag.DescriptionNode = n.Description;

            ViewBag.NodeId = nodeId;
            return View();
        }

        // POST: Nodes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                quiz.StartTime = DateTime.Now;
                quiz.AspNetUsersId = User.Identity.GetUserId();
                db.Quiz.Add(quiz);
                db.SaveChanges();
                return RedirectToAction("StartQuiz", new { quizId = quiz.Id});
            }

            //ViewBag.ParentId = new SelectList(db.Node, "NodeID", "Name", node.ParentID);
            return View(quiz);
        }


        [HttpGet]
        public ActionResult StartQuiz(int quizId)
        {
            //teste
            QuestionSelector q = new QuestionSelector(db.Quiz.Find(quizId));
            Session["Quiz"] = q;
            return RedirectToAction("AskQuestion");
        }


        [HttpGet]
        public ActionResult AskQuestion()            
        {
            var selector = Session["Quiz"] as QuestionSelector;
            if (selector.status != Status.InProgress)
            {
                return RedirectToAction("EndQuiz");
            }
            AskQuestionPageData mockup = new AskQuestionPageData(selector.getNewQuestion());
            return View(mockup);
        }

        [HttpPost]
        public ActionResult AskQuestion(AskQuestionPageData mockup)
        {
            var selector = Session["Quiz"] as QuestionSelector;
            selector.sendQuestionAnswer(mockup.chosen);
            if(selector.status != Status.InProgress)
            {
                return RedirectToAction("EndQuiz");
            }
            mockup = new AskQuestionPageData(selector.getNewQuestion());
            return View(mockup);
        }


        [HttpGet]
        public ActionResult EndQuiz( )
        {

           var selector = Session["Quiz"] as QuestionSelector;

           Status s = selector.status;
           //Atualiza o quiz com a seu momento de término
           Quiz q = db.Quiz.Find(selector.quiz.Id);
           q.EndTime = DateTime.Now;
           db.Entry(q).State = EntityState.Modified;
           db.SaveChanges();

            //Monta a mensagem que descreve o motivo pelo qual o quiz terminou.
            if (s == Status.NQuestionsAnswered)
            {
                ViewBag.EndingReason = selector.quiz.NumQuestions + " questions answered";
            }
            else if (s == Status.NoMoreValidNodes)
            {
                ViewBag.EndingReason = "No more valid Nodes";
            }

            //Libera a memória de sessão utilizada para o algoritmo de seleção de perguntas
           Session["Quiz"] = null;


           return View(q.Id);
        }



        [HttpGet]
        public ActionResult VisualizeTreeReport(int quizId)
        {
            Quiz quiz = db.Quiz.Find(quizId);
            TreeReport tree = BuildReportTree(quizId, quiz.NodeId, null);
            return View(tree);
        
        }


        public TreeReport BuildReportTree(int quizId, int nodeId, TreeReport father)
        {

                TreeReport tree = new TreeReport(db, nodeId,quizId,father);

                //Obtem todos os nodes filhos
                List<Node> sons = NodeService.FindSons(db, nodeId);

                foreach (var n in sons)
                {
                    tree.sons.Add(BuildReportTree(quizId, n.Id, tree));
                }
                return tree;
            
        }



    }
}