using Aff.API.Models;
using Aff.DataAccess;

namespace Aff.API.Repository
{
    public interface IAuthentication
    {
        void AddUser(UserModel registeruser);
        tblUser ValidateRegisteredUser(UserModel registeruser);
        bool ValidateUsername(UserModel registeruser);
        int GetLoggedUserID(UserModel registeruser);
    }
}
