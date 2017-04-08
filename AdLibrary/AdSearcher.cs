/*
 * ADLIBRARY
 * Bibliothek für den Zugriff über die LDAP Schnittstelle des ActiveDirectories der HTL Wien V.
 * Autor: Michael Schletz, April 2017
 * Email: schletz@spengergasse.at
 */
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace AdLibrary
{
    public class AdSearcher : IDisposable
    {
        public const string LdapServer = "LDAP://htl-wien5.schule/OU=SPG,DC=htl-wien5,DC=schule";

        private DirectoryEntry _directory;
        private DirectoryEntry directory
        {
            get
            {
                if (_directory == null) { throw new AdException("Nicht authentifiziert."); }
                return _directory;
            }
            set
            {
                if (value == null) throw new AdException("Interner Fehler");
                _directory = value;
            }
        }
        private DirectorySearcher _searcher;
        private DirectorySearcher searcher
        {
            get
            {
                if (_searcher == null) { throw new AdException("Nicht authentifiziert."); }
                return _searcher;
            }
            set
            {
                if (value == null) throw new AdException("Interner Fehler");
                _searcher = value;
            }
        }

        /// <summary>
        /// Repräsentiert den User, mit dem AD Abfragen durchgeführt werden. Wird durch
        /// Authenticate() gesetzt.
        /// </summary>
        public AdEntry CurrentUser { get; private set; }

        /// <summary>
        /// Setzt die Credentials, mit denen generell Abfragen im AD durchgeführt werden. Dadurch
        /// sieht man mehr oder weniger Properties, je nach Rechte.
        /// </summary>
        /// <param name="username">Der Common Name des Users ("normaler Username")</param>
        /// <param name="password">Das Passwort</param>
        /// <returns>true, wenn ein Login durchgeführt werden konnte. False wenn nicht.</returns>
        public bool Authenticate(string username, string password)
        {
            /* Hier wird nur das AD konfiguriert. Es wird noch nichts über das Netzwerk gesendet.
             * Das wird erst bei der Find Methode gemacht. */
            directory = new DirectoryEntry(LdapServer, username, password, AuthenticationTypes.Secure);
            /* Der Searcher existiert auch nur 1x. Je nach gesetztem Filter liefert er uns die Daten.
             * Er stellt Quasi einen Stream ins AD dar. Ohne eine Einstellung der PageSize liefert
             * er allerdings eine Exception. */
            searcher = new DirectorySearcher(directory) { PageSize = 1000 };
            /* Mit dem übergebenen Usercontext nach dem eigenen User suchen. Hier kommen alle
             * verfügbaren Properties zurück. */
            AdEntry result = FindCn(username);
            /* Wenn der eigene User nicht gelesen werden konnte, war das Login falsch. */
            if (result == null) { return false; }
            CurrentUser = result;
            return true;

        }

        public void Dispose()
        {
            _searcher?.Dispose();
            _directory?.Dispose();
        }

        /// <summary>
        /// Sucht alle Objekte im AD, die einen bestimmten Propertywert haben. Dabei wird ein
        /// LDAP Suchfilter (propertyName=value) konstruiert und abgeschickt.
        /// </summary>
        /// <param name="propertyName">Der Name des LDAP Properties (z. B. cn)</param>
        /// <param name="value">Der zu suchende Wert. Wildcards funktionieren nicht.</param>
        /// <returns></returns>
        public AdEntry[] Find(string propertyName, string value)
        {
            try
            {
                AdEntry[] results;
                /* LDAP Suchfilter bauen. */
                searcher.Filter = "(" + propertyName + "=" + value + ")";
                SearchResultCollection searchResults = searcher.FindAll();
                results = new AdEntry[searchResults.Count];
                int i = 0;

                foreach (SearchResult searchResult in searchResults)
                {
                    ResultPropertyCollection resultProperties = searchResult.Properties;
                    /* Ein AdEntry mit dem dn (distinguishedName) anlegen. Dieser Wert muss
                     * als ID bei jedem Objekt im AD vorhanden sein. */
                    if (resultProperties["distinguishedName"].Count == 0)
                    {
                        throw new AdException("Ein LDAP Datensatz enthält kein Feld distinguishedName.");
                    }
                    AdEntry result = new AdEntry(resultProperties["distinguishedName"][0] as string);

                    foreach (string property in resultProperties.PropertyNames)
                    {
                        AdPropertyValues values = new AdPropertyValues();
                        foreach (object propertyVal in resultProperties[property])
                        {
                            /* Wir mögen keine null Werte */
                            values.Add(propertyVal as string ?? "");
                        }
                        result.Add(property, values);
                    }
                    results[i++] = result;
                }
                return results;
            }
            catch (Exception err)
            {
                /* Login Failed. */
                if (err.HResult == -2147023570) { return new AdEntry[0]; }
                /* Netzwerkfehler. */
                if (err.HResult == -2147016646) { throw new AdException("Der Server ist nicht errichbar."); }
                throw new AdException("Fehler beim Durchsuchen des Verzeichnisses.", err);
            }
        }
        /// <summary>
        /// Sucht nach einem Objekt im AD mit dem übergebenen Common Name (cn) und gibt dieses
        /// zurück.
        /// </summary>
        /// <param name="cn">Der Common Name des Users, der Gruppe, etc.</param>
        /// <returns>Das gefundene Objekt mit allen Properties oder null, wenn kein Objekt gefunden
        /// wurde.</returns>
        public AdEntry FindCn(string cn)
        {
            AdEntry[] results = Find("cn", cn);
            if (results.Length == 0) { return null; }
            return results[0];
        }

        /// <summary>
        /// Liefert alle Mitglieder einer Gruppe. Daber werden aber nur direkte Mitglieder
        /// herausgefunden. Indirekte Mitgliedschaften (User U ist in Gruppe A, die wiederum in B ist)
        /// werden hier nicht aufgelistet.
        /// </summary>
        /// <param name="group">Die zu durchsuchende Gruppe.</param>
        /// <returns>Liste aller Gruppenmitglieder. Falls die Gruppe null ist, wird eine
        /// leere Liste geliefert.</returns>
        public List<AdEntry> FindGroupMembers(AdEntry group)
        {
            if (group == null) { return new List<AdEntry>(); }
            /* Die Mitgliedschaft ist in memberof Property definiert. Achtung: Die Lehrer sind
             * nicht in der Gruppe AlleLehrende, sondern in der OU=Lehrer. */
            return Find("memberof", group.Dn).ToList();
        }

        /// <summary>
        /// Liefert alle Mitglieder einer Gruppe. Daber werden aber nur direkte Mitglieder
        /// herausgefunden. Indirekte Mitgliedschaften (User U ist in Gruppe A, die wiederum in B ist)
        /// werden hier nicht aufgelistet.
        /// </summary>
        /// <param name="cn">Der Common Name der Gruppe (z. B. 5CHIF)</param>
        /// <returns>Liste aller Gruppenmitglieder. Falls die Gruppe null ist oder nicht gefunden wurde, wird eine
        /// leere Liste geliefert.</returns>
        public List<AdEntry> FindGroupMembers(string cn)
        {
            AdEntry group = FindCn(cn);
            if (group == null) { return new List<AdEntry>(); }
            return FindGroupMembers(group);
        }
    }


}
