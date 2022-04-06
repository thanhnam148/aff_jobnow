using System;
using System.Configuration;

namespace Aff.WebApplication.Configurations
{
    public static class SystemConfiguration
    {

        public static int PageSizeDefault = 50;
        public static string GetStringKey(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? ConfigurationManager.AppSettings[key].Trim() : string.Empty;

        }
        public static string GetStringKey(string key, string defaultValue)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? ConfigurationManager.AppSettings[key].Trim() : defaultValue;

        }
        public static int GetInt32(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? int.Parse(ConfigurationManager.AppSettings[key]) : 0;
        }
    }
}
