using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PF2.Services;
namespace PF2.Models
{
    public class AskQuestionPageData
    {
        public Question question;
        public List<AnswerOption> options;
        public int chosen { get; set; }
        public AskQuestionPageData() { }

        public AskQuestionPageData(Question q)
        {
            using (var db = new DefaultConnectionEntities())
            {
                question = q;
                options = new List<AnswerOption>();

                List<AnswerOption> answerOptions = AnswerOptionService.FindAnswerOptionsOfQuestion(db,question.Id);
                foreach (AnswerOption a in answerOptions)
                {
                    options.Add(a);
                }
            }
        
        }
    }
}