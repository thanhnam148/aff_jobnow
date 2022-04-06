using Aff.Models.Models;
using System.Security.Principal;
using System.Web.Security;

namespace Aff.WebApplication.Security
{
    public class ClientSystemIdentity : IIdentity
    {
        #region Properties

        public IIdentity Identity { get; set; }
        public UserCommon UserCommon { get; set; }

        #endregion

        #region Implementation of IIdentity

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        /// <returns>
        /// The name of the user on whose behalf the code is running.
        /// </returns>
        public string Name
        {
            get { return Identity.Name; }
        }

        /// <summary>
        /// Gets the type of authentication used.
        /// </summary>
        /// <returns>
        /// The type of authentication used to identify the user.
        /// </returns>
        public string AuthenticationType
        {
            get { return Identity.AuthenticationType; }
        }

        /// <summary>
        /// Gets a value that indicates whether the user has been authenticated.
        /// </summary>
        /// <returns>
        /// true if the user was authenticated; otherwise, false.
        /// </returns>
        public bool IsAuthenticated { get { return Identity.IsAuthenticated; } }

        #endregion

        #region Constructor

        public ClientSystemIdentity(IIdentity identity)
        {
            Identity = identity;
            var customMembershipUser = (ClientSystemMembershipUser)Membership.GetUser(identity.Name);
            if (customMembershipUser != null)
            {
                UserCommon = customMembershipUser.UserCommon;
            }
        }

        #endregion
    }
}