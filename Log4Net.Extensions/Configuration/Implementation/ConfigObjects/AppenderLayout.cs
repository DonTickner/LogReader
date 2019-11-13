using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Implementation.ConfigObjects
{
    /// <summary>
    /// The layout of a Log record that an <see cref="Appender"/> will produce.
    /// </summary>
    public class AppenderLayout
    {
        /// <summary>
        /// The Header Line that will proceed every new log session.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// The Footer Line that will end every log session.
        /// </summary>
        public string Footer { get; set; }

        /// <summary>
        /// The breakup of the physical log line.
        /// </summary>
        public AppenderConversionPattern ConversionPattern { get; set; }
    }
}
