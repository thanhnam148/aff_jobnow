using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Aff.WebApplication.Security
{
    public class ClientSystemPrincipal : IPrincipal
    {
        #region Implementation of IPrincipal

        public bool IsInRole(string roleName)
        {
            return true;
        }

        /// <summary>
        /// Gets the identity of the current principal.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Security.Principal.IIdentity"/> object associated with the current principal.
        /// </returns>
        public IIdentity Identity { get; private set; }

        #endregion

        public ClientSystemIdentity ClientSystemIdentity { get { return (ClientSystemIdentity)Identity; } set { Identity = value; } }

        public ClientSystemPrincipal(ClientSystemIdentity identity)
        {
            Identity = identity;
        }
    }
}