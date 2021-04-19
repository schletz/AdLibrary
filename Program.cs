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
    class Program
    {
        static async Task Main()
        {
            Console.Write("Benutzername: ");
            var username = Console.ReadLine();
            Console.Write("Passwort: ");
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = Console.BackgroundColor;
            var password = Console.ReadLine();
            Console.ForegroundColor = oldColor;

            var myUser = AdService.Login(username, password).CurrentUser;
            Console.WriteLine($"Angemeldet mit DN {myUser.Dn}");

#if DEBUG
            Console.WriteLine("Das Programm läuft im DEBUG Modus.");
            Console.WriteLine($"Die Benutzerdaten wurden daher mit dem Suchuser {AdService.Searchuser.user} ohne Benutzerkennwort geladen.");
#endif

            if (myUser.Role == AdUserRole.Pupil)
            {
                var ownClass = myUser.Classes.FirstOrDefault() ?? "(N/A)";
                Console.WriteLine($"Deine Klasse ist: {ownClass}");
            }
            else if (myUser.Role == AdUserRole.Teacher)
            {
                var ownClass = string.Join(", ", myUser.Classes);
                Console.WriteLine($"Du unterrichtest folgende Klassen: {ownClass}");
            }

            // Für die nachfolgenden Abfragen verwenden wir einen Suchuser (Login ohne Argumente),
            // da wir das Kennwort des Benutzers nicht speichern wollen.
            // Falls kein Suchuser vorhanden ist, kann natürlich der eigene Benutzer verwendet werden.
            var adService = AdService.Login();
            var classes = string.Join(", ", adService.GetClasses());
            Console.WriteLine($"Gefundene Klassen: {classes}");

            var kv = adService.GetKv("5AHIF");
            Console.WriteLine($"Der KV der 5AHIF ist {kv?.Firstname} {kv?.Lastname} ({kv?.Email})");

            var pupils = adService.GetPupils("5AHIF")?.Select(p => $"{p.Lastname} {p.Firstname}") ?? new string[0];
            Console.WriteLine($"Folgende Schüler sind in der 5AHIF: {string.Join(", ", pupils)}");

            var teachers = adService.GetTeachers("5AHIF")?.Select(p => $"{p.Lastname} {p.Firstname}") ?? new string[0];
            Console.WriteLine($"Folgende Lehrer unterrichten die 5AHIF: {string.Join(", ", teachers)}");

            // *************************************************************************************
            // SENDEN EINER MAIL ÜBER DEN EXCHANGE SERVER DER SCHULE
            // *************************************************************************************
            var message = new MimeMessage();
            using var client = new SpgMailClient(myUser.Cn, password);

            Console.WriteLine("SENDEN EINER EMAIL (STRG+C für Abbruch)");
            Console.Write("Name des Empfängers:           ");
            var mailTo = Console.ReadLine();
            Console.Write("E-Mail Adresse des Empfängers: ");
            var mailToAddress = Console.ReadLine();
            Console.Write("Reply to Adresse:              ");
            var replyToAddress = Console.ReadLine();

            // Die FROM Adresse muss der Exchange User sein, mit dem wir angemeldet sind.
            // Senden von anderen Adressen aus ist nicht erlaubt.
            message.From.Add(new MailboxAddress($"{myUser.Firstname} {myUser.Lastname}", client.SenderEmail));
            // Wenn der Empfänger auf Antworten klickt, soll die Nachricht an einen sinnvolleren Empfänger
            // gesendet werden (meist die Person, die die Benachrichtigung verursacht hat.
            message.ReplyTo.Add(new MailboxAddress($"{myUser.Firstname} {myUser.Lastname}", client.SenderEmail));
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
