using PF2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PF2.Services
{
    public sealed class NodeService
    {
        private static readonly NodeService instance = new NodeService();
        private NodeService() { }
        public static NodeService Instance
        {
            get
            {
                return instance;
            }
        }

        public static void DeleteNode(DefaultConnectionEntities db, int nodeId)
        {
            Node node = db.Node.Find(nodeId);


            //Remove todos os quiz associadas
            for (int i = node.Quiz.Count - 1; i >= 0; i--)
            {
                Quiz q = node.Quiz.ElementAt(i);
                QuizService.DeleteQuiz(db, q.Id);
            }
            //Remove todas as questões associadas
            for (int i = node.Question.Count - 1; i >= 0; i--)
            {
                Question q = node.Question.ElementAt(i);
                QuestionService.DeleteQuestion(db, q.Id);
            }

            //Remove todos os nodes filho
            for (int i = node.Node1.Count - 1; i >= 0; i--)
            {
                Node n = node.Node1.ElementAt(i);
                NodeService.DeleteNode(db, n.Id);
            }


            db.Node.Remove(node);
            db.SaveChanges();
        }

        public static List<Node> FindSons(DefaultConnectionEntities db, int nodeId)
        {
            return (from c in db.Node
                    where c.ParentId == nodeId
                    select c).ToList();
        
        }


        /// <summary>
        /// Recebe uma lista na qual serão inseridos os nodes folha da raiz enviada
        /// </summary>
        /// <param name="db">Conexão com o banco</param>
        /// <param name="rootId">O id do node raiz da árvore</param>
        /// <param name="resultList">A lista que será preenchida com os nós folha</param>
        /// <returns></returns>
        public static void GetTreeLeaves(DefaultConnectionEntities db, int rootId, List<Node> resultList )
        {
            //Se a lista ainda não tiver sido inicializada
            if (resultList == null)
            {
                resultList = new List<Node>();
            }
            List<Node> sons = NodeService.FindSons(db, rootId);
            
            //Se o node raiz em si for uma folha e a árvore possui um nó apenas adiciona o nó raiz na lista e retorna
            if (sons.Count == 0)
            {
                resultList.Add(db.Node.Find(rootId));
                return;
            }

            foreach (Node node in sons)
            {
                //Se não tiver nodes filho, inclui na lista
                if (NodeService.FindSons(db, node.Id).Count == 0)
                {
                    resultList.Add(node);
                }
                //Se tiver nodes filho, manda 
                else
                {
                    GetTreeLeaves(db, node.Id, resultList);
                }
            }


        }



        /// <summary>
        /// Recebe uma lista na qual serão inseridos os nodes da árvore obtida a partir do node raiz enviado
        /// </summary>
        /// <param name="db">Conexão com o banco</param>
        /// <param name="rootId">O id do node raiz da árvore</param>
        /// <param name="resultList">A lista que será preenchida com os nós da árvore</param>
        /// <returns></returns>
        public static void GetTreeNodes(DefaultConnectionEntities db, int rootId, List<Node> resultList)
        {
            //Se a lista ainda não tiver sido inicializada
            if (resultList == null)
            {
                resultList = new List<Node>();
            }
            List<Node> sons = NodeService.FindSons(db, rootId);

            resultList.Add(db.Node.Find(rootId));

            foreach (Node node in sons)
            {
                    GetTreeNodes(db, node.Id, resultList);
            }
        }

        public static bool IsNodeInTree(DefaultConnectionEntities db, int leafId, int rootId)
        {
            List<Node> nodes = new List<Node>();
            NodeService.GetTreeNodes(db, rootId, nodes);
            foreach (Node node in nodes)
            {
                if (node.Id == leafId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}