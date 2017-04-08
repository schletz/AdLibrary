using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdLibrary;

namespace AdLibrary.App
{
    class Program
    {
        static void Main(string[] args)
        {
            string username, password;

            Console.Write("Username: "); username = Console.ReadLine();
            Console.Write("Passwort (wird angezeigt!): "); password = Console.ReadLine();
            using (AdSearcher searcher = new AdSearcher())
            {
                try
                {
                    if (searcher.Authenticate(username, password))
                    {
                        /* Beispiel 1: Wer bin ich? Alle Properties des aktuellen Users
                         * in die Konsole schreiben. */
                        Console.WriteLine(searcher.CurrentUser);
                        Console.ReadKey();

                        /* Beispiel 2: Welches Homeverzeichnis habe ich? Das steht im Property
                         * homedirectory. Da das Property nur 1x gesetzt ist, kann mit Value
                         * ein einzelner Wert angerufen werden. Ohne Value sind Properties immer
                         * eine Collection von Werten! */
                        Console.WriteLine(searcher.CurrentUser["homedirectory"].Value);
                        Console.ReadKey();

                        /* Beispiel 3: Wer hat den Usernamen ABC1234? */
                        Console.WriteLine(searcher.FindCn("ABC1234")?.Displayname);
                        Console.ReadKey();

                        /* Beispiel 4: Welche Schüler sind in der 5CHIF? */
                        List<AdEntry> puils = searcher.FindGroupMembers("5chif");
                        puils.OrderBy(m => m.Displayname).ToList().ForEach(m =>
                        {
                            Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", m.Displayname, m.Lastname, m.Firstname, m.Email, m.IsTeacher);
                        });
                        Console.ReadKey();

                        /* Beispiel 5: Welche Lehrer unterrichten die 5CHIF? Dafür sehen wir uns
                         * die Gruppe Lehrende_5CHIF an. */
                        List<AdEntry> teachers = searcher.FindGroupMembers("Lehrende_5CHIF");
                        teachers.OrderBy(m => m.Displayname).ToList().ForEach(m =>
                        {
                            Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", m.Displayname, m.Lastname, m.Firstname, m.Email, m.IsTeacher);
                        });
                    }
                    else
                    {
                        Console.WriteLine("Login fehlgeschlagen.");
                    }
                }
                catch (AdException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.InnerException?.Message);
                }
            }

            Console.ReadKey();
        }
    }
}
