using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AdLibrary
{
    /// <summary>
    /// Repräsentiert ein Objekt im Active Directory. Es ist ein Dircionary, welches einen Eintrag
    /// für jedes Property hat. Da Properties auch mehrere Werte haben können (z. B. membership),
    /// sind die Daten in einer Liste vom Typ AdPropertyValues gespeichert.
    /// </summary>
    public class AdEntry : Dictionary<string, AdPropertyValues>
    {
        /* Einige Standardattribute als Property extra definieren. */
        public string Dn { get; private set; }
        /* Der "normale" Loginname, also ABC12345 bei Schülern. */
        public string Cn { get { return getPropertyVal("cn"); } }
        public string Firstname { get { return getPropertyVal("givenname"); } }
        public string Lastname { get { return getPropertyVal("sn"); } }
        public string Displayname { get { return getPropertyVal("displayname"); } }
        public string Email { get { return getPropertyVal("mail"); } }
        /* Nur bei Lehrern gesetzt. */
        public string Phone { get { return getPropertyVal("ipphone"); } }
        /* Nur bei Lehrern gesetzt. */
        public string PersNr { get { return getPropertyVal("extensionattribute4"); } }

        /* Aufgrund des DN wird geprüft, ob der User unter der OU Lehrer angelegt wurde. */
        public bool IsTeacher
        {
            get
            {
                return Dn.IndexOf("OU=Lehrer") != -1;
            }
        }
        public AdEntry(string dn)
        {
            Dn = dn;
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
