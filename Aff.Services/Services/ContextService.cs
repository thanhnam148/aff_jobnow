using System;
using System.Web;
namespace Aff.Services
{
    public interface IContextService
    {
        void SetCulture(string culture);
        string GetRequestCulture();
        void ClearSession();
        bool ContainsInSession(string key);
        void RemoveFromSession(string key);
        void SaveInSession(string key, object value);
        T GetFromSession<T>(string key, object defaultValue = null);
        object GetObjectFromSession(string key, object defaultValue = null);
        void ClearCookie(string key);
        void ClearCookie();
        void RemoveFromCookie(string key);
        void SaveInCookie(string key, object value, int dateExpire = 1);
        string GetFromCookie(string key, string defaultValue = null);
        object GetAndRemoveSession(string key, object defaultValue = null);
    }
    public class ContextService : IContextService
    {
        public void SetCulture(string culture)
        {
            HttpContext.Current.Session["language"] = culture;
        }

        public string GetRequestCulture()
        {
            return (string)HttpContext.Current.Session["language"];
        }

        #region Session
        /// <summary>
        /// Clear session
        /// </summary>
        public void ClearSession()
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return;
            HttpContext.Current.Session.Clear();
        }

        /// <summary>
        /// Check Object Exist In Session
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>yes or no</returns>
        public bool ContainsInSession(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return false;
            return HttpContext.Current.Session[key] != null;
        }

        /// <summary>
        /// Remove Object Out Of Session
        /// </summary>
        /// <param name="key">key</param>
        public void RemoveFromSession(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return;
            HttpContext.Current.Session.Remove(key);
        }

        /// <summary>
        /// Save Object Into Session If Not Exist Or Update If Exist
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void SaveInSession(string key, object value)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return;
            HttpContext.Current.Session[key] = value;
        }

        /// <summary>
        /// Get Object From Session
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>object</returns>
        public T GetFromSession<T>(string key, object defaultValue = null)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return (T)defaultValue;
            return (T)HttpContext.Current.Session[key];
        }

        /// <summary>
        /// Get Object From Session
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>object</returns>
        public object GetObjectFromSession(string key, object defaultValue = null)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return defaultValue;
            return HttpContext.Current.Session[key];
        }

        public object GetAndRemoveSession(string key, object defaultValue = null)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return defaultValue;
            var result = HttpContext.Current.Session[key].ToString();
            HttpContext.Current.Session.Remove(key);
            return result;
        }
        #endregion

        #region Cookie
        public void ClearCookie(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Response == null)
                return;

            var aCookie = new HttpCookie(key);
            aCookie.Expires = DateTime.Now.AddDays(-1);
            HttpContext.Current.Response.Cookies.Add(aCookie);
        }

        /// <summary>
        /// Clear Cookie
        /// </summary>
        public void ClearCookie()
        {
            if (HttpContext.Current == null || HttpContext.Current.Response == null)
                return;
            int limit = HttpContext.Current.Response.Cookies.Count;
            for (int i = 0; i < limit; i++)
            {
                ClearCookie(HttpContext.Current.Response.Cookies[i].Name);
            }
        }

        /// <summary>
        /// Check Object Exist In Cookie
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>yes or no</returns>
        public bool ContainsInCookie(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                return false;
            return HttpContext.Current.Request.Cookies[key] != null;
        }

        /// <summary>
        /// Remove Object Out Of Cookie
        /// </summary>
        /// <param name="key">key</param>
        public void RemoveFromCookie(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Response == null)
                return;
            var cookie = new HttpCookie(key);
            cookie.Expires = DateTime.Now.AddDays(-1d);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Save Object Into Cookie If Not Exist Or Update If Exist
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void SaveInCookie(string key, object value, int dateExpire = 1)
        {
            if (HttpContext.Current == null || value == null)
                return;

            var cookie = new HttpCookie(key) { Value = value.ToString() };
            cookie.Expires = DateTime.Now.AddDays(dateExpire);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Get String From Cookie
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="defaultValue"></param>
        /// <returns>object</returns>
        public string GetFromCookie(string key, string defaultValue = null)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request.Cookies[key] == null)
                return defaultValue;
            return HttpContext.Current.Request.Cookies[key].Value;
        }

        /// <summary>
        /// Get T Object From Cookie
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        //public static T GetFromCookie<T>(string key, object defaultValue = null)
        //{
        //    if (HttpContext.Current == null || HttpContext.Current.Request == null)
        //        return (T)defaultValue;
        //    string strValue = HttpContext.Current.Request.Cookies[key].Value;
        //    T obj = Activator.CreateInstance<T>();
        //    if (strValue.Contains("|")) //  save into cookie with format as object
        //    {
        //        PropertyInfo[] properties = (typeof(T)).GetProperties();
        //        string[] arrayValue = strValue.Split('|');
        //        if (properties.Length == arrayValue.Length)
        //        {
        //            try
        //            {
        //                for (int i = 0; i < arrayValue.Length; i++)
        //                {
        //                    properties[i].SetValue(obj, Convert.ChangeType(arrayValue[i], properties[i].PropertyType), null);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                obj = (T)defaultValue;
        //            }
        //        }
        //    }
        //    return obj;
        //}
        #endregion
    }
}
