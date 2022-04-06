using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models
{ 
    public class SmtpInfo
    {
        public string ApiKey { get; set; }

        public string AuthenticationUserName { get; set; }

        public string AuthenticationPassword { get; set; }

        public bool HasAuthentication { get; set; }

        public string SmtpHost { get; set; }

        public int SmtpPort { get; set; }
    }
}
