using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Aff.WebApplication.Models
{
    public enum LoginResult
    {
        Unknown = 0,
        Success = 1,
        IsLockedOut = 2,
        InvalidEmail = 3,
        InvalidPassword = 4,
        UnActive = 5,
    }
    public enum MessageType
    {
        Success,
        Error,
        Notice,
        Warning
    }

    public enum NotificationType
    {
        System = 1,
        Message = 2
    }

    public enum UserTokenType : short
    {
        ActiveAccount = 1,
        PasswordRecover = 2,
        RemoteLogin = 3
    }

}
