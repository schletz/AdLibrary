using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdLibrary
{
    public class AdException : Exception
    {
        public AdException() { }
        public AdException(string message) : base(message) { }
        public AdException(string message, Exception inner) : base(message, inner) { }
    }
}
