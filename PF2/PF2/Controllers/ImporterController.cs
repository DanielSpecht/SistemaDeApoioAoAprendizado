using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PF2.Models;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text;
using System.Globalization;
namespace PF2.Controllers
{
    public class ImporterController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

        // GET: Importer
        public ActionResult Path()
        {
            ImportPath p = new ImportPath();
            return View(p);

        }


        // GET: Importer
        [HttpPost]
        public ActionResult Path(ImportPath importPath)
        {
            ImportPath p = new ImportPath();
            String path = importPath.Path;
 


           if (Directory.Exists(path))
            {
                ProcessDirectory(path,null);
            }
            else
            {
                return View(p);
            }

            return View(p);

        }

        // Process all files in the directory passed in, recurse on any directories  
        // that are found, and process the files they contain. 
        public  void ProcessDirectory(string targetDirectory, int? idFather)
        {
            //Cria o Node
            Node node = new Node();
            string nodeName = System.IO.Path.GetFileName(targetDirectory);

            node.ParentId = idFather;
            node.Name = nodeName;
                db.Node.Add(node);
                db.SaveChanges();

            // Process the list of files found in the directory. 
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                ProcessFile(fileName,node.Id);
            }
            // Recurse into subdirectories of this directory. 
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                ProcessDirectory(subdirectory,node.Id);
            }
        }


        //http://www.dotnetperls.com/xmlreader
        public void ProcessFile(string path, int nodeId)
        {
            StreamReader streamReader = new StreamReader(path,Encoding.GetEncoding(CultureInfo.GetCultureInfo("pt-BR").TextInfo.ANSICodePage));
            string text = streamReader.ReadToEnd();

            //for (int cont = 0; cont > (text.Length - 1); cont++)
            //{
            //    if (text[cont].Equals("<"))
            //    { 
            //        text.in

            //    }
            //}
            //streamReader.Close();
            using (XmlReader reader = XmlReader.Create(new StringReader(text)))
            {
                int? questionId = null;

                while (reader.Read())
                {
                    // Only detect start elements.
                    if (reader.IsStartElement())
                    {
                        // Get element name and switch on it.
                        switch (reader.Name)
                        {
                            case "questions":
                                break;
                            case "notCorrectAnswer":
                                if (reader.Read())
                                {
                                    AnswerOption answerQuestion = new AnswerOption();
                                    answerQuestion.IsCorrect = false;
                                    answerQuestion.Answer = reader.Value;
                                    answerQuestion.QuestionId = (int)questionId;
                                    db.AnswerOption.Add(answerQuestion);
                                    db.SaveChanges();
                                }
                                break;
                            case "correctAnswer":
                                if (reader.Read())
                                {
                                    AnswerOption answerQuestion = new AnswerOption();
                                    answerQuestion.IsCorrect = true;
                                    answerQuestion.Answer = reader.Value;
                                    answerQuestion.QuestionId = (int)questionId;
                                    db.AnswerOption.Add(answerQuestion);
                                    db.SaveChanges();
                                }
                                break;
                            case "question":
                                if (reader.Read())
                                {
                                    Question question = new Question();
                                    question.Enunciation = reader.Value;
                                    question.NodeId = nodeId;
                                    db.Question.Add(question);
                                    db.SaveChanges();
                                    questionId = question.Id;
                                }
                                break;
                        }
                    }
                }
            }
        }


    }
}