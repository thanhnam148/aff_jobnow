using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aff.WebApplication.Security
{
    public interface IUserAccount
    {
        string UserName { get;  }
        int UserId { get;  }
        DateTime LastAccess { get; set; }
        string Token { get; set; }
        string FullName { get; set; }
        string Email { get; set; }
    }
}