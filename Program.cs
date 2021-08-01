// *************************************************************************************************
// DEMOPROGRAMM ZUM ABFRAGEN AUS DEM ACTIVE DIRECTORY
// NuGet Pakete: Novell.Directory.Ldap.NETStandard
// 1) Im DEBUG Modus akzeptiert das Programm jedes Passwort, da die Suchabfragen mit dem
//    testschueler00 Account durchgeführt werden.
//    Ausführen mit dotnet run oder Visual Studio startet den DEBUG Modus.
// 2) Im RELEASE Modus akzeptiert das Programm natürlich nur richtige Passwörter und braucht keinen
//    testschueler00 Account.
//    Ausführen mit dotnet run -c Release startet das Programm in RELEASE Modus.
// *************************************************************************************************
using AdLoginDemo.Services;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdLoginDemo
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.Write("Benutzername: ");
            var username = Console.ReadLine();
            Console.Write("Passwort: ");
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            var password = Console.ReadLine();
            Console.ForegroundColor = oldColor;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) { return; }
            Console.WriteLine("*************************************************************************************");
            Console.WriteLine("AD LIBRARY TESTPROGRAMM (STRG+C für Abbruch)");
            Console.WriteLine("*************************************************************************************");
            using var service = AdService.Login(username, password);
            var currentUser = service.CurrentUser;

            Console.WriteLine($"Angemeldet mit DN {currentUser.Dn}");
            if (currentUser.Role == AdUserRole.Pupil)
            {
                var ownClass = currentUser.Classes.FirstOrDefault() ?? "(N/A)";
                Console.WriteLine($"Deine Klasse ist: {ownClass}");
            }
            else if (currentUser.Role == AdUserRole.Teacher)
            {
                var ownClass = string.Join(", ", currentUser.Classes);
                Console.WriteLine($"Du unterrichtest folgende Klassen: {ownClass}");
            }

            // Für die nachfolgenden Abfragen verwenden wir einen Suchuser (Login ohne Argumente),
            // da wir das Kennwort des Benutzers nicht speichern wollen.
            // Falls kein Suchuser vorhanden ist, kann natürlich der eigene Benutzer verwendet werden.
            var classes = string.Join(", ", service.GetClasses());
            Console.WriteLine($"Gefundene Klassen: {classes}");

            var kv = service.GetKv("5AHIF");
            Console.WriteLine($"Der KV der 5AHIF ist {kv?.Firstname} {kv?.Lastname} ({kv?.Email})");

            var pupils = service.GetPupils("5AHIF")?.Select(p => $"{p.Lastname} {p.Firstname}") ?? new string[0];
            Console.WriteLine($"Folgende Schüler sind in der 5AHIF: {string.Join(", ", pupils)}");

            var teachers = service.GetTeachers("5AHIF")?.Select(p => $"{p.Lastname} {p.Firstname}") ?? new string[0];
            Console.WriteLine($"Folgende Lehrer unterrichten die 5AHIF: {string.Join(", ", teachers)}");

            // *************************************************************************************
            // SENDEN EINER MAIL ÜBER DEN EXCHANGE SERVER DER SCHULE
            // *************************************************************************************
            var message = new MimeMessage();
            using var client = new SpgMailClient(currentUser.Cn, password);

            Console.WriteLine();
            Console.WriteLine("*************************************************************************************");
            Console.WriteLine("SENDEN EINER EMAIL (STRG+C für Abbruch)");
            Console.WriteLine("*************************************************************************************");
            Console.Write("Name des Empfängers:           ");
            var mailTo = Console.ReadLine();
            Console.Write("E-Mail Adresse des Empfängers: ");
            var mailToAddress = Console.ReadLine();
            Console.Write("Reply to Adresse:              ");
            var replyToAddress = Console.ReadLine();

            // Die FROM Adresse muss der Exchange User sein, mit dem wir angemeldet sind.
            // Senden von anderen Adressen aus ist nicht erlaubt.
            message.From.Add(new MailboxAddress($"{currentUser.Firstname} {currentUser.Lastname}", client.SenderEmail));
            // Wenn der Empfänger auf Antworten klickt, soll die Nachricht an einen sinnvolleren Empfänger
            // gesendet werden (meist die Person, die die Benachrichtigung verursacht hat.
            message.ReplyTo.Add(new MailboxAddress($"{currentUser.Firstname} {currentUser.Lastname}", client.SenderEmail));
            // Der Empfänger
            message.To.Add(new MailboxAddress(mailTo, mailToAddress));
            // Betreff
            message.Subject = "Testmail aus dem Exchange Server";
            // Senden einer Nur-Text Nachricht
            message.Body = new TextPart("plain")
            {
                Text = $"Testmail gesendet um {DateTime.UtcNow} UTC."
            };

            await client.SendMailAsync(message);
        }
    }
}