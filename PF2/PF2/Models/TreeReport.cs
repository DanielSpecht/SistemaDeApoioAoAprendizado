using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PF2.Services;
namespace PF2.Models
{
    public class TreeReport
    {
        public Node node;
        public int quizId;

        public int nQuestions;

        public int nCorrectlyAnsweredQuestions;
        public int nNotCorrectlyAnswered;

        public int nIncorrectlyAnsweredQuestionsInQuiz;
        public int nCorrectlyAnsweredQuestionsInQuiz;

        public int nSons;

        public KnowledgeLevel level;
        public int nDescendentsWithBadKnowledgeLevel;
        public int nDescendentsWithUnknownKnowledgeLevel;
        public int nDescendentsWithGoodKnowledgeLevel;

        public TreeReport father;
        public List<TreeReport> sons;

        public List<TreeReport> selectedForReport;

        
        /// <summary>
        /// Preenche a lista selectedForReport com os nodes que devem aparecer no plano de estudos.
        /// </summary>
        public void selectNodesForReport()
        {
            TreeReport reportAuxiliar = this;
            selectedForReport = new List<TreeReport>();
            if((reportAuxiliar.nDescendentsWithBadKnowledgeLevel>0)&&(reportAuxiliar.nDescendentsWithUnknownKnowledgeLevel==0)&&(reportAuxiliar.nDescendentsWithGoodKnowledgeLevel==0))
            {
                selectedForReport.Add(reportAuxiliar);
            }
            else if((reportAuxiliar.nDescendentsWithBadKnowledgeLevel==0)&&(reportAuxiliar.nDescendentsWithUnknownKnowledgeLevel>0)&&(reportAuxiliar.nDescendentsWithGoodKnowledgeLevel==0))
            {
                selectedForReport.Add(reportAuxiliar);
            }
            else if((reportAuxiliar.nDescendentsWithBadKnowledgeLevel==0)&&(reportAuxiliar.nDescendentsWithUnknownKnowledgeLevel==0)&&(reportAuxiliar.nDescendentsWithGoodKnowledgeLevel>0))
            {
                selectLeavesOfTree(reportAuxiliar);
            }
            else
            {
                foreach(TreeReport t in reportAuxiliar.sons)
                {
                    selectNodesForReportLoop(t);
                }
            }
        }

        private void selectNodesForReportLoop(TreeReport treeReport)
        {
            if((treeReport.nDescendentsWithBadKnowledgeLevel>0)&&(treeReport.nDescendentsWithUnknownKnowledgeLevel==0)&&(treeReport.nDescendentsWithGoodKnowledgeLevel==0))
            {
                selectedForReport.Add(treeReport);
            }
            else if((treeReport.nDescendentsWithBadKnowledgeLevel==0)&&(treeReport.nDescendentsWithUnknownKnowledgeLevel>0)&&(treeReport.nDescendentsWithGoodKnowledgeLevel==0))
            {
                selectLeavesOfTree(treeReport);
            }
            else if((treeReport.nDescendentsWithBadKnowledgeLevel==0)&&(treeReport.nDescendentsWithUnknownKnowledgeLevel==0)&&(treeReport.nDescendentsWithGoodKnowledgeLevel>0))
            {
                selectLeavesOfTree(treeReport);
            }
            else
            {
                foreach(TreeReport t in treeReport.sons)
                {
                    selectNodesForReportLoop(t);
                }
            }
        }

        private void selectLeavesOfTree(TreeReport treeReport)
        {
            if (treeReport.nSons == 0)
            {
                selectedForReport.Add(treeReport);
            }
            else
            {
                foreach (TreeReport t in treeReport.sons)
                {
                    selectLeavesOfTree(t);
                }
            }
        }




