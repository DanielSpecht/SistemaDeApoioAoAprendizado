using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PF2.Models;
using PF2.Services;

namespace PF2.Controllers
{
    public class NodesController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

        // GET: Nodes
        public ActionResult List()
        {
            var node = db.Node.Include(n => n.Node2);
            return View(node.ToList());
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


        // GET: Nodes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Node node = db.Node.Find(id);
            if (node == null)
            {
                return HttpNotFound();
            }
            return View(node);
        }

        // GET: Nodes/Create
        public ActionResult Create(int? parentId)
        {
            ViewBag.ParentId = parentId;
            return View();
        }

        // POST: Nodes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NodeID,Name,Description,ParentID")] Node node)
        {
            if (ModelState.IsValid)
            {
                db.Node.Add(node);
                db.SaveChanges();
                return RedirectToAction("VisualizeTree", new { id = GetTreeRootFromNode(node.Id).Id });
            }

            //ViewBag.ParentId = new SelectList(db.Node, "NodeID", "Name", node.ParentID);
            return View(node);
        }

        // GET: Nodes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Node node = db.Node.Find(id);
            if (node == null)
            {
                return HttpNotFound();
            }
            if (node.ParentId != null)
            {
                ViewBag.ParentId = new SelectList(db.Node, "Id", "Name", node.ParentId);
            }
            return View(node);
        }

        // POST: Nodes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Node node)
        {
            if (ModelState.IsValid)
            {
                db.Entry(node).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("VisualizeTree", new { id = GetTreeRootFromNode(node.Id).Id });
            }
            ViewBag.ParentId = new SelectList(db.Node, "NodeID", "Name", node.ParentId);
            return View(node);
        }

        // GET: Nodes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Node node = db.Node.Find(id);
            if (node == null)
            {
                return HttpNotFound();
            }
            return View(node);
        }

        // POST: Nodes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            Node node = db.Node.Find(id);
            bool isRoot=(node.ParentId == null);

            //Define para o controlador redirecionar para o controlador VisualizeTree passando o id da raiz da árvore
            ActionResult returnValue = RedirectToAction("VisualizeTree", new { id = GetTreeRootFromNode(node.Id).Id });
          
            //Se o node sendo deletado for o node raiz, redireciona para a lista de arvores
            if (isRoot)
            {
                returnValue = RedirectToAction("ListTrees");
            }

            NodeService.DeleteNode(db,id);
            /**
            db.Node.Remove(node);
            db.SaveChanges();
            **/
            return returnValue;
        }

        [HttpGet]
        public ActionResult VisualizeFullTree(int id)
        {

            return RedirectToAction("VisualizeTree", new { id = GetTreeRootFromNode(id).Id });
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

        private Node GetTreeRootFromNode(int nodeID)
        {

            Node node = db.Node.Find(nodeID);
            while (node.ParentId != null)
            {
                node = db.Node.Find(node.ParentId);
            }

            return node;
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
