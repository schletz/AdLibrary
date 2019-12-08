using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdLibrary.Api;

namespace AdLibrary.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Passwort: ");
            ConsoleColor background = Console.BackgroundColor;
            ConsoleColor foreground = Console.ForegroundColor;
            Console.ForegroundColor = background;
            string password = Console.ReadLine();
            Console.ForegroundColor = foreground;

            using (AdSearcher searcher = new AdSearcher())
            {
                try
                {
                    searcher.Authenticate(username, password);

                    /* Beispiel 1: Wer bin ich? Alle Properties des aktuellen Users
                     * in die Konsole schreiben. */
                    Console.Clear();
                    Console.WriteLine($"Details zu User {username}");
                    Console.WriteLine(searcher.CurrentUser);
                    Console.ReadKey();

                    /* Beispiel 2: Welches Homeverzeichnis habe ich? Das steht im Property
                     * homedirectory. Da das Property nur 1x gesetzt ist, kann mit Value
                     * ein einzelner Wert angerufen werden. Ohne Value sind Properties immer
                     * eine Collection von Werten! */
                    Console.Clear();
                    Console.WriteLine($"Homeverzeichnis von {username}");
                    Console.WriteLine(searcher.CurrentUser["homedirectory"].Value);

                    /* Beispiel 3: Wer hat den Usernamen ABC1234? */
                    Console.WriteLine($"Name des Users ABC1234");
                    Console.WriteLine(searcher.FindCn("ABC1234")?.Displayname);

                    /* Beispiel 4: Welche Schüler sind in der 5AHIF? */
                    Console.WriteLine($"Schülerliste der 5AHIF");
                    var pupils = searcher.FindGroupMembers("5AHIF");
                    foreach (AdEntry p in pupils.OrderBy(p => p.Displayname))
                    {
                        Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", p.Displayname, p.Lastname, p.Firstname, p.Email, p.IsTeacher);
                    };

                    /* Beispiel 5: Welche Lehrer unterrichten die 5AHIF? Dafür sehen wir uns
                     * die Gruppe Lehrende_5AHIF an. */
                    Console.WriteLine($"Lehrer der 5AHIF");
                    var teachers = searcher.FindGroupMembers("Lehrende_5AHIF");
                    foreach (AdEntry t in teachers.OrderBy(t => t.Displayname))
                    {
                        Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", t.Displayname, t.Lastname, t.Firstname, t.Email, t.IsTeacher);
                    };

                    /* Beispiel 6: Suche alle Personenobjekte in 
                     * OU=Lehrer,OU=Automatisch gewartete Benutzer,OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule */
                    Console.WriteLine($"Benutzer im Suchpfad OU=Lehrer,OU=Automatisch gewartete Benutzer,OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule");
                    var allTeachers = searcher.Find("objectclass", "person", "OU=Lehrer,OU=Automatisch gewartete Benutzer,OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule");
                    foreach (AdEntry t in allTeachers.OrderBy(at => at.Displayname))
                    {
                        Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", t.Displayname, t.Lastname, t.Firstname, t.Email, t.IsTeacher);
                    };
                }
                catch (LoginException e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.InnerException?.Message);
                }
                catch (NetworkException e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.InnerException?.Message);
                }
                catch (AdException e)
                {
                    Console.Error.WriteLine(e.Message);
                    Console.Error.WriteLine(e.InnerException?.Message);
                }
            }
        }
    }
}
