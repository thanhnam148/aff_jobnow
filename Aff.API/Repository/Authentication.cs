using Aff.API.Models;
using Aff.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aff.API.Repository
{
    public class Authentication : IAuthentication
    {
        TimaAffiliateEntities _context;
        public Authentication()
        {
            _context = new TimaAffiliateEntities();
        }

        public void AddUser(UserModel registeruser)
        {
            _context.tblUsers.Add(registeruser.MapToEntity());
            _context.SaveChanges();
        }

        public int GetLoggedUserID(UserModel registeruser)
        {
            var usercount = (from User in _context.tblUsers
                             where User.Email == registeruser.Email && User.PassWord == registeruser.PassWord
                             select User.UserId).FirstOrDefault();

            return usercount;
        }

        public tblUser ValidateRegisteredUser(UserModel registeruser)
        {
            registeruser.PassWord = Security.SecurityUtil.EncryptText(registeruser.PassWord);
            var userLogin = _context.tblUsers.FirstOrDefault(u => u.Email == registeruser.Email && u.PassWord == registeruser.PassWord);
            return userLogin;
        }

        public bool ValidateUsername(UserModel registeruser)
        {
            var usercount = (from User in _context.tblUsers
                             where User.Email == registeruser.Email
                             select User).Count();
            if (usercount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}