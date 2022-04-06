using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;

namespace Aff.WebApplication.Security
{
    public class TokenStore
    {
        private static readonly Lazy<TokenStore> Lazy = new Lazy<TokenStore>(() => new TokenStore());

        public static TokenStore Instance { get { return Lazy.Value; } }

        private TokenStore() { }

        private static readonly ObjectCache Cache = MemoryCache.Default;
        private const int DefaultTokenTimeOut = 20;

        private int TokenTimeOut
        {
            get
            {
                var value = 20;
                if (value > 0)
                {
                    return value;
                }
                return DefaultTokenTimeOut;
            }
        }

        private CacheItemPolicy GetPolicy()
        {
            return new CacheItemPolicy
            {
                Priority = (System.Runtime.Caching.CacheItemPriority)CacheItemPriority.Default,
                SlidingExpiration = new TimeSpan(0, TokenTimeOut, 0),
                UpdateCallback = this.CacheEntryUpdate
            };
        }

        private void Set(String token, IUserAccount context)
        {
            Cache.Set(token, context, GetPolicy());
        }

        public void Add(IUserAccount accountContext)
        {
            var now = DateTime.UtcNow;
            var tValue = string.Format("{0}-{1}-{2}", accountContext.Email, accountContext.UserId, now.Ticks);
            var token = SecurityUtil.GenerateToken(tValue);

            accountContext.Token = token;
            accountContext.LastAccess = now;
        
            Set(token, accountContext);
        }

        public void Update(IUserAccount accountContext)
        {
            var token = accountContext.Token;
            if (string.IsNullOrEmpty(token) || !Cache.Contains(token))
                return;

            accountContext.LastAccess = DateTime.UtcNow;
            Set(token, accountContext);
        }

        public IUserAccount Get(String token)
        {
            if (string.IsNullOrEmpty(token) || !Cache.Contains(token))
                return null;

            var context = Cache[token] as IUserAccount;
            if (context == null) return null;
            context.LastAccess = DateTime.UtcNow;

            return context;
        }

        public static List<IUserAccount> GetAll()
        {
            return Cache.Select(pair => pair.Value as IUserAccount).ToList();
        }

        public void Remove(String token)
        {
            if (Cache.Contains(token))
            {
                Cache.Remove(token);
            }
        }

        private void CacheEntryUpdate(CacheEntryUpdateArguments args)
        {
            var cacheItem = Cache.GetCacheItem(args.Key);
            if (cacheItem == null) return;
            if (args.RemovedReason == CacheEntryRemovedReason.Expired || args.RemovedReason == CacheEntryRemovedReason.Removed)
            {
                var context = cacheItem.Value as IUserAccount;
                if (context != null && context.LastAccess.AddMinutes(TokenTimeOut) > DateTime.UtcNow)
                {
                    cacheItem.Value = context;
                    args.UpdatedCacheItem = cacheItem;
                    args.UpdatedCacheItemPolicy = GetPolicy();
                }
            }
        }
    }
}