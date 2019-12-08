using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdLibrary.Api
{
    public class AdException : Exception
    {
        public AdException() { }
        public AdException(string message) : base(message) { }
        public AdException(string message, Exception inner) : base(message, inner) { }
    }
    public class LoginException : AdException
    {
        public LoginException() { }
        public LoginException(string message) : base(message) { }
        public LoginException(string message, Exception inner) : base(message, inner) { }
    }
    public class NetworkException : AdException
    {
        public NetworkException() { }
        public NetworkException(string message) : base(message) { }
        public NetworkException(string message, Exception inner) : base(message, inner) { }
    }
}
