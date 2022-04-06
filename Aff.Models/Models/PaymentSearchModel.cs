using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class PaymentSearchModel : Paging
    {
        public string TextSearch { get; set; }
        public List<PaymentModel> Payments { get; set; }
        public void RequestParam(System.Collections.Specialized.NameValueCollection Params)
        {
            int value = 0;
            if (int.TryParse(Params["datatable[pagination][page]"], out value))
            {
                PageIndex = value;
            }
            if (int.TryParse(Params["datatable[pagination][pages]"], out value))
            {
                Pages = value;
            }
            if (int.TryParse(Params["datatable[pagination][perpage]"], out value))
            {
                if (value == 0) value = 30;
                PageSize = value;
            }
            if (int.TryParse(Params["datatable[pagination][total]"], out value))
            {
                TotalRecords = value;
            }
            Field = Params["datatable[sort][field]"];
            Sort = Params["datatable[sort][sort]"];
            TextSearch = Params["datatable[query][generalSearch]"];
        }
    }
}