        public TreeReport(DefaultConnectionEntities db, int nodeId, int quizId, TreeReport father)
        {
            nQuestions = 0;
            nCorrectlyAnsweredQuestions = 0;
            nNotCorrectlyAnswered = 0;
            nIncorrectlyAnsweredQuestionsInQuiz = 0;
            nCorrectlyAnsweredQuestionsInQuiz = 0;

            nDescendentsWithBadKnowledgeLevel = 0;
            nDescendentsWithUnknownKnowledgeLevel = 0;
            nDescendentsWithGoodKnowledgeLevel = 0;



            this.node = db.Node.Find(nodeId);
            this.quizId = quizId;
            this.sons = new List<TreeReport>();
            this.father = father;

            //Obtem o número de filhos do nó
            nSons = NodeService.FindSons(db, nodeId).Count;

            //Se for node folha
            if (nSons == 0)
            {
                //Obtem todas as questões do node
                List<Question> nodeQuestions = QuestionService.GetNodeQuestions(db, nodeId);

                //Remove todas asquestões inváidas da lista
                QuestionService.RemoveInvalidQuestions(db, nodeQuestions);

                //Obtem o número total de questões do node
                nQuestions = nodeQuestions.Count;

                foreach (Question question in nodeQuestions)
                {
                    QuestionStatus questionStatus = GetQuestionStatusForUser(db, question.Id, db.Quiz.Find(quizId).AspNetUsersId);
                    switch (questionStatus)
                    {
                        case QuestionStatus.AnsweredCorrectlyInQuiz:
                            nCorrectlyAnsweredQuestions++;
                            nCorrectlyAnsweredQuestionsInQuiz++;
                            break;

                        case QuestionStatus.AnsweredCorrectly:
                            nCorrectlyAnsweredQuestions++;
                            break;

                        case QuestionStatus.AnsweredIncorrectlyInQuiz:
                            nIncorrectlyAnsweredQuestionsInQuiz++;
                            nNotCorrectlyAnswered++;
                            break;

                        case QuestionStatus.NotAnsweredCorrectly:
                             nNotCorrectlyAnswered++;
                            break;

                    }
                }

                //Obtem o nível de conhecimento da folha para o usuário no quiz
                NodeInfo ni = new NodeInfo(nodeId, quizId, null, false);
                this.level = ni.level;

                if(ni.level.name.Equals("bad"))
                {
                    nDescendentsWithBadKnowledgeLevel++;
                }
                if(ni.level.name.Equals("unknown"))
                {
                    nDescendentsWithUnknownKnowledgeLevel++;
                }
                if(ni.level.name.Equals("good"))
                {
                    nDescendentsWithGoodKnowledgeLevel++;
                }
                if(ni.level.name.Equals("veryGood"))
                {
                    nDescendentsWithGoodKnowledgeLevel++;
                }

                //Propaga em cascata as informações da folha para os antepassados
                TreeReport loopAuxiliar = this;
                loopAuxiliar.father.nQuestions += loopAuxiliar.nQuestions;

                int auxiliarNQuestions = loopAuxiliar.nQuestions;
                int auxiliarNNotCorrectlyAnswered = loopAuxiliar.nNotCorrectlyAnswered;
                int auxiliarNCorrectlyAnsweredQuestions = loopAuxiliar.nCorrectlyAnsweredQuestions;
                int auxiliarNIncorrectlyAnsweredQuestionsInQuiz = loopAuxiliar.nIncorrectlyAnsweredQuestionsInQuiz;
                int auxiliarNCorrectlyAnsweredQuestionsInQuiz = loopAuxiliar.nCorrectlyAnsweredQuestionsInQuiz;
                
                int auxiliarNDescendentsWithBadKnowledgeLevel = loopAuxiliar.nDescendentsWithBadKnowledgeLevel;
                int auxiliarNDescendentsWithUnknownKnowledgeLevel = loopAuxiliar.nDescendentsWithUnknownKnowledgeLevel;
                int auxiliarNDescendentsWithGoodKnowledgeLevel = loopAuxiliar.nDescendentsWithGoodKnowledgeLevel;

                while(loopAuxiliar.father!=null)
                {
                    loopAuxiliar.father.nQuestions += auxiliarNQuestions;

                    loopAuxiliar.father.nNotCorrectlyAnswered += auxiliarNNotCorrectlyAnswered;
                    loopAuxiliar.father.nCorrectlyAnsweredQuestions += auxiliarNCorrectlyAnsweredQuestions;
                    loopAuxiliar.father.nIncorrectlyAnsweredQuestionsInQuiz += auxiliarNIncorrectlyAnsweredQuestionsInQuiz;
                    loopAuxiliar.father.nCorrectlyAnsweredQuestionsInQuiz += auxiliarNCorrectlyAnsweredQuestionsInQuiz;

                    loopAuxiliar.father.nDescendentsWithBadKnowledgeLevel += auxiliarNDescendentsWithBadKnowledgeLevel;
                    loopAuxiliar.father.nDescendentsWithUnknownKnowledgeLevel += auxiliarNDescendentsWithUnknownKnowledgeLevel;
                    loopAuxiliar.father.nDescendentsWithGoodKnowledgeLevel += auxiliarNDescendentsWithGoodKnowledgeLevel;

                    loopAuxiliar = loopAuxiliar.father;
                }
            }


        }

