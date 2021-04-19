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
    public class AdService
    {
        public static string Hostname { get; } = "ldap.spengergasse.at";
        public static string Domain { get; } = "htl-wien5.schule";
        public static string BaseDn { get; } = "OU=Benutzer,OU=SPG,DC=htl-wien5,DC=schule";
        public static (string user, string pass) Searchuser { get; } = ("Hier einen Suchuser angeben.", "Das Passwort.");

        public AdUser CurrentUser { get; }
        private readonly string encryptedPassword;
        private readonly byte[] key;


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
#if DEBUG
                try { connection.Bind($"{Searchuser.user}@{Domain}", Searchuser.pass); }
                catch { throw new AdException($"Ungültiger Benutzername oder Passwort."); }
#else
                try { connection.Bind($"{cn}@{Domain}", password); }
                catch { throw new AdException($"Ungültiger Benutzername oder Passwort."); }
#endif
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
            using var connection = GetConnection(cn, password);
            try
            {
                var result = connection.Search(
                            BaseDn,
                            LdapConnection.ScopeSub,
                            $"(&(objectClass=user)(objectClass=person)(cn={cn}))",
                            new string[] { "cn", "givenName", "sn", "mail", "employeeid", "memberof" },
                            false);
                LdapEntry loginUser = result.FirstOrDefault() ?? throw new AdException("Der Benutzer wurde nicht gefunden.");
                var user = new AdUser(loginUser);
                var key = GenerateKey(256);

                return new AdService(user, password.Encrypt(key), key);
            }
            catch
            {
                throw;
            }
            finally
            {
                connection.Disconnect();
            }
        }

        private AdService(AdUser currentUser, string encryptedPassword, byte[] key)
        {
            CurrentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            this.encryptedPassword = encryptedPassword ?? throw new ArgumentNullException(nameof(encryptedPassword));
            this.key = key ?? throw new ArgumentNullException(nameof(key));
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
            using var connection = AdService.GetConnection(CurrentUser.Cn, encryptedPassword.Decrypt(key));
            try
            {
                return connection.Search(
                            baseDn,
                            LdapConnection.ScopeSub,
                            searchFilter,
                            attributes,
                            false).ToList();
            }
            catch { throw; }
            finally { connection.Disconnect(); }
        }

        private static byte[] GenerateKey(int bits = 128)
        {
            byte[] key = new byte[bits / 8];
            using (System.Security.Cryptography.RandomNumberGenerator rnd = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rnd.GetBytes(key);
            }
            return key;
        }
    }
}
