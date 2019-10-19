# AdLibrary
C# Bibliothek für den Zugriff auf das Active Directory (LDAP). Der Zugriff ist nur aus dem 
Schulnetzwerk oder über eine VPN Verbindung möglich, da direkt auf htl-wien5.schule 
(private IP Adresse) zugegriffen wird.

Der Zugriff erfolgt über die Ports 389 TCP und UDP.

Beim Starten des Visual Studio Projektes muss das Projekt *AdLibrary.Testapp* als Start up Project 
definiert werden, da sonst die DLL starten würde.

## Kompatibilität
Das verwendete Paket *System.DirectoryServices* ist nicht sehr gut von Microsoft gepflegt, es wurde 
lediglich aus dem .NET Framework mit alter Codebasis migriert. Dies zeigt sich dadurch, dass die 
Methoden noch nicht asynchron sind.

Diese Bibliothek benötigt mindestens .NET Core 3.0, läuft aber nur unter Windows, da von diesem Paket
verwendete Abhängigkeiten nur für Windows zur Verfügung stehen.

## Verwendung in ASP.NET Core
Für die Verwendung in anderen Programmen und in ASP.NET Core wird in Visual Studio ein Releasebuild
erzeugt. Die DLL Datei liegt danach in *bin/Release/netcoreapp3.0*. Diese kann in andere Projekte
kopiert und darauf als Dependency verwiesen werden.


```C#
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
            Console.Clear();
            Console.WriteLine(searcher.CurrentUser);
            Console.ReadKey();

            /* Beispiel 2: Welches Homeverzeichnis habe ich? Das steht im Property
             * homedirectory. Da das Property nur 1x gesetzt ist, kann mit Value
             * ein einzelner Wert angerufen werden. Ohne Value sind Properties immer
             * eine Collection von Werten! */
            Console.Clear();
            Console.WriteLine(searcher.CurrentUser["homedirectory"].Value);
            Console.ReadKey();

            /* Beispiel 3: Wer hat den Usernamen ABC1234? */
            Console.Clear();
            Console.WriteLine(searcher.FindCn("ABC1234")?.Displayname);
            Console.ReadKey();

            /* Beispiel 4: Welche Schüler sind in der 5CHIF? */
            Console.Clear();
            List<AdEntry> pupils = searcher.FindGroupMembers("5CHIF");
            pupils.OrderBy(p => p.Displayname).ToList().ForEach(p =>
            {
                Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", p.Displayname, p.Lastname, p.Firstname, p.Email, p.IsTeacher);
            });
            Console.ReadKey();

            /* Beispiel 5: Welche Lehrer unterrichten die 5CHIF? Dafür sehen wir uns
             * die Gruppe Lehrende_5CHIF an. */
            Console.Clear();
            List<AdEntry> teachers = searcher.FindGroupMembers("Lehrende_5CHIF");
            teachers.OrderBy(t => t.Displayname).ToList().ForEach(t =>
            {
                Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", t.Displayname, t.Lastname, t.Firstname, t.Email, t.IsTeacher);
            });
            Console.ReadKey();

            /* Beispiel 6: Suche alle Personenobjekte in 
             * OU=Lehrer,OU=Automatisch gewartete Benutzer,OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule */
            Console.Clear();
            AdEntry[] allTeachers = searcher.Find("objectclass", "person", "OU=Lehrer,OU=Automatisch gewartete Benutzer,OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule");
            allTeachers.OrderBy(at=>at.Displayname).ToList().ForEach(at=>
            {
                Console.WriteLine("{0}: {1} {2}, {3}, Lehrer: {4}", at.Displayname, at.Lastname, at.Firstname, at.Email, at.IsTeacher);
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
```
