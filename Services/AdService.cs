using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdLoginDemo.Services
{
    public enum AdUserRole { Other = 0, Pupil, Teacher, Administration, Management }

    /// <summary>
    /// Klasse für den AD Zugriff. Stellt die Loginfunktion und abfragemethoden bereit.
    /// </summary>
    public class AdService : IDisposable
    {
        private readonly LdapConnection _connection;
        private bool isDisposed;

        public static string Hostname { get; } = "ldap.spengergasse.at";
        public static string Domain { get; } = "htl-wien5.schule";
        public static string BaseDn { get; } = "OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule";
        public static (string user, string pass) Searchuser { get; } = ("Hier einen Suchuser angeben.", "Das Passwort.");

        public AdUser CurrentUser { get; }

        /// <summary>
        /// Liefert eine aktive Serverbindung mit den Rechten des angemeldeten Benutzers.
        /// </summary>
        protected static LdapConnection GetConnection(string cn, string password)
        {
            var connection = new LdapConnection
            {
                SecureSocketLayer = true
            };

            try
            {
                try { connection.Connect(Hostname, 636); }
                catch { throw new AdException($"Der Anmeldeserver ist nicht erreichbar."); }
                try { connection.Bind($"{cn}@{Domain}", password); }
                catch { throw new AdException($"Ungültiger Benutzername oder Passwort."); }

                return connection;
            }
            catch { connection.Disconnect(); throw; }
        }

        public static AdService Login() => Login(Searchuser.user, Searchuser.pass);

        /// <summary>
        /// Führt ein Login am Active Directory durch.
        /// </summary>
        public static AdService Login(string cn, string password)
        {
            var connection = GetConnection(cn, password);
            var result = connection.Search(
                        BaseDn,
                        LdapConnection.ScopeSub,
                        $"(&(objectClass=user)(objectClass=person)(cn={cn}))",
                        new string[] { "cn", "givenName", "sn", "mail", "employeeid", "memberof" },
                        false);
            LdapEntry loginUser = result.FirstOrDefault() ?? throw new AdException("Der Benutzer wurde nicht gefunden.");
            var user = new AdUser(loginUser);
            return new AdService(user, connection);
        }

        private AdService(AdUser currentUser, LdapConnection connection)
        {
            _connection = connection;
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public string[] GetClasses()
        {
            var results = Search("(objectClass=group)", "OU=Klassen,OU=Mailaktivierte Sicherheitsgruppen,OU=Gruppen,OU=SPG,DC=htl-wien5,DC=schule");
            return results
                .Select(r => r.TryGetAttribute("cn")?.StringValue ?? "")
                .Where(r => !string.IsNullOrEmpty(r))
                .OrderBy(r => r.Length < 5 ? "" : r.Substring(2, 3))
                .ThenBy(r => r)
                .ToArray();
        }

        public AdUser[]? GetPupils(string schoolclass)
        {
            try
            {
                var classGroup = $"CN={schoolclass},OU=Klassen,OU=Mailaktivierte Sicherheitsgruppen,OU=Gruppen,OU=SPG,DC=htl-wien5,DC=schule";
                var members = Search($"(&(objectClass=user)(objectClass=person)(memberOf={classGroup}))");
                return members.Select(m => new AdUser(m)).ToArray();
            }
            catch { return null; }
        }

        public AdUser[]? GetTeachers(string schoolclass)
        {
            try
            {
                var classGroup = $"CN=lehrende_{schoolclass},OU=Klassenlehrer,OU=Mailaktivierte Sicherheitsgruppen,OU=Gruppen,OU=SPG,DC=htl-wien5,DC=schule";
                var members = Search($"(&(objectClass=user)(objectClass=person)(memberOf={classGroup}))");
                return members.Select(m => new AdUser(m)).ToArray();
            }
            catch { return null; }
        }

        public AdUser? GetKv(string schoolclass)
        {
            try
            {
                var classGroup = $"CN=KV_{schoolclass},OU=KV,OU=Mailaktivierte Sicherheitsgruppen,OU=Gruppen,OU=SPG,DC=htl-wien5,DC=schule";
                var kv = Search($"(&(objectClass=user)(objectClass=person)(memberOf={classGroup}))");
                return new AdUser(kv[0]);
            }
            catch { return null; }
        }

        private List<LdapEntry> Search(string searchFilter) =>
            Search(searchFilter, BaseDn);

        private List<LdapEntry> Search(string searchFilter, string baseDn) =>
            Search(searchFilter, baseDn, new string[0]);

        private List<LdapEntry> Search(string searchFilter, string[] additionalAttributes) =>
            Search(searchFilter, BaseDn, additionalAttributes);

        private List<LdapEntry> Search(string searchFilter, string baseDn, string[] additionalAttributes)
        {
            var attributes =
                new string[] { "cn", "givenName", "sn", "mail", "employeeid", "memberof" }
                .Concat(additionalAttributes)
                .ToArray();

            return _connection.Search(
                        baseDn,
                        LdapConnection.ScopeSub,
                        searchFilter,
                        attributes,
                        false).ToList();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) { return; }
            if (disposing)
            {
                _connection.Disconnect();
                _connection.Dispose();
            }
            isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}