# AdLibrary

C# Bibliothek für den Zugriff auf das Active Directory (LDAP). Der Zugriff ist nur aus dem
Schulnetzwerk oder über eine VPN Verbindung möglich, da direkt auf *htl-wien5.schule*
(private IP Adresse) zugegriffen wird.

Der Zugriff erfolgt über die Ports 389 TCP und UDP.

Beim Starten des Visual Studio Projektes muss das Projekt *AdLibrary.Testapp* als Start up Project 
definiert werden, da sonst die DLL starten würde.

## Dependencies

Die Bibliothek verwendet .NET Standard 2.0. Das Paket *System.DirectoryServices* wird über NuGet
geladen.

### Ausführen des Testprogrammes

```text
...\AdLibrary.Testapp>dotnet run
```

### Erstellen der DLL zur Einbindung in andere Projekte

Dieser Befehl erstellt einen Ordner *publish* und kopiert alle Abhängigkeiten in dieses Verzeichnis.
Dieses Verzeichnis kann danach in anderen Projekten verwendet werden. Natürlich kann auch ohne
Kompilieren die Projektdatei als *ProjectReference* hinzugefügt werden, um ein Debuggen zu ermöglichen.

```text
...\AdLibrary.Api>dotnet publish -c Release -o ./publish
```

## Beispielprogramm: Abfrage des Active Directories

In der Testapp [Program.cs](AdLibrary.Testapp/Program.cs) werden Beispielabfragen ausgegeben.
