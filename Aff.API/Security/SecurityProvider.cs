using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aff.API.Security
{
    public class SecurityProvider
    {
        public static IUserAccount GetAccountByToken(string token)
        {
            return TokenStore.Instance.Get(token);
        }
               
        public static void StoreAccount(IUserAccount account)
        {
            TokenStore.Instance.Add(account);
        }

        public static void UpdateAccount(IUserAccount account)
        {
            TokenStore.Instance.Update(account);
        }

        public static void RemoveToken(string token)
        {
            TokenStore.Instance.Remove(token);
        }
    }
}