using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PF2.Models;
using PF2.Services;

namespace PF2.Services
{
    public sealed class QuizService
    {
        private static readonly QuizService instance = new QuizService();
        private QuizService() { }
        public static QuizService Instance
        {
            get
            {
                return instance;
            }
        }

        public static void DeleteQuiz(DefaultConnectionEntities db, int quizId)
        {
            Quiz quiz = db.Quiz.Find(quizId);

            //Remove todos os QuizAnswerQuestion associados
            for (int i = quiz.QuizAnswerQuestion.Count - 1; i >= 0; i--)
            {
                QuizAnswerQuestion n = quiz.QuizAnswerQuestion.ElementAt(i);
                QuizAnswerQuestionService.DeleteQuizAnswerQuestion(db, n.QuestionId, n.QuizId);
            }

            db.Quiz.Remove(quiz);
            db.SaveChanges();
        }

        public static List<Quiz> GetAllPreviousQuizzesOfUserWithNode(DefaultConnectionEntities db, int nodeId, String userId)
        {
            return (from quiz in db.Quiz
                    orderby quiz.EndTime descending
                    where ((quiz.AspNetUsersId.Equals(userId)) && (NodeService.IsNodeInTree(db, nodeId, quiz.NodeId)))
                    select quiz).ToList();

        }

        public static Quiz MostRecentFinishedQuizOfUserWithNode(DefaultConnectionEntities db, int nodeId,String userId)
        {
            //Obtem uma lista de quizzes do usuário onde o nó estava incluso na árvore
            List<Quiz> quizzes = (from quiz in db.Quiz
                                  orderby quiz.EndTime descending
                                  where ((quiz.AspNetUsersId.Equals(userId))&&(quiz.EndTime!=null))
                                  select quiz).ToList();

            for (int i = quizzes.Count - 1; i >= 0; i--)
            {
                Quiz q = quizzes.ElementAt(i);
                if (!NodeService.IsNodeInTree(db, nodeId, q.NodeId))
                {
                    quizzes.RemoveAt(i);
                }
            }

            //Retorna o primeiro elemento se existe
            if (quizzes.Count > 0)
            {
                return quizzes[0];
            }
            else
            { 
                return null;
            }
        }
    }
}