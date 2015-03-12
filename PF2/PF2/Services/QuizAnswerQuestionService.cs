using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PF2.Models;
	
namespace PF2.Services
{
    public sealed class QuizAnswerQuestionService
    {
        private static readonly QuizAnswerQuestionService instance = new QuizAnswerQuestionService();
        private QuizAnswerQuestionService() { }
        public static QuizAnswerQuestionService Instance
        {
            get
            {
                return instance;
            }
        }

        public static void CreateIfInexistent(DefaultConnectionEntities db, int questionId, int quizId)
        {
            //Verifica se o QuizAnswerQuestion já foi inicializado
            QuizAnswerQuestion quizAnswer = db.QuizAnswerQuestion.Find(quizId, questionId);

            //Se não foi, inicializa
            if (quizAnswer == null)
            {
                QuizAnswerQuestion newQuizAnswerQuestion = new QuizAnswerQuestion();
                newQuizAnswerQuestion.QuizId = quizId;
                newQuizAnswerQuestion.QuestionId = questionId;
                db.QuizAnswerQuestion.Add(newQuizAnswerQuestion);
                db.SaveChanges();
            }
        }

        public static void DeleteQuizAnswerQuestion(DefaultConnectionEntities db, int questionId, int quizId)
        {
            QuizAnswerQuestion quizAnswer = db.QuizAnswerQuestion.Find(quizId, questionId);
            db.QuizAnswerQuestion.Remove(quizAnswer);
            db.SaveChanges();
        }

        public static List<QuizAnswerQuestion> FindAllCorrectAnswersOfUserForQuestion(DefaultConnectionEntities db, int questionId, String userId)
        {

           return (from answersOfUser in db.QuizAnswerQuestion
             where ((answersOfUser.AnswerOption.IsCorrect == true) && (answersOfUser.Quiz.AspNetUsersId.Equals(userId)) && (answersOfUser.QuestionId == questionId))
             select answersOfUser).ToList();
        }

        public static List<QuizAnswerQuestion> FindAllCorrectAnswersOfUserForQuestionInQuiz(DefaultConnectionEntities db, int questionId,int quizId, String userId)
        {

            return (from answersOfUser in db.QuizAnswerQuestion
                    where ((answersOfUser.AnswerOption.IsCorrect == true) && (answersOfUser.Quiz.AspNetUsersId.Equals(userId)) && (answersOfUser.QuestionId == questionId) && (answersOfUser.QuizId == quizId))
                    select answersOfUser).ToList();
        }

        public static List<QuizAnswerQuestion> FindAllIncorrectAnswersOfUserForQuestionInQuiz(DefaultConnectionEntities db, int questionId, int quizId, String userId)
        {

            return (from answersOfUser in db.QuizAnswerQuestion
                    where ((answersOfUser.AnswerOption.IsCorrect == false) && (answersOfUser.Quiz.AspNetUsersId.Equals(userId)) && (answersOfUser.QuestionId == questionId) && (answersOfUser.QuizId == quizId))
                    select answersOfUser).ToList();
            
        }
    }
}