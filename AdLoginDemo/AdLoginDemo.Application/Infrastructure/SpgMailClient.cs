using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdLoginDemo.Application.Infrastructure
{
    public class SpgMailClient : IDisposable
    {
        public static string SmtpServer { get; } = "mail.spengergasse.at";
        public static int SmtpPort { get; } = 587;

        private readonly MailKit.Net.Smtp.SmtpClient _smtpClient;
        private bool _disposed = false;

        public static async Task<SpgMailClient> Create(string username, string password, System.Threading.CancellationToken token = default)
        {
            var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, token);
            await client.AuthenticateAsync(username, password, token);
            return new SpgMailClient(client);
        }

        private SpgMailClient(MailKit.Net.Smtp.SmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public Task SendMailAsync(MimeMessage message, System.Threading.CancellationToken token = default) => _smtpClient.SendAsync(message, token);

        public async Task<MimeMessage[]> SendMultipleMailAsync(IEnumerable<MimeMessage> messages, System.Threading.CancellationToken token = default)
        {
            if (messages is null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            var unsent = new List<MimeMessage>();
            foreach (var message in messages)
            {
                if (!token.IsCancellationRequested)
                {
                    try { await _smtpClient.SendAsync(message, token); }
                    catch { unsent.Add(message); }
                }
                else
                {
                    unsent.Add(message);
                }
            }
            return unsent.ToArray();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _smtpClient.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}