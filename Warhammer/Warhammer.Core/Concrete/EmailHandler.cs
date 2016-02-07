using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using Warhammer.Core.Abstract;
using Warhammer.Core.Entities;

namespace Warhammer.Core.Concrete
{
    public class EmailHandler : IEmailHandler
    {
        IAuthenticatedUserProvider _user;

        private string SendingMailAddress
        {
            get { return ConfigurationManager.AppSettings["AdminMailAddress"]; }
        }

        private string Password
        {
            get { return ConfigurationManager.AppSettings["AdminMailPassword"]; }
        }

        private string SMTPServer
        {
            get { return ConfigurationManager.AppSettings["SMTPServer"]; }
        }

        public EmailHandler(IAuthenticatedUserProvider user)
        {
            _user = user;
        }


        public void NotifyPlayerTurn(Session session, Player player)
        {
            string subject = string.Format("{0}! Ahoy! It's your turn!", player.DisplayName);
            string message = string.Format("It's totally your turn in the text session '{0}' so go have a look!", session.FullName);
            NetworkCredential loginInfo = new NetworkCredential(SendingMailAddress, Password);

            SmtpClient client = new SmtpClient(SMTPServer)
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                EnableSsl = false,
                Credentials = loginInfo
            };

            MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
            MailAddress fromAddress = new MailAddress(SendingMailAddress, "The Pirate Captain");
            MailMessage mail = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            client.Send(mail);
        }

        public void NotifyNewPage(Page page, List<Player> players)
        {
            foreach (Player player in players)
            {
                string subject = string.Format("{0}! Ahoy! {1} created a page called '{2}'", player.DisplayName, page.CreatedBy.DisplayName, page.FullName);
                string message = "So, there's this new page on the site now. So, totally check it out, right?";
                NetworkCredential loginInfo = new NetworkCredential(SendingMailAddress, Password);

                SmtpClient client = new SmtpClient(SMTPServer)
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = false,
                    Credentials = loginInfo
                };

                MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
                MailAddress fromAddress = new MailAddress(SendingMailAddress, "The Pirate Captain");
                MailMessage mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
            client.Send(mail);

            }
        }

        public void NotifyEditPage(Page page, List<Player> players)
        {
            foreach (Player player in players)
            {
                string subject = string.Format("{0}! Ahoy! {1} edited the page called '{2}'", player.DisplayName, page.CreatedBy.DisplayName, page.FullName);
                string message = "So, there's this page that's all updated and interesting on the site now. So, totally check it out, okay?";
                NetworkCredential loginInfo = new NetworkCredential(SendingMailAddress, Password);

                SmtpClient client = new SmtpClient(SMTPServer)
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = false,
                    Credentials = loginInfo
                };

                MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
                MailAddress fromAddress = new MailAddress(SendingMailAddress, "The Pirate Captain");
                MailMessage mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                client.Send(mail);

            }
        }

        public void NotifyNewComment(string senderName, Page page, List<Player> players)
        {
            foreach (Player player in players)
            {
                string subject = string.Format("{0}! Ahoy! {1} commented on the page called '{2}'!", player.DisplayName, senderName, page.FullName);
                string message = "If you want to know what the comment was you'll just have to go look, because I forgot already, okay?";
                NetworkCredential loginInfo = new NetworkCredential(SendingMailAddress, Password);

                SmtpClient client = new SmtpClient(SMTPServer)
                {
                    Port = 25,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = false,
                    Credentials = loginInfo
                };

                MailAddress toAddress = new MailAddress(player.UserName, player.DisplayName);
                MailAddress fromAddress = new MailAddress(SendingMailAddress, "The Pirate Captain");
                MailMessage mail = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                client.Send(mail);

            }
        }
    }
}
