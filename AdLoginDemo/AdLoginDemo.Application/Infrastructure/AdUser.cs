using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdLoginDemo.Application.Extensions;

namespace AdLoginDemo.Application.Infrastructure
{
    public class AdUser
    {
        [JsonConstructor]
        public AdUser(
            string firstname, string lastname, string email, string cn, string dn,
            string[] groupMemberhips, string? pupilId = null, string? teacherId = null)
        {
            Firstname = firstname;
            Lastname = lastname;
            Email = email;
            Cn = cn;
            Dn = dn;
            GroupMemberhips = groupMemberhips;
            PupilId = pupilId;
            TeacherId = teacherId;
        }

        public AdUser(LdapEntry entry)
        {
            Firstname = entry.TryGetAttribute("givenName")?.StringValue ?? "";
            Lastname = entry.TryGetAttribute("sn")?.StringValue ?? "";
            Email = entry.TryGetAttribute("mail")?.StringValue ?? "";
            Cn = entry.TryGetAttribute("cn")?.StringValue ?? "";
            Dn = entry.Dn;
            PupilId = entry.TryGetAttribute("employeeid")?.StringValue;
            TeacherId = entry.TryGetAttribute("description")?.StringValue;
            GroupMemberhips = entry.TryGetAttribute("memberof")?.StringValueArray ?? Array.Empty<string>();
        }

        public string Firstname { get;  }
        public string Lastname { get; }
        public string Email { get; }
        public string Cn { get; }
        public string Dn { get; }
        public string[] GroupMemberhips { get; }
        public string? PupilId { get; }
        public string? TeacherId { get; }
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

        public string ToJson() => System.Text.Json.JsonSerializer.Serialize(this);
        public static AdUser? FromJson(string json) => System.Text.Json.JsonSerializer.Deserialize<AdUser>(json);
    }
}
