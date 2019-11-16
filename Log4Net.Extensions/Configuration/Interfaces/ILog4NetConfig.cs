using System;
using System.Collections.Generic;
using System.Text;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;

namespace Log4Net.Extensions.Configuration.Interfaces
{
    public interface ILog4NetConfig
    {
        IEnumerable<Appender> AppendersOfType(AppenderType appenderType);

        Appender AddAppender(Appender appenderToAdd);
    }
}
