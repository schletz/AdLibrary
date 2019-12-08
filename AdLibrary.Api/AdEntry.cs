using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AdLibrary.Api
{
    /// <summary>
    /// Repräsentiert ein Objekt im Active Directory. Es ist ein Dictionary, welches einen Eintrag
    /// für jedes Property hat. Da Properties auch mehrere Werte haben können (z. B. membership),
    /// sind die Daten in einer Liste vom Typ AdPropertyValues gespeichert.
    /// Einige Standardattribute werden als Property extra definieren.
    /// </summary>
    public class AdEntry : Dictionary<string, AdPropertyValues>
    {
        public string Dn { get; }
        /// <summary>
        /// Common Name, ist der "normale" Loginname, also ABC12345 bei Schülern.
        /// </summary>
        public string Cn => getPropertyVal("cn");
        public string Firstname => getPropertyVal("givenname");
        public string Lastname => getPropertyVal("sn");
        public string Displayname => getPropertyVal("displayname");
        public string Email => getPropertyVal("mail");
        /// <summary>
        /// Telefonnummer in der Schule; wird nur bei Lehrern gesetzt.
        /// </summary>
        public string Phone => getPropertyVal("ipphone");
        /// <summary>
        /// Personen-ID aus dem alten Katalogsystem. Wird nur bei Lehrern gesetzt.
        /// </summary>
        public string PersNr => getPropertyVal("extensionattribute4");
        /// <summary>
        /// Prüft, ob der User ein Lehrer ist.
        /// </summary>
        public bool IsTeacher => Dn.IndexOf("OU=Lehrer") != -1;
        public AdEntry(string dn)
        {
            Dn = dn;
        }

        public AdEntry(string dn, ResultPropertyCollection results) : this(dn)
        {
            foreach (string property in results.PropertyNames)
            {
                AdPropertyValues values = new AdPropertyValues();
                foreach (object propertyVal in results[property])
                {
                    values.Add(propertyVal as string ?? "");
                }
                this.Add(property, values);
            }
        }
        /// <summary>
        /// Liefert den Eintrag als XML String.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            /* Nur beim XML Writer können wir CheckCharacters = false setzen. Die UTF8 Zeichen bei
             * manchen Werten lösen sonst bei XElement eine Exception aus. */
            XmlWriterSettings xws = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                CheckCharacters = false,
                Indent = true
            };
            StringBuilder sb = new StringBuilder();
            using (XmlWriter xw = XmlWriter.Create(sb, xws))
            {
                XElement doc = new XElement("AdEntry",
                    new XAttribute("dn", Dn),
                    from prop in this.OrderBy(a => a.Key)
                    select new XElement("Property",
                        new XAttribute("name", prop.Key),
                        from val in prop.Value.OrderBy(v => v)
                        select new XElement("Value", val)));
                doc.WriteTo(xw);
            }
            return sb.ToString();
        }

        private string getPropertyVal(string propertyName)
        {
            try
            {
                return this[propertyName].Value;
            }
            catch { return ""; }
        }
    }
}
