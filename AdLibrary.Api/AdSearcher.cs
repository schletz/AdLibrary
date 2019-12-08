/*
 * ADLIBRARY
 * Bibliothek für den Zugriff über die LDAP Schnittstelle des ActiveDirectories der HTL Wien V.
 * Autor: Michael Schletz, April 2017
 * Email: schletz@spengergasse.at
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.DirectoryServices;

namespace AdLibrary.Api
{
    /// <summary>
    /// Durchsucht das AD der HTL Wien V. Dies ist nur im Schulnetzwerk oder über VPN mögoich,
    /// da der Domaincontroller nur eine private IP Adresse hat.
    /// Außerdem muss Port 389 TCP und UDP nach außen hin freigeschatgen sein.
    /// </summary>
    public sealed class AdSearcher : IDisposable
    {
        // LDAP muss großgeschrieben sein, sonst gibt es einen "unbekannten Fehler".
        public string LdapServer => "LDAP://htl-wien5.schule";
        public string DefaultPath => "OU=SPG,DC=htl-wien5,DC=schule";
        /// <summary>
        /// Die Anzahl der Datensätze, die vom LDAP Server bei einer Abfrage abgerufen werden.
        /// </summary>
        public int PageSize => 1000;

        /// <summary>
        /// Repräsentiert den User, mit dem AD Abfragen durchgeführt werden. Wird durch
        /// <see cref="Authenticate"/> gesetzt.
        /// </summary>
        public AdEntry? CurrentUser { get; private set; }

        private string? _username;
        private DirectoryEntry? _directory;
        private DirectoryEntry directory => _directory ?? throw new LoginException("Not logged in.");
        private DirectorySearcher? _searcher;
        private DirectorySearcher searcher => _searcher ?? throw new LoginException("Not logged in.");

        /// <summary>
        /// Setzt die Credentials, mit denen generell Abfragen im AD durchgeführt werden. Dadurch
        /// sieht man mehr oder weniger Properties, je nach Rechte.
        /// </summary>
        /// <param name="username">Der Common Name des Users ("normaler Username")</param>
        /// <param name="password">Das Passwort</param>
        /// <returns>true, wenn ein Login durchgeführt werden konnte. False wenn nicht.</returns>
        public void Authenticate(string username, string password)
        {
            _username = username;
            // Hier wird nur das AD konfiguriert. Es wird noch nichts über das Netzwerk gesendet.
            // Das wird erst bei der Find Methode gemacht.
            string path = $"{LdapServer}/{DefaultPath}";
            _directory = new DirectoryEntry(path, username, password, AuthenticationTypes.Secure);
            // Der Searcher existiert auch nur 1x. Je nach gesetztem Filter liefert er uns die Daten.
            // Er stellt Quasi einen Stream ins AD dar. Ohne eine Einstellung der PageSize liefert
            // er allerdings eine Exception.
            _searcher = new DirectorySearcher(_directory) { PageSize = PageSize };
            // Mit dem übergebenen Usercontext nach dem eigenen User suchen. Hier kommen alle
            // verfügbaren Properties zurück.
            CurrentUser = FindCn(username);
        }

        public void Dispose()
        {
            _searcher?.Dispose();
            _directory?.Dispose();
        }

        /// <summary>
        /// Sucht alle Objekte im AD unter OU=SPG,DC=htl-wien5,DC=schule, die einen bestimmten Property Wert haben. Dabei wird ein
        /// LDAP Suchfilter (propertyName=value) konstruiert und abgeschickt.
        /// </summary>
        /// <param name="propertyName">Der Name des LDAP Properties (z. B. cn)</param>
        /// <param name="value">Der zu suchende Wert. Wildcards funktionieren nicht.</param>
        /// <returns>Die gefundenen AD Objekte. Wird nichts gefunden, wird ein leeres Array geliefert.</returns>
        public IEnumerable<AdEntry> Find(string propertyName, string value) => Find(propertyName, value, DefaultPath);

        /// <summary>
        /// Sucht alle Objekte im AD, die einen bestimmten Property Wert haben. Dabei wird ein
        /// LDAP Suchfilter (propertyName=value) konstruiert und abgeschickt.
        /// </summary>
        /// <exception cref="AdException">Wird ausgelöst, wenn der Server nicht erreichbar, die
        /// Suchabfrage nicht durchführbar oder
        /// ein Eintrag kein Property distinguishedName enthält. </exception>
        /// <param name="propertyName">Der Name des LDAP Properties (z. B. cn)</param>
        /// <param name="value">Der zu suchende Wert. Wildcards funktionieren nicht.</param>
        /// <param name="path">Der Suchfad, von dem die Suche aus beginnt (z. B. OU=SPG,DC=htl-wien5,DC=schule)</param>
        /// <returns>Die gefundenen AD Objekte. Wird nichts gefunden, wird ein leeres Array geliefert.</returns>
        public IEnumerable<AdEntry> Find(string propertyName, string value, string path)
        {
            try
            {
                // LDAP Suchfilter bauen.
                directory.Path = $"{LdapServer}/{path}";
                searcher.Filter = $"({propertyName}={value})";
                SearchResultCollection searchResults = searcher.FindAll();
                // SearchResultCollection ist noch eine alte, nicht generische Collection. Daher muss
                // ein Cast von object auf SearchResult durchgeführt werden.
                return from sr in searchResults.Cast<SearchResult>()
                       let resultProperties = sr.Properties
                       select new AdEntry(
                           resultProperties["distinguishedName"][0] as string ?? throw new AdException("Missing field distinguishedName."),
                           resultProperties ?? throw new AdException("Missing result properties"));

            }
            catch (AdException)
            {
                throw;
            }
            catch (Exception err) when (err.HResult == -2147023570)
            {
                throw new LoginException("Login failed.", err);
            }
            catch (Exception err) when (err.HResult == -2147016646)
            {
                throw new NetworkException("Network error.", err);
            }
            catch (Exception err)
            {
                throw new AdException("An unknown error occurred while searching the directory.", err);
            }
        }
        /// <summary>
        /// Sucht nach einem Objekt im AD mit dem übergebenen Common Name (cn) und gibt dieses
        /// zurück.
        /// </summary>
        /// <param name="cn">Der Common Name des Users, der Gruppe, etc.</param>
        /// <returns>Das gefundene Objekt mit allen Properties oder null, wenn kein Objekt gefunden
        /// wurde.</returns>
        public AdEntry FindCn(string cn) => Find("cn", cn).FirstOrDefault();

        /// <summary>
        /// Liefert alle Mitglieder einer Gruppe. Daber werden aber nur direkte Mitglieder
        /// herausgefunden. Indirekte Mitgliedschaften (User U ist in Gruppe A, die wiederum in B ist)
        /// werden hier nicht aufgelistet.
        /// </summary>
        /// <param name="group">Die zu durchsuchende Gruppe.</param>
        /// <returns>Liste aller Gruppenmitglieder. Falls die Gruppe null ist, wird eine
        /// leere Liste geliefert.</returns>
        public IEnumerable<AdEntry> FindGroupMembers(AdEntry group) => Find("memberof", group.Dn);

        /// <summary>
        /// Liefert alle Mitglieder einer Gruppe. Daber werden aber nur direkte Mitglieder
        /// herausgefunden. Indirekte Mitgliedschaften (User U ist in Gruppe A, die wiederum in B ist)
        /// werden hier nicht aufgelistet.
        /// </summary>
        /// <param name="cn">Der Common Name der Gruppe (z. B. 5CHIF)</param>
        /// <returns>Liste aller Gruppenmitglieder. Falls die Gruppe null ist oder nicht gefunden wurde, wird eine
        /// leere Liste geliefert.</returns>
        public IEnumerable<AdEntry> FindGroupMembers(string cn) => FindGroupMembers(FindCn(cn));
    }
}
