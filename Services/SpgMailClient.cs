using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdLoginDemo.Services
{
    class SpgMailClient : MailKit.Net.Smtp.SmtpClient
    {
        public string SmtpServer { get; } = "mail.spengergasse.at";
        public int SmtpPort { get; } = 587;
        public string Username { get; } = "(Hier einen fixen Benutzer einfügen)";
        public string SenderEmail => $"{Username}@spengergasse.at";
        public string Password { get; } = "(Hier das Passwort einfügen)";

        public SpgMailClient() { }

        public SpgMailClient(string username, string password)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }

        /// <summary>
        /// Sendet eine Nachricht über den Server.
        /// </summary>
        /// <param name="message">Die zu sendende Nachricht.</param>
        /// <param name="token">Token für den Abbruch der Operation.</param>
        /// <returns></returns>
        public async Task SendMailAsync(MimeMessage message, System.Threading.CancellationToken token = default)
        {
            try
            {
                await this.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, token);
                await this.AuthenticateAsync(Username, Password, token);
                await this.SendAsync(message, token);
            }
            catch (TaskCanceledException) { }
            catch { throw; }
            finally { await this.DisconnectAsync(true); }
        }
        /// <summary>
        /// Sendet mehrere Nachrichten über den Server.
        /// </summary>
        /// <param name="messages">Die zu sendenden Nachrichten.</param>
        /// <param name="token">Token für den Abbruch der Operation.</param>
        /// <returns>Durch einen Fehler nicht gesendete Nachrichten.</returns>
        public async Task<MimeMessage[]> SendMultipleMailAsync(IEnumerable<MimeMessage> messages, System.Threading.CancellationToken token = default)
        {
            if (messages is null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            var unsent = new List<MimeMessage>();
            try
            {
                await this.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, token);
                await this.AuthenticateAsync(Username, Password, token);
                foreach (var message in messages)
                {
                    try { await this.SendAsync(message, token); }
                    catch (TaskCanceledException) { throw; }
                    catch { unsent.Add(message); }
                }
            }
            catch (TaskCanceledException) { }
            catch { throw; }
            finally { await this.DisconnectAsync(true); }
            return unsent.ToArray();
        }
    }
}
