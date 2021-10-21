# ASP.NET Core Applikation (Razor Pages) mit Login

Folgende SDK und NuGet Pakete werden verwendet:

- .NET 5 SDK (in Visual Studio ab 2019 enthalten)
- [Novell.Directory.Ldap.NETStandard](https://www.nuget.org/packages/Novell.Directory.Ldap.NETStandard/)
  zum Abfragen von Daten aus dem Active Directory
- [MailKit](https://www.nuget.org/packages/MailKit/) zum Senden von Mails über den Mailserver der Schule.

## Klonen des Repositories
`git clone https://github.com/schletz/AdLibrary.git`

## Konfigurieren des Abfrageusers

Um sich im Development Mode auch als anderer User anmelden zu können, muss ein Abfrageuser in
[AdLoginDemo.Webapp/appsettings.json](AdLoginDemo/AdLoginDemo.Webapp/appsettings.json) hinterlegt werden (Properties
*Searchuser* und *Searchpass*). Das kann auch der eigene User sein, allerdings darf die Konfiguration
natürlich nie öffentlich geteilt werden.

## Starten der Webapp
Über die Konsole (oder die IDE) kann das Projekt in *AdLoginDemo.Webapp* gestartet werden.

```text
cd AdLoginDemo/AdLoginDemo.Webapp
dotnet watch run
```

Die App ist dann im Browser unter https://localhost:5001 verfügbar.

## Testen des Mailfeatures

Im Testprojekt gibt es die Testklasse *SpgMailClientTests*. Dort ist ein Test angelegt, der das
Senden von Mails über den Schulmailserver demonstriert.

## Ansehen der AD Daten

Um eigene Methoden zu implementieren analysiert man am Besten mit einem LDAP Browser die Inhalte
des Active Directories. Dafür lädt man sich die neueste Version von Softerra LDAP Browser
von https://www.ldapadministrator.com/download.htm#browser

Danach kann eine neue Verbindung mit folgenden Parametern erstellt werden:

![](ldap_browser.png)
