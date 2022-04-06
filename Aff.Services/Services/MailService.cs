using Aff.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Aff.Services
{
    public interface IMailService : IDisposable
    {
        Task<bool> SendMail(MailAddress mailFrom, MailAddress mailTo, string subject, string body, SmtpInfo smtp, out string errorMsg, List<Attachment> attachments = null);
        bool SendMail(string sendTo, string title, string body, List<string> attachments = null);

        void SendMail(string sendFrom, string sendTo, string title, string body);
    }
    public class MailService : IMailService
    {
        #region Methods
        public bool SendMail(string sendTo, string title, string body, List<string> attachments = null)
        {

            var host = GetStringKey("SmtpMailServer");
            var username = GetStringKey("MailServerUserName");
            var dispayName = GetStringKey("MailServerUserName");
            var password = GetStringKey("MailServerPassword");
            var port = GetInt32("MailServerPort");

            var mailFrom = new MailAddress(username, "aff.jobnow.com.vn");
            var mailTo = new MailAddress(sendTo);
            var smtpInfo = new SmtpInfo()
            {
                AuthenticationPassword = password,
                AuthenticationUserName = username,
                HasAuthentication = true,
                SmtpHost = host,
                SmtpPort = port
            };
            var attachmentFiles = attachments?.Select(x => new Attachment(x)).ToList();
            string errorMsg;
            return SendMail(mailFrom, mailTo, title, body, smtpInfo, out errorMsg, attachmentFiles).Result;

        }

        public Task<bool> SendMail(MailAddress mailFrom, MailAddress mailTo, string subject, string body, SmtpInfo smtp, out string errorMsg, List<Attachment> attachments = null)
        {
            //step1: validate param input
            errorMsg = string.Empty;
            if (!ValidateParamForSendMail(mailFrom, mailTo, subject, body, smtp, out errorMsg))
            {
                return Task.FromResult(false);
            }
            try
            {
                //Send mail.
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = smtp.SmtpHost;
                    smtpClient.Port = smtp.SmtpPort;
                    smtpClient.UseDefaultCredentials = false;
                    if (smtp.HasAuthentication)
                    {
                        smtpClient.Credentials = new NetworkCredential(smtp.AuthenticationUserName,
                                                                       smtp.AuthenticationPassword);
                    }

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.EnableSsl = true;
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.From = mailFrom;
                        mailMessage.To.Add(mailTo);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;
                        if (attachments != null)
                            foreach (var attachment in attachments)
                            {
                                mailMessage.Attachments.Add(attachment);
                            }
                        smtpClient.Send(mailMessage);
                        return Task.FromResult(true);
                    }
                }
            }
            catch (Exception exception)
            {
                errorMsg = "Server Internal Error";
                return Task.FromResult(false);
            }

        }

        public void SendMail(string sendFrom, string sendTo, string title, string body)
        {

            var host = GetStringKey("SmtpMailServer");
            var username = GetStringKey("MailServerUserName");
            var dispayName = GetStringKey("MailServerUserName");
            var password = GetStringKey("MailServerPassword");
            var port = GetInt32("MailServerPort");

            var mailFrom = new MailAddress(sendFrom);
            var mailTo = new MailAddress(sendTo);
            var smtpInfo = new SmtpInfo()
            {
                AuthenticationPassword = password,
                AuthenticationUserName = username,
                HasAuthentication = true,
                SmtpHost = host,
                SmtpPort = port
            };
            var errorMsg = string.Empty;
            SendMail(mailFrom, mailTo, title, body, smtpInfo, out errorMsg);

        }
        #endregion

        #region Helpers    
        private bool ValidateParamForSendMail(MailAddress mailFrom, MailAddress mailTo, string subject, string body,
                                            SmtpInfo smtp, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (mailFrom == null)
            {
                errorMsg = "MailFrom is required";
                return false;
            }

            if (mailTo == null)
            {
                errorMsg = "MailTo is required";
                return false;
            }

            if (string.IsNullOrEmpty(subject))
            {
                errorMsg = "Subject email is required";
                return false;
            }

            if (body == null)
            {
                errorMsg = "Body email is required";
                return false;
            }

            if (smtp == null)
            {
                errorMsg = "Smtp info email is required";
                return false;
            }
            return true;
        }

        public string GetStringKey(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? ConfigurationManager.AppSettings[key].Trim() : string.Empty;

        }
        public string GetStringKey(string key, string defaultValue)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? ConfigurationManager.AppSettings[key].Trim() : defaultValue;

        }
        public int GetInt32(string key)
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings[key]) ? int.Parse(ConfigurationManager.AppSettings[key]) : 0;
        }
        #endregion

        #region GCs    
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
