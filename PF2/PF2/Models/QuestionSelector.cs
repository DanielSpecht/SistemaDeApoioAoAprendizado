using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using PF2.Services;

namespace PF2.Models
{
    public enum Status { InProgress, NQuestionsAnswered, NoMoreValidNodes };

    public struct Interval
    {
        public int? from;
        public int? to;
    }
    public class KnowledgeLevel
    {
        public String name;
        public int weight;
        public Interval interval;
    }

    public sealed class KnowledgeLevels 
    {
        private static readonly KnowledgeLevels instance = new KnowledgeLevels();
        private KnowledgeLevels() { Initializelevels(); }
        public static KnowledgeLevels Instance
        {
            get
            {
                return instance;
            }
        }

        public static List<KnowledgeLevel> levels;
        private void Initializelevels()
        {
            levels = new List<KnowledgeLevel>();

            KnowledgeLevel badLastQuiz = new KnowledgeLevel();
            badLastQuiz.name = "badLastQuiz";
            badLastQuiz.weight = 6;

            KnowledgeLevel bad = new KnowledgeLevel();
            Interval badInterval = new Interval();
            badInterval.to = -2;
            bad.interval = badInterval;
            bad.name = "bad";
            bad.weight = 0;

            KnowledgeLevel unknown = new KnowledgeLevel();
            Interval unknownInterval = new Interval();
            unknownInterval.from = badInterval.to + 1;
            unknownInterval.to = 0;//teste 9
            unknown.interval = unknownInterval;
            unknown.name = "unknown";
            unknown.weight = 4;

            KnowledgeLevel good = new KnowledgeLevel();
            Interval goodInterval = new Interval();
            goodInterval.from = unknown.interval.to + 1;
            goodInterval.to = 1;//teste 19
            good.interval = goodInterval;
            good.name = "good";
            good.weight = 2;

            KnowledgeLevel veryGood = new KnowledgeLevel();
            Interval veryGoodInterval = new Interval();
            veryGoodInterval.from = good.interval.to + 1;
            veryGoodInterval.to = null;

            veryGood.interval = veryGoodInterval;
            veryGood.name = "veryGood";
            veryGood.weight = 1;

            levels.Add(badLastQuiz);
            levels.Add(bad);
            levels.Add(unknown);
            levels.Add(good);
            levels.Add(veryGood);
        }
    }


    enum QuestionStatus { AnsweredCorrectly = 1, NotAnsweredCorrectly = 2, AnsweredCorrectlyInQuiz=3, AnsweredIncorrectlyInQuiz=4 };
    public  enum AvailabilityForRound {Avaliable, BadKnowledgeLevel, AllQuestionsCorrectlyAnswered, NoNewQuestionAvaliable, NoQuestionsRegistered }
    
    public class QuestionSelector
    {       
        public Quiz quiz;
        public List<NodeInfo> nodesInfo;
        int totalAnsweredQuestions;
        public List<int> Round;
        public Status status;

        public Log log;


        /// <summary>
/// Construtor da classe QuestionSelector
/// </summary>
/// <param name="q"> O quiz do qual serão selecionadas as questões  </param>
 
 
        public QuestionSelector(Quiz q)
        {
            quiz = q;
            nodesInfo = new List<NodeInfo>();
            log = new Log();

            GetLeavesInfos(quiz.NodeId);
            updateStatus();
            PrepareRound();
        }

   
        private void GetLeavesInfos(int nodeId)
        {
            using (var db = new DefaultConnectionEntities())
            {
                //todos os node filho
                List<Node> resultado = NodeService.FindSons(db, nodeId);

                if (resultado.Count == 0)
                {//nodeId é o id de um node folha
                    NodeInfo nodeInfo = new NodeInfo(nodeId, quiz.Id, log,true);
                    nodesInfo.Add(nodeInfo);
                }
                else
                {//não é folha
                    foreach (Node n in resultado)
                    {
                            GetLeavesInfos(n.Id);
                    }
                }
            }
        }
  
