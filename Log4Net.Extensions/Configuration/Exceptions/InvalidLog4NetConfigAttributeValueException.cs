using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Exceptions
{
    public class InvalidLog4NetConfigAttributeValueException: Exception
    {
        public InvalidLog4NetConfigAttributeValueException()
        {
        }

        public InvalidLog4NetConfigAttributeValueException(string message)
            : base(message)
        {
        }

        public InvalidLog4NetConfigAttributeValueException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
