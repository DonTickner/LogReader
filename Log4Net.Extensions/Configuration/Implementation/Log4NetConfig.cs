using System.Collections.Generic;
using System.Linq;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;
using Log4Net.Extensions.Configuration.Interfaces;

namespace Log4Net.Extensions.Configuration.Implementation
{
    /// <summary>
    /// An object that stores the retrieved configuration properties of a Log4NetConfig file.
    /// </summary>
    public class Log4NetConfig: ILog4NetConfig
    {
        private readonly List<Appender> _appenders;

        public Log4NetConfig()
        {
            _appenders = new List<Appender>();
        }

        #region Methods

        /// <summary>
        /// The collection of <see cref="Appender"/>s that will create Logging records.
        /// </summary>
        public IEnumerable<Appender> Appenders => _appenders.AsEnumerable();

        /// <summary>
        /// Retrieves an enumerable of <see cref="Appender"/>s based on their <see cref="AppenderType"/>
        /// </summary>
        /// <param name="appenderType">The <see cref="AppenderType"/> to retrieve <see cref="Appender"/>s for.</param>
        public IEnumerable<Appender> AppendersOfType(AppenderType appenderType)
        {
            return Appenders.Where(a => a.Type == appenderType);
        }

        /// <summary>
        /// Adds an <see cref="Appender"/> to the collection of <see cref="Appender"/>s associated with this <see cref="Log4NetConfig"/>.
        /// </summary>
        /// <param name="appenderToAdd">The <see cref="Appender"/> to add to <see cref="Log4NetConfig"/>.</param>
        public Appender AddAppender(Appender appenderToAdd)
        {
            if (!_appenders.Contains(appenderToAdd))
            {
                _appenders.Add(appenderToAdd);
            }

            return _appenders[_appenders.IndexOf(appenderToAdd)];
        }

        #endregion
    }
}
