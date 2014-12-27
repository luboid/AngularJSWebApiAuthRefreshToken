using Microsoft.AspNet.Identity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace AngularJSAuthRefreshToken.Web
{
    public class EmailService : IIdentityMessageService
    {
        static readonly SmtpSection section = WebConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
        static readonly MailAddress from = new MailAddress(section.From);

        static volatile bool messagesToSendWorking = false;
        static ConcurrentQueue<IdentityMessage> messagesToSend = new ConcurrentQueue<IdentityMessage>();
        static System.Threading.Timer messagesToSendTimer = new System.Threading.Timer((state) =>
        {
            if (messagesToSendWorking)
            {
                return;
            }

            messagesToSendWorking = true;
            Task.Run(() =>
            {
                IdentityMessage message;
                using (SmtpClient client = new SmtpClient())
                {
                    while (messagesToSend.TryDequeue(out message))
                    {
                        SendEmail(client, message);
                    }
                }
                messagesToSendWorking = false;
            });

        }, null, 0, 30 * 1000);

        static void SendEmail(SmtpClient client, IdentityMessage message)
        {
            try
            {
                var m = new MailMessage
                {
                    From = from,
                    Subject = message.Subject,
                    Body = message.Body,
                    IsBodyHtml = true
                };

                m.To.Add(message.Destination);

                client.Send(m);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }


        public Task SendAsync(IdentityMessage message)
        {
            messagesToSend.Enqueue(message);
            return Task.FromResult<int>(0);
        }
    }
}