using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Exceptions
{
    /// <summary>
    /// An exception that represents an issue with a physical Log4Net Config file.
    /// </summary>
    public class Log4NetConfigFileAccessErrorException: Exception
    {
        public Log4NetConfigFileAccessErrorException()
        {
        }

        public Log4NetConfigFileAccessErrorException(string message)
            : base(message)
        {
        }

        public Log4NetConfigFileAccessErrorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
