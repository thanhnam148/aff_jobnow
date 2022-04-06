using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class TransactionApiModel
    {
        public long TotalPurchaseAmount { get; set; }
        public int TotalInvoice { get; set; }
        public long TotalPaymentAmount { get; set; }
    }
    public class TransactionRequestModel
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public TransactionRequestModel()
        {

        }
        public TransactionRequestModel(string fromDate, string toDate)
        {
            FromDate = FromDate;
            ToDate = toDate;
        }
    }

    public class SummaryDashboardApiModel
    {
        public SummaryDashboardDetail TodaySummary { get; set; }
        public SummaryDashboardDetail CurrentMonthSummary { get; set; }
        public SummaryDashboardDetail PreviousMonthSummary { get; set; }
        public SummaryDashboardDetail AllTimeSummary { get; set; }
        public SummaryDashboardApiModel()
        {
            TodaySummary = new SummaryDashboardDetail();
            CurrentMonthSummary = new SummaryDashboardDetail();
            PreviousMonthSummary = new SummaryDashboardDetail();
            AllTimeSummary = new SummaryDashboardDetail();
        }
    }

    public class SummaryDashboardDetail
    {
        public long TotalRevenue { get; set; }
        public long TotalPaymentMMO { get; set; }
    }
}
