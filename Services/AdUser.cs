using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdLoginDemo.Services
{
    /// <summary>
    /// Speichert die Daten eines Benutzers im Active Directory.
    /// </summary>
    public class AdUser
    {
        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Email { get; set; } = "";
        public string Cn { get; set; } = "";
        public string Dn { get; set; } = "";
        public string? PupilId { get; set; }
        public string[] GroupMemberhips { get; set; } = new string[0];
        public string[] Classes => GroupMemberhips
            .Where(v => v.Contains("OU=Klassen,") || v.Contains("OU=Klassenlehrer,"))
            .Select(v =>
            {
                var m = Regex.Match(v, "CN=(lehrende_)?([^,]+)", RegexOptions.IgnoreCase);
                return m.Success ? m.Groups[2].Value.ToUpper().Trim() : v;
            })
            .OrderBy(c => c.Length < 5 ? "" : c.Substring(2, 3))
            .ThenBy(c => c)
            .ToArray();

        public AdUserRole Role
        {
            get
            {
                if (GroupMemberhips.Any(g => g.Contains("CN=AV,OU=Schulleitung"))) { return AdUserRole.Management; }
                return
                    Dn.Contains("OU=Verwaltung") ? AdUserRole.Administration :
                    Dn.Contains("OU=Lehrer") ? AdUserRole.Teacher :
                    Dn.Contains("OU=Schueler") ? AdUserRole.Pupil :
                    AdUserRole.Other;
            }
        }
        public AdUser()
        {

        }

        public AdUser(AdUser other)
        {
            Firstname = other.Firstname;
            Lastname = other.Lastname;
            Email = other.Email;
            Cn = other.Cn;
            Dn = other.Dn;
            PupilId = other.PupilId;
            GroupMemberhips = other.GroupMemberhips;
        }

        public AdUser(LdapEntry entry)
        {
            Firstname = entry.TryGetAttribute("givenName")?.StringValue ?? "";
            Lastname = entry.TryGetAttribute("sn")?.StringValue ?? "";
            Email = entry.TryGetAttribute("mail")?.StringValue ?? "";
            Cn = entry.TryGetAttribute("cn")?.StringValue ?? "";
            Dn = entry.Dn;
            PupilId = entry.TryGetAttribute("employeeid")?.StringValue;
            GroupMemberhips = entry.TryGetAttribute("memberof")?.StringValueArray ?? new string[0];
        }
    }
}