        public void PrepareRound()
        {
            //Se o round não estiver inicializado como lista
            if (Round == null)
            {
                Round = new List<int>();
            }

            //Para cada nodeFolha da árvore/subárvore
            foreach (NodeInfo nodeInfo in nodesInfo)
            {
                //Se o node deve ser incluído no round
                if (decideIfNodeShouldBeInRound(nodeInfo))
                {
                    //Insere na rodada um número de perguntas do node ígual ao seu peso
                    for (int i = 0; i < nodeInfo.level.weight; i++)
                    {
                        Round.Add(nodeInfo.nodeId);
                    }
                }

                //Randomiza as perguntas do round
                Random rng = new Random();
                int n = Round.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    int value = Round[k];
                    Round[k] = Round[n];
                    Round[n] = value;
                }

            }
        }

        public Question getNewQuestion()
        {
            using (var db = new DefaultConnectionEntities())
            {
                if (Round.Count == 0)
                {
                    return null;
                }
                int nodeId = Round[0];
                Round.RemoveAt(0);
                

                foreach (NodeInfo n in nodesInfo)
                {
                    if (n.nodeId == nodeId)
                    {
                        Question question = db.Question.Find(n.questionsNotCorrectlyAnswered[0]);
                        if (question == null)
                        {
                            return null;
                        }
                        else
                        {
                            return question;
                        }
                    }
                
                }

            }
            return null;
        }

        public void sendQuestionAnswer(int answerOptionId)
        {
            using (var db = new DefaultConnectionEntities())
            {
                AnswerOption a =  db.AnswerOption.Find(answerOptionId);
                Question q = a.Question;
                totalAnsweredQuestions++;

                //Cria um registro de QuizAnswerQuestion
                QuizAnswerQuestionService.CreateIfInexistent(db, q.Id, quiz.Id);

                //Atualiza o registro de QuizAnswerQuestion apropriado
                QuizAnswerQuestion answer = db.QuizAnswerQuestion.Find(quiz.Id,q.Id);
                answer.AnswerOptionId = a.Id;
                db.Entry(answer).State = EntityState.Modified;
                db.SaveChanges();

                foreach (NodeInfo nodeInfo in nodesInfo)
                {                
                    //Encontra o nodeInfo da última questão respondida
                    if (nodeInfo.nodeId == q.NodeId)
                    {
                        //Faz com que o NodeInfo Realize as alterações necessárias
                        nodeInfo.AnswerQuestion(db, answerOptionId);

                        //Se o node não possúi os requerimentos necessários para ficar no round é renovido do mesmo
                        if (!decideIfNodeShouldBeInRound(nodeInfo))
                        {
                            while (Round.Remove(nodeInfo.nodeId))
                            { }
                        }

                        //Verifica se o round acabou
                        if (Round.Count == 0)
                        {
                            this.PrepareRound();
                        }

                        //Verifica se houve alguma alteração no status do andamento do quiz
                        updateStatus();

                        break;
                    }
                }
                
            }
        }
        public bool decideIfNodeShouldBeInRound(NodeInfo n)
        {
            if (n.level.name.Equals("bad"))
            {
                //while (Round.Remove(n.nodeId)) ;
                return false;
            }
            if (n.nQuestions == n.nCorrectlyAnsweredQuestions)
            {
                //while (Round.Remove(n.nodeId)) ;
                return false;
            }
            if (n.nQuestions == 0)
            {
                //while (Round.Remove(n.nodeId)) ;
                return false;
            }
            return true;
        }

