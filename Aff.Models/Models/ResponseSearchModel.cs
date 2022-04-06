using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class ResponseSearchModel
    {
        public MetaData meta { get; set; }
        public List<TransactionModel> data { get; set; }
    }

    public class MetaData
    {
        public int page { get; set; }
        public int pages { get; set; }
        public int perpage { get; set; }
        public int total { get; set; }
        public string sort { get; set; }
        public string field { get; set; }
    }

    //public class data
    //{
    //    public List<TransactionModel> transactions { get; set; }
    //}
}
