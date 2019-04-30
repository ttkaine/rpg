using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Hangfire;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Warhammer.Core.Concrete
{
    public class EmailHandler : IEmailHandler
    {
        IAuthenticatedUserProvider _user;
        readonly IAdminSettingsProvider _settings;

        private string SendingMailAddress
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.SendingEmailAddress);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["AdminMailAddress"];
                }
                return setting;
            }
        }

        private string SendingMailAddressName
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.SendingEmailName);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["AdminMailAddressName"];
                }
                return setting;
            }
        }

        private string Password
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.SendingEmailPassword);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["AdminMailPassword"];
                }
                return setting;
            }
        }

        private string SmtpServer
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.SendingSmtpServer);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["SmtpServer"];
                }
                return setting;
            }
        }

        private string SmtpAccount
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.SendingSmtpAccount);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["SmtpAccount"];
                }
                return setting;
            }
        }

        private string SendGridKey
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.SendGridKey);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["SendGridKey"];
                }
                return setting;
            }
        }
        private bool UseHangfire
            
        {
            get
            {
                string setting = _settings.GetAdminSetting(AdminSettingName.UseHangfire);
                if (string.IsNullOrWhiteSpace(setting))
                {
                    setting = ConfigurationManager.AppSettings["UseHangfire"];
                }
                return !string.IsNullOrWhiteSpace(setting) && setting.ToLower() == "true";
            }
        }

        public EmailHandler(IAuthenticatedUserProvider user, IAdminSettingsProvider settings)
        {
            _user = user;
            _settings = settings;
        }

        public void NotifyPlayerTurn(Session session, Player player)
        {
            string subject = string.Format("{0}! Ahoy! It's your turn!", player.DisplayName);
            string message = string.Format("It's totally your turn in the text session '{0}' so go have a look!", session.FullName);

            MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
            MailAddress fromAddress = new MailAddress(SendingMailAddress, SendingMailAddressName);
            MailMessage mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            Send(toAddress, mail);
        }

        private void Send(MailAddress toAddress, MailMessage mail)
        {
            if (UseHangfire)
            {
                var jobId = BackgroundJob.Enqueue(
                    () => SendMail(mail, toAddress));
            }
            else
            {
                SendMail(mail, toAddress);
            }
        }

        public void SendMail(MailMessage mail, MailAddress toAddress)
        {
            if (!string.IsNullOrWhiteSpace(SendGridKey))
            {
                string plainText = Regex.Replace(mail.Body, "<.*?>", string.Empty);
                
                var apiKey = SendGridKey;
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(mail.From.Address, mail.From.DisplayName);
                var subject = mail.Subject;
                var to = new EmailAddress(toAddress.Address, toAddress.DisplayName);
                var plainTextContent = plainText;
                var htmlContent = mail.Body;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                try
                {
#if !DEBUG
                    var response = client.SendEmailAsync(msg);
#endif
                }
                catch (Exception exception)
                {
                    int i = 0;
                    LogException(exception, "Send Grid Emailer", i, DateTime.Now);
                    Exception inner = exception.InnerException;
                    while (inner != null)
                    {
                        i++;
                        LogException(inner, "Send Grid Emailer", i, DateTime.Now);
                        inner = inner.InnerException;
                    }
                    throw;
                }
            }
            else
            {
                SmtpClient client = new SmtpClient
                {
                    Credentials = new System.Net.NetworkCredential(SmtpAccount, Password),
                    Host = SmtpServer
                };

                try
                {
#if !DEBUG
                    client.Send(mail);
#endif
                }
                catch (SmtpException ex)
                {
                    int i = 0;
                    LogException(ex, "emailer", i, DateTime.Now);

                    Exception inner = ex.InnerException;
                    while (inner != null)
                    {
                        i++;
                        LogException(inner, "emailer", i, DateTime.Now);
                        inner = inner.InnerException;
                    }

                    string recipent = "Unknown Email";
                    MailAddress theToAddress = mail.To.FirstOrDefault();

                    if (toAddress != null)
                    {
                        recipent = $"{toAddress.DisplayName} at {toAddress.Address}";
                    }

                    var ex2 = new SmtpException($"Failed to send email to {recipent}: {ex.Message}", ex);
                    LogException(ex2, "emailer", i, DateTime.Now);
                    throw ex2;
                }
                catch (Exception exception)
                {
                    LogException(exception, "emailer", 0, DateTime.Now);
                    throw;
                }
            }
        }

        public void NotifyNewPage(Page page, List<Player> players)
        {
            foreach (Player player in players)
            {
                string subject = string.Format("{0}! Ahoy! {1} created a page called '{2}'", player.DisplayName, page.CreatedBy.DisplayName, page.FullName);
                string message = "So, there's this new page on the site now. So, totally check it out, right?";

                MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
                MailAddress fromAddress = new MailAddress(SendingMailAddress, SendingMailAddressName);
                MailMessage mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                Send(toAddress, mail);
            }
        }

        public void NotifyEditPage(Page page, List<Player> players)
        {
            foreach (Player player in players)
            {
                string subject = string.Format("{0}! Ahoy! {1} edited the page called '{2}'", player.DisplayName, page.CreatedBy.DisplayName, page.FullName);
                string message = "So, there's this page that's all updated and interesting on the site now. So, totally check it out, okay?";

                MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
                MailAddress fromAddress = new MailAddress(SendingMailAddress, SendingMailAddressName);
                MailMessage mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                Send(toAddress, mail);
            }
        }

        public void NotifyNewComment(string senderName, Page page, List<Player> players, string description)
        {
            foreach (Player player in players)
            {
                string subject = string.Format("{0}! Ahoy! {1} commented on the page called '{2}'!", player.DisplayName, senderName, page.FullName);
                string message = string.Format("<b>{0}:</b>{1}", senderName, description);

                MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
                MailAddress fromAddress = new MailAddress(SendingMailAddress, SendingMailAddressName);
                MailMessage mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                Send(toAddress, mail);
            }
        }

        public void PasswordReset(Player player, string callbackUrl)
        {
            string subject = string.Format("Hey, {0}! Did you forgot your password?", player.DisplayName);
            string message = string.Format("No worries! You can just set yoursself a new one by going to this link: <a href='{0}'>'{0}'</a>", callbackUrl);



            MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
            MailAddress fromAddress = new MailAddress(SendingMailAddress, SendingMailAddressName);
            MailMessage mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            Send(toAddress, mail);
        }

        private void LogException(Exception exception, string identifier, int sequence, DateTime timestamp)
        {
            var logger = DependencyResolver.Current.GetService<IExceptionLogHandler>();
            if (logger != null) logger.LogException(exception, identifier, sequence, timestamp);
        }
    }



}
