using PF2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PF2.Services
{
    public sealed class QuestionService
    {
        private static readonly QuestionService instance = new QuestionService();
        private QuestionService() { }
        public static QuestionService Instance
        {
            get
            {
                return instance;
            }
        }

        public static List<Question> GetNodeQuestions(DefaultConnectionEntities db, int nodeId)
        {
            return 
                (from q in db.Question
             where (q.NodeId == nodeId)
             select q).ToList();
        }

        public static void RemoveInvalidQuestions(DefaultConnectionEntities db, List<Question> questions)
        {
            for (int i = questions.Count - 1; i >= 0; i--)
            {
                if (!CheckIfQuestionIsValid(questions[i]))
                {
                    questions.RemoveAt(i);
                }
            }
        }

        public static bool CheckIfQuestionIsValid(Question q)
        {
            

            //Verifica se a pergunta possui pelo menos 2 opções de resposta
            if (q.Answer.Count < 2)
            {
                return false;
            }

            //Verifica se a pergunta possui exatamente uma opção de resposta
            int NCorrectOptions = 0;
            foreach (AnswerOption ao in q.Answer)
            { 
                if(ao.IsCorrect)
                {
                    NCorrectOptions++;
                }
            }
            if (NCorrectOptions != 1)
            {
                return false;
            }

            return true;
        }

        public static void DeleteQuestion(DefaultConnectionEntities db, int questionId)
        {
            Question question = db.Question.Find(questionId);
  
            //Deleta todas as opções de resposta
            for (int i = question.Answer.Count - 1; i >= 0; i--)
            {
                AnswerOption a = question.Answer.ElementAt(i);
                AnswerOptionService.DeleteAnswerOption(db,a.Id);
            }
            db.Question.Remove(question);
            db.SaveChanges();
        }

        public static bool WasQuestionEverAnsweredCorrectlyByUser(DefaultConnectionEntities db, int questionId, String userId)
        {
            List<QuizAnswerQuestion> correctAnswersOfUserForQuestion = QuizAnswerQuestionService.FindAllCorrectAnswersOfUserForQuestion(db, questionId, userId);
            //Se existem respostas em que o usuário acertou a questão 
            if (correctAnswersOfUserForQuestion.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool WasQuestionAnsweredCorrectlyByUserInQuiz(DefaultConnectionEntities db, int questionId,int quizId, String userId)
        {
            List<QuizAnswerQuestion> correctAnswersOfUserForQuestionInQuiz = QuizAnswerQuestionService.FindAllCorrectAnswersOfUserForQuestionInQuiz(db, questionId, quizId, userId);
            //Se existem respostas em que o usuário acertou a questão no quiz
            if (correctAnswersOfUserForQuestionInQuiz.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool WasQuestionAnsweredIncorrectlyByUserInQuiz(DefaultConnectionEntities db, int questionId, int quizId, String userId)
        {
            List<QuizAnswerQuestion> incorrectAnswersOfUserForQuestionInQuiz = QuizAnswerQuestionService.FindAllIncorrectAnswersOfUserForQuestionInQuiz(db, questionId, quizId, userId);
            //Se existem respostas em que o usuário acertou a questão no quiz
            if (incorrectAnswersOfUserForQuestionInQuiz.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}