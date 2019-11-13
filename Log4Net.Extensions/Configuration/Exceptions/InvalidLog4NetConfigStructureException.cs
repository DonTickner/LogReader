using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Exceptions
{
    /// <summary>
    /// An exception that represents an issue with the internal structure of a Log4Net Config file.
    /// </summary>
    public class InvalidLog4NetConfigStructureException: Exception
    {
        public InvalidLog4NetConfigStructureException()
        {
        }

        public InvalidLog4NetConfigStructureException(string message)
            : base(message)
        {
        }

        public InvalidLog4NetConfigStructureException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