        public void updateStatus()
        {
            int cont = 0;//conta quantos nodes não podem entrar no round por motivo de não possuir perguntas disponíveis
            //int flag = 1;//indica se o motivo do término é porque respondeu todas as perguntas corretamente(1) ou porque respondeu todas as perguntas(0)
            if (totalAnsweredQuestions == quiz.NumQuestions)
            {
                status = Status.NQuestionsAnswered;
                return;
            }
            foreach (NodeInfo n in nodesInfo)
            {
                //Se já respondeu corretamente todas as perguntas disponíveis, não é mais válido
                if (n.nQuestions == n.nCorrectlyAnsweredQuestions)
                {
                    cont++;
                }
                //Se já respondeu todas as perguntas disponíveis independente de ter acertado
                else if (n.nQuestions == n.nCorrectlyAnsweredQuestions + n.nIncorrectlyAnsweredQuestionsInQuiz)
                {
                    cont++;
                    //flag = 0;
                }
                //Se não possúi nenhuma pergunta para ser respondida não é válido
                else if (n.nQuestions == 0)
                {
                    cont++;
                }
                    //Se o nível de conhecimento do usuário para o node se mostrou ruim durante o quiz
                else if (n.level.name.Equals("bad"))
                {
                    cont++;
                }
            }

            //Se não existem mais nodes válidos para a obtenção de perguntas, atualiza o status para tal
            if (cont == nodesInfo.Count)
            {
                status = Status.NoMoreValidNodes;
                return;
            }
            status = Status.InProgress;
        }
    }

    public class NodeInfo
    {
        //Id do node o qual o nodeInfo mantem informações
        public int nodeId;

        //Nome do node, para exibição na interface
        public String nodeName;

        //Quiz associado 
        public int quizId;

        //Numero total de questões válidas registradas para o node
        public int nQuestions;

        //Numero total de questões respondidas corretamente para o node, independente do quiz
        public int nCorrectlyAnsweredQuestions;
        //Numero total de questões não respondidas corretamente para o node, independente do quiz
        public int nNotCorrectlyAnsweredQuestions;
        //Numero total de questões respondidas corretamente para o node no quiz em questão
        public int nCorrectlyAnsweredQuestionsInQuiz;
        //Numero total de questões não respondidas corretamente para o node no quiz em questão
        public int nIncorrectlyAnsweredQuestionsInQuiz;

        //Lista de questões não corretamente respondidas
        public List<int> questionsNotCorrectlyAnswered;

        //Nível de conhecimento do usuário para a pergunta
        public KnowledgeLevel level;

        //Qual a disponibilidade do node de entrar no round
        public AvailabilityForRound availability;

        //O objeto log, utilizado para se fazer o acompanhamento da execução do algoritmo
        public Log log;

        //Indica se testa o knowledgeLevel do node BadLastQuiz e se cria entradas no log
        private bool flag;

        /// <summary>
        /// Construtor de NodeInfo
        /// </summary>
        /// <param name="nodeId">Id do node</param>
        /// <param name="quizId">Id do quiz</param>
        /// <param name="knowledgeLevels">Lista de knowledgeLevel para ser utilizado para classificar os nodes</param>
       public NodeInfo(int nodeId, int quizId, Log log,bool flag)
       {
           using (var db = new DefaultConnectionEntities())
            {
            this.questionsNotCorrectlyAnswered = new List<int>();
            this.nodeId = nodeId;
            this.quizId = quizId;
            this.nodeName = db.Node.Find(nodeId).Name;
            this.log = log;
            this.flag = flag;
            AtualizeInfos(db);
            }
        }
        private void AtualizeInfos(DefaultConnectionEntities db)
        {

            nCorrectlyAnsweredQuestions=0;
            nNotCorrectlyAnsweredQuestions = 0;
            nCorrectlyAnsweredQuestionsInQuiz = 0;
            nIncorrectlyAnsweredQuestionsInQuiz = 0;


            questionsNotCorrectlyAnswered = new List<int>();

            //Obtem todas as questões do node
            List<Question> nodeQuestions = QuestionService.GetNodeQuestions(db, nodeId);

            //Remove todas asquestões inváidas da lista
            QuestionService.RemoveInvalidQuestions(db, nodeQuestions);
            
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
                        nNotCorrectlyAnsweredQuestions++;//OBS Mudei e não testei

                        break;

                    case QuestionStatus.NotAnsweredCorrectly:
                        questionsNotCorrectlyAnswered.Add(question.Id);
                        nNotCorrectlyAnsweredQuestions++;
                        break;

                }
            }

            CheckKnowledgeLevel(db, true);
            UpdateAvaliability(true);

        }

        /// <summary>
        /// Verifica a situação da pergunta para o usuário considerando o quiz atual
        /// </summary>
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


        public void AnswerQuestion(DefaultConnectionEntities db,int answerOptionId)
        {
            AnswerOption answerOption = db.AnswerOption.Find(answerOptionId);
            if (answerOption.IsCorrect == true)
            {
                nCorrectlyAnsweredQuestions++;
                nCorrectlyAnsweredQuestionsInQuiz++;
                nNotCorrectlyAnsweredQuestions--;
                if (flag)
                {
                    String entry1 = "Question correctly answered for node " + this.nodeName;
                    String entry2 = "CorrectlyAnsweredQuestions for node from" + (nCorrectlyAnsweredQuestions - 1).ToString() + " to " + nCorrectlyAnsweredQuestions.ToString();
                    String entry3 = "tCorrectlyAnsweredQuestions for node in quiz from" + (nCorrectlyAnsweredQuestionsInQuiz - 1).ToString() + " to " + nCorrectlyAnsweredQuestionsInQuiz.ToString();
                    String entry4 = "NotCorrectlyAnsweredQuestions for node from" + (nNotCorrectlyAnsweredQuestions + 1).ToString() + " to " + nNotCorrectlyAnsweredQuestions.ToString(); ;
                    
                    log.createEntry(entry1);
                    log.createEntry(entry2);
                    log.createEntry(entry3);
                    log.createEntry(entry4);
                    log.createEntry("horizontalLine");
                }
            }
            else
            {
                nIncorrectlyAnsweredQuestionsInQuiz++;

                if(flag)
                {
                    String entry1 = "Question incorrectly answered for node " + this.nodeName;
                    String entry2 = "IncorrectlyAnsweredQuestions for node in quiz from" + (nIncorrectlyAnsweredQuestionsInQuiz - 1).ToString() + " to " + nIncorrectlyAnsweredQuestionsInQuiz.ToString();
                    log.createEntry(entry1);
                    log.createEntry(entry2);
                    log.createEntry("horizontalLine");

                }


            }
            questionsNotCorrectlyAnswered.Remove(answerOption.QuestionId);
            CheckKnowledgeLevel(db,false);
            UpdateAvaliability(false);
        }

        public void CheckKnowledgeLevel(DefaultConnectionEntities db,bool isFirst)
        {
            //Armazena o KnowledgeLevel atual para checar se houve mudanças
            KnowledgeLevel auxiliaryKnowledgeLevel = this.level;
            
            foreach (KnowledgeLevel k in KnowledgeLevels.levels)
            {

                if (k.name.Equals("badLastQuiz"))
                {
                    if (flag)
                    {
                        //Obtem o id do usuário realizando o quiz
                        String userId = db.Quiz.Find(this.quizId).AspNetUsersId;

                        //Obtem o último quiz realizado pelo usuário em que aparece o node
                        Quiz quiz = QuizService.MostRecentFinishedQuizOfUserWithNode(db, nodeId, userId);

                        if (quiz != null)
                        {
                            int quizId = quiz.Id;
                            //Monta a estrutura NodeInfo considerando o último quiz realizado pelo usuário
                            NodeInfo infoLastQuiz = new NodeInfo(nodeId, (int)quizId, log, false);

                            //Se o desempenho do usuário no último quiz que continha o node foi ruim
                            if (infoLastQuiz.level.name.Equals("bad"))
                            {
                                this.level = k;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (k.name.Equals("bad"))
                    {
                        if ((nCorrectlyAnsweredQuestionsInQuiz-nIncorrectlyAnsweredQuestionsInQuiz) <= (k.interval.to))
                        {
                            this.level = k;
                            break;
                        }
                    }
                    if (k.interval.to == null)
                    {
                        if (nCorrectlyAnsweredQuestions >= k.interval.from)
                        {
                            this.level = k;
                            break;
                        }
                    }
                    if ((k.interval.to >= nCorrectlyAnsweredQuestions) && (nCorrectlyAnsweredQuestions >= k.interval.from))
                    {
                        this.level = k;
                        break;
                    }
                }
            }
                        
            //Se houve mudanças 
            //Checa se é a primeira vez que o knowledge level foi calculada, registra no log
            if (flag)
            {
                if (isFirst)
                {
                    String knowledgeLevelTo = this.level.name;
                    String entry = "Knowledge level of node " + this.nodeName + " set to " + knowledgeLevelTo;
                    log.createEntry(entry);
                    log.createEntry("horizontalLine");

                }
                //Se não for a primeira vez
                else
                {
                    if (auxiliaryKnowledgeLevel != this.level)
                    {
                        String knowledgeLevelFrom = auxiliaryKnowledgeLevel.name;
                        String knowledgeLevelTo = this.level.name;
                        String entry = "Knowledge level of node " + this.nodeName + " changed from " + knowledgeLevelFrom + " to " + knowledgeLevelTo;
                        log.createEntry(entry);
                        log.createEntry("horizontalLine");

                    }
                }
            }

        }

        public void UpdateAvaliability(bool isFirst)
        {
            //Salva a disponibilidade atual para checar se houve mudanças
            AvailabilityForRound auxiliaryAvailability = availability;
            availability = AvailabilityForRound.Avaliable;


            //A ordem aqui faz toda a diferença
            if (this.nQuestions == (this.nCorrectlyAnsweredQuestions + this.nIncorrectlyAnsweredQuestionsInQuiz))
            {
                availability = AvailabilityForRound.NoNewQuestionAvaliable;
            }
            if (this.nQuestions == this.nCorrectlyAnsweredQuestions)
            {
                availability = AvailabilityForRound.AllQuestionsCorrectlyAnswered;
            }
            if (this.nQuestions == 0)
            {
                availability = AvailabilityForRound.NoQuestionsRegistered;
            }

            if (this.level.name.Equals("bad"))
            {
                availability = AvailabilityForRound.BadKnowledgeLevel;
            }
            
            
            //Se houve mudanças 
            //Checa se é a primeira vez que a disponibilidade foi calculada, registra no log
            if (flag)
            {
                if (isFirst)
                {
                    String availabilityTo = "";

                    if (this.availability == AvailabilityForRound.Avaliable) { availabilityTo = "'Available'"; }
                    if (this.availability == AvailabilityForRound.BadKnowledgeLevel) { availabilityTo = "'BadKnowledgeLevel'"; }
                    if (this.availability == AvailabilityForRound.AllQuestionsCorrectlyAnswered) { availabilityTo = "'AllQuestionsCorrectlyAnswered'"; }
                    if (this.availability == AvailabilityForRound.NoNewQuestionAvaliable) { availabilityTo = "'NoNewQuestionAvaliable'"; }
                    if (this.availability == AvailabilityForRound.NoQuestionsRegistered) { availabilityTo = "'NoQuestionsRegistered'"; }

                    String entry = "Availability of node " + this.nodeName + " set to " + availabilityTo;
                    log.createEntry(entry);
                    log.createEntry("horizontalLine");
                }
                //Se não for a primeira vez
                else
                {
                    //Se a disponibiloidade mudou, registra no log
                    if (auxiliaryAvailability != this.availability)
                    {
                        String availabilityFrom = "";
                        String availabilityTo = "";


                        if (auxiliaryAvailability == AvailabilityForRound.Avaliable) { availabilityFrom = "'Available'"; }
                        if (auxiliaryAvailability == AvailabilityForRound.BadKnowledgeLevel) { availabilityFrom = "'BadKnowledgeLevel'"; }
                        if (auxiliaryAvailability == AvailabilityForRound.AllQuestionsCorrectlyAnswered) { availabilityFrom = "'AllQuestionsCorrectlyAnswered'"; }
                        if (auxiliaryAvailability == AvailabilityForRound.NoNewQuestionAvaliable) { availabilityFrom = "'NoNewQuestionAvaliable'"; }
                        if (auxiliaryAvailability == AvailabilityForRound.NoQuestionsRegistered) { availabilityFrom = "'NoQuestionsRegistered'"; }

                        if (this.availability == AvailabilityForRound.Avaliable) { availabilityTo = "'Available'"; }
                        if (this.availability == AvailabilityForRound.BadKnowledgeLevel) { availabilityTo = "'BadKnowledgeLevel'"; }
                        if (this.availability == AvailabilityForRound.AllQuestionsCorrectlyAnswered) { availabilityTo = "'AllQuestionsCorrectlyAnswered'"; }
                        if (this.availability == AvailabilityForRound.NoNewQuestionAvaliable) { availabilityTo = "'NoNewQuestionAvaliable'"; }
                        if (this.availability == AvailabilityForRound.NoQuestionsRegistered) { availabilityTo = "'NoQuestionsRegistered'"; }

                        String entry = "Availability of node " + this.nodeName + " changed from " + availabilityFrom + " to " + availabilityTo;
                        log.createEntry(entry);
                        log.createEntry("horizontalLine");

                    }
                }
            }
        }
    }

    public class Log 
    {
        public List<String> entrys;

        public Log()
        {
            this.entrys = new List<string>();
        }
        public void createEntry(String entry)
        {
            this.entrys.Add(entry);
        }

    }


}