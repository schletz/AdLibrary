using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdLoginDemo.Application.Infrastructure;
using Xunit;

namespace AdLoginDemo.Test
{
    public class SpgMailClientTests
    {
        private static readonly string username = "";
        private static readonly string password = "";

        [Fact]
        public async Task SendMailSuccessTest()
        {
            var recipient = "testadresse@gmail.com";

            using var client = await SpgMailClient.Create(username, password);
            var message = new MimeKit.MimeMessage();
            message.From.Add(new MimeKit.MailboxAddress("Ich", $"{username}@spengergasse.at"));
            message.ReplyTo.Add(new MimeKit.MailboxAddress("Ich", $"{username}@spengergasse.at"));
            message.To.Add(new MimeKit.MailboxAddress("Du", recipient));
            message.Subject = "Testmail aus dem Exchange Server";
            message.Body = new MimeKit.TextPart("plain")
            {
                Text = $"Testmail gesendet um {DateTime.UtcNow} UTC."
            };
            await client.SendMailAsync(message);
        }
    }
}