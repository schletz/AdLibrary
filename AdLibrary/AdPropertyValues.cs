using System;
using System.Collections.Generic;

namespace AdLibrary
{
    /// <summary>
    /// Stellt eine Liste von strings dar, die die einzelnen Werte eines Properties aufnimmt.
    /// </summary>
    public class AdPropertyValues : List<string>
    {
        /// <summary>
        /// Damit einfacher auf Werte, die sicher nur 1x vorhanden sind (dn, firstname, ...) 
        /// zugegriffen werden kann.
        /// </summary>
        public string Value
        {
            get
            {
                if (Count == 1) { return this[0]; }
                else { return String.Join(" ", this); }
            }
        }
    }
}
