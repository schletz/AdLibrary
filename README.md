# AD Abfrage und Versand von Mails aus der Schule

Folgende NuGet Pakete werden verwendet:

- [Novell.Directory.Ldap.NETStandard](https://www.nuget.org/packages/Novell.Directory.Ldap.NETStandard/)
  zum Abfragen von Daten aus dem Active Directory
- [MailKit](https://www.nuget.org/packages/MailKit/) zum Senden von Mails über den Mailserver der Schule.

## Konfigurieren des Abfrageusers

Damit nicht der eigene (angemeldete) User die Abfragen durchführen muss, kann das Property
*Searchuser* in der Klasse [AdService](Services/AdService.cs) gesetzt werden. Dafür muss allerdings
ein solcher Benutzer beim ZID beantragt mit entsprechender Begründung werden.

