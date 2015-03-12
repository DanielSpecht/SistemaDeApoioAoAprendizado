using PF2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PF2.Services
{
    public sealed class AnswerOptionService
    {
        private static readonly AnswerOptionService instance = new AnswerOptionService();
        private AnswerOptionService() { }
        public static AnswerOptionService Instance
        {
            get
            {
                return instance;
            }
        }



        public static void DeleteAnswerOption(DefaultConnectionEntities db, int answerOptionId)
        {
            AnswerOption answerOption = db.AnswerOption.Find(answerOptionId);
            db.AnswerOption.Remove(answerOption);
            db.SaveChanges();
        }
        public static List<AnswerOption> FindAnswerOptionsOfQuestion(DefaultConnectionEntities db, int questionId)
        {
            return (from answers in db.AnswerOption
                    where (answers.QuestionId == questionId)
                    select answers).ToList();

        }


    }
}