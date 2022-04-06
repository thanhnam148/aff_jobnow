
using Aff.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aff.API.Models
{
    public static class Mapper
    {
        public static tblUser MapToEntity(this UserModel model)
        {
            var user = new tblUser();
            user.UserId = model.UserId;
            user.PassWord = model.PassWord;
            user.UserName = model.UserName;
            user.CreatedDate = model.CreatedDate;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.IsActive = model.IsActive;
            user.AffCode = model.AffCode;
            user.Address = model.Address;
            user.Company = model.Company;
            return user;
        }
    }
}