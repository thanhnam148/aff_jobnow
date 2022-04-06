using Aff.DataAccess;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using Aff.Services.Services;
using Aff.WebApplication.Controllers;
using Aff.WebApplication.Messaging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Messaging;
using System.Threading;
using System.Web.Hosting;
using System.Web.Mvc;

[assembly: WebActivatorEx.PreApplicationStartMethod(
                typeof(TimerJob), "Start")]
namespace Aff.WebApplication.Messaging
{
    public static class TimerJob
    {
        private static readonly Timer _timer = new Timer(OnTimerElapsed);
        private static readonly AutoJob _jobHost = new AutoJob();
        private static MessageQueue mq;
        public static void Start()
        {
            ////Q Creation
            //if (MessageQueue.Exists(@".\Private$\MyQueue"))
            //    mq = new MessageQueue(@".\Private$\MyQueue");
            //else
            //    mq = MessageQueue.Create(@".\Private$\MyQueue");

            //_timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(5000));
        }

        private static void OnTimerElapsed(object sender)
        {
            _jobHost.DoWork(() =>
            {
                Message mes;
                try
                {
                    //ErrorLog(HostingEnvironment.ApplicationPhysicalPath, "Begin Receive Message");
                    mes = mq.Receive(new TimeSpan(0, 0, 2));
                    mes.Formatter = new XmlMessageFormatter(new String[] { "System.String,mscorlib" });
                    ErrorLog(HostingEnvironment.ApplicationPhysicalPath, "--->>content: " + mes.Label + "##" + mes.Body.ToString());
                    if (mes != null && mes.Body != null && !string.IsNullOrEmpty(mes.Label))
                    {
                        var transModel = JsonConvert.DeserializeObject<TransactionModel>(mes.Body.ToString().Trim());
                        TimaAffiliateEntities dbContext = new TimaAffiliateEntities();
                        string message = "";
                        if (mes.Label.ToLower().Contains("loan"))
                        {
                            var repo = new LoanRepository(dbContext);
                            repo.CreateLoanCredit(transModel, out message);
                        }
                        else if (mes.Label.ToLower().Contains("transaction"))
                        {
                            var repo = new TransactionRepository(dbContext);
                            var isSuccess = repo.CreateTransaction(transModel, out message);
                            ErrorLog(HostingEnvironment.ApplicationPhysicalPath, "Create transaction: " + isSuccess + "##Message: " + message);
                        }
                    }

                    //Random rnd = new Random();
                    //TransactionModel transaction = new TransactionModel
                    //{
                    //    AffCode = mes.Body.ToString(),//"abc333",
                    //    FullName = "Nguyễn Văn Toàn " + rnd.Next(1, 10),
                    //    TotalAmount = 150000,
                    //    TransactionId = rnd.Next(15, 500).ToString(),
                    //    Address = "Hà nội",
                    //    PhoneNumber = "098123456" + +rnd.Next(1, 99),
                    //    LenderId = rnd.Next(100, 500)
                    //};
                    //string message = "";
                    //TimaAffiliateEntities dbContext = new TimaAffiliateEntities();
                    //var repo0 = new LoanRepository(dbContext);
                    //repo0.CreateLoanCredit(transaction, out message);

                    //dbContext = new TimaAffiliateEntities();
                    //var repo = new TransactionRepository(dbContext);
                    //repo.CreateTransaction(transaction, out message);
                }
                catch (Exception ex)
                {
                    ErrorLog(HostingEnvironment.ApplicationPhysicalPath, "Error: " + ex.Message);
                }
            });
        }

        public static void ErrorLog(string sPathName, string sErrMsg)
        {
            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //this variable used to create log filename format "
            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            string sErrorTime = sYear + sMonth + sDay;

            StreamWriter sw = new StreamWriter(sPathName + sErrorTime, true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.Flush();
            sw.Close();
        }
    }
}