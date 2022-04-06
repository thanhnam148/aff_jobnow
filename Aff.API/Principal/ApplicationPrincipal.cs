using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Aff.API.Principal
{
    public class ApplicationPrincipal: GenericPrincipal
    {
        public ApplicationPrincipal(int userId, IIdentity identity, string[] roles) : base(identity, roles)
        {
            this.UserId = userId;
        }

        public int UserId { get; private set; }

    }

}