        private QuestionStatus GetQuestionStatusForUser(DefaultConnectionEntities db, int questionId, string userId)
        {
            if (QuestionService.WasQuestionEverAnsweredCorrectlyByUser(db, questionId, userId))
            {
                if (QuestionService.WasQuestionAnsweredCorrectlyByUserInQuiz(db, questionId, quizId, userId))
                {
                    return QuestionStatus.AnsweredCorrectlyInQuiz;
                }
                return QuestionStatus.AnsweredCorrectly;
            }
            else
            {
                if (QuestionService.WasQuestionAnsweredIncorrectlyByUserInQuiz(db, questionId, quizId, userId))
                {
                    return QuestionStatus.AnsweredIncorrectlyInQuiz;
                }
                return QuestionStatus.NotAnsweredCorrectly;
            }
        }

        public int getPercentCorrectlyAnswered()
        {
            if (nQuestions== 0)
            {
                return 0;
            }
            return (100 * nCorrectlyAnsweredQuestions) / (nQuestions);
        }

        //Categoria 1 - Nodes raiz de árvores ou subárvores do conhecimento em que ó domínio do usuário é ruim.
        public List<TreeReport> getSortedCategory1Nodes()
        {
            List<TreeReport> list = new List<TreeReport>();

            //Se não foi inicializada a lista 
            if (selectedForReport == null)
            {
                selectNodesForReport();
            }

            //Obtem todos os nodes selecionados para o plano de estudos em que todos os descendentes possuem nivel de conhecimento ruim
            foreach (TreeReport n in selectedForReport)
            { 
                if(n.nDescendentsWithBadKnowledgeLevel>0)
                {
                    list.Add(n);
                }
            }

            //Remove todos os nodes de categoria 5(inválidos) da lista
            foreach (TreeReport t in this.getSortedCategory5Nodes())
            {
                list.Remove(t);
            }
            //Remove todos os nodes de categoria 4(completados) da lista
            foreach (TreeReport t in this.getSortedCategory4Nodes())
            {
                list.Remove(t);
            }

            list.Sort(
                 delegate(TreeReport p1, TreeReport p2)
                 {
                     var val1 = p1.getPercentCorrectlyAnswered();
                     var val2 = p2.getPercentCorrectlyAnswered();
                     if (val1 == val2)
                     {
                         return 0;
                     }
                     //Se val1 for maior que val2, significa que val1 tem uma porcentagem maior de perguntas acertadas que erradas em relação a val2 e , portanto, deve ser precedido por val 2 
                     else if (val1 > val2)
                     {
                         return -1;
                     }
                     //p1 tem maius perguntas erradas que p2, e deve vir antes na lista
                     else
                     {
                         return 1;
                     }
                 }
            );
            return list;

        }
        //Categoria 2 -  Nodes raiz de árvores ou subárvores do conhecimento em que ó domínio do usuário é desconhecido.
        public List<TreeReport> getSortedCategory2Nodes()
        {
            List<TreeReport> list = new List<TreeReport>();

            //Se não foi inicializada a lista 
            if (selectedForReport == null)
            {
                selectNodesForReport();
            }

            //Obtem todos os nodes selecionados para o plano de estudos em que todos os descendentes possuem nivel de conhecimento desconhecido
            foreach (TreeReport n in selectedForReport)
            {
                if (n.nDescendentsWithUnknownKnowledgeLevel > 0)
                {
                    list.Add(n);
                }
            }

            //Remove todos os nodes de categoria 5(inválidos) da lista
            foreach (TreeReport t in this.getSortedCategory5Nodes())
            {
                list.Remove(t);
            }
            //Remove todos os nodes de categoria 4(completados) da lista
            foreach (TreeReport t in this.getSortedCategory4Nodes())
            {
                list.Remove(t);
            }

            list.Sort(
                 delegate(TreeReport p1, TreeReport p2)
                 {
                     var val1 = p1.getPercentCorrectlyAnswered();
                     var val2 = p2.getPercentCorrectlyAnswered();
                     if (val1 == val2)
                     {
                         return 0;
                     }
                     //Se val1 for maior que val2, significa que val1 tem uma porcentagem maior de perguntas acertadas que erradas em relação a val2 e , portanto, deve ser precedido por val 2 
                     else if (val1 > val2)
                     {
                         return -1;
                     }
                     //p1 tem maius perguntas erradas que p2, e deve vir antes na lista
                     else
                     {
                         return 1;
                     }
                 }
            );
            return list;
        }
        //Categoria 3 - Nodes folha de árvores do conhecimento em que ó domínio do usuário é bom.
        public List<TreeReport> getSortedCategory3Nodes()
        {
            List<TreeReport> list = new List<TreeReport>();

            //Se não foi inicializada a lista 
            if (selectedForReport == null)
            {
                selectNodesForReport();
            }

            //Obtem todos os nodes selecionados para o plano de estudos que possuem nivel de conhecimento bom
            foreach (TreeReport n in selectedForReport)
            {
                if (n.nDescendentsWithGoodKnowledgeLevel > 0)
                {
                    list.Add(n);
                }
            }

            //Remove todos os nodes de categoria 5(inválidos) da lista
            foreach (TreeReport t in this.getSortedCategory5Nodes())
            {
                list.Remove(t);
            }
            //Remove todos os nodes de categoria 4(completados) da lista
            foreach (TreeReport t in this.getSortedCategory4Nodes())
            {
                list.Remove(t);
            }

            list.Sort(
                 delegate(TreeReport p1, TreeReport p2)
                 {
                     var val1 = p1.getPercentCorrectlyAnswered();
                     var val2 = p2.getPercentCorrectlyAnswered();
                     if (val1 == val2)
                     {
                         return 0;
                     }
                     //Se val1 for maior que val2, significa que val1 tem uma porcentagem maior de perguntas acertadas que erradas em relação a val2 e , portanto, deve ser precedido por val 2 
                     else if (val1 > val2)
                     {
                         return -1;
                     }
                     //p1 tem maius perguntas erradas que p2, e deve vir antes na lista
                     else
                     {
                         return 1;
                     }
                 }
            );
            return list;
        }
        //Categoria 4 - Nodes completados
        public List<TreeReport> getSortedCategory4Nodes()
        {
            List<TreeReport> list = new List<TreeReport>();
            getTreesOfSons(ref list, this);

            list.RemoveAll(IsCategory4);

            //Remove todos os nodes de categoria 5(inválidos) da lista
            foreach (TreeReport t in this.getSortedCategory5Nodes())
            {
                list.Remove(t);
            }
            return list;
        }
        //Categoria 5 - Nodes sem perguntas válidas
        public List<TreeReport> getSortedCategory5Nodes()
        {

            List<TreeReport> list = new List<TreeReport>();
            getTreesOfSons(ref list, this);
            list.RemoveAll(IsCategory5);

            return list;

        }

        private void getTreesOfSons(ref List<TreeReport> list, TreeReport tree)
        {
            list.Add(tree);
            foreach (TreeReport t in tree.sons)
            {
               
                getTreesOfSons(ref list, t);
            }
        }


        private static bool IsCategory4(TreeReport t)
        {
            //Se todas as questões tverem sido respondidas
            if (((t.nCorrectlyAnsweredQuestions) == (t.nQuestions)))
            {
                return false;//nao remove
            }
            return true;//remove
        }
        private static bool IsCategory5(TreeReport t)
        {

            if (t.nQuestions == 0)
            {
                return false;
            }

            return true;
        }
    }
}