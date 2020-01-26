using System;
using System.Collections.Generic;
using System.Text;
using Log4Net.Extensions.Configuration.Implementation.LogObjects;

namespace Log4Net.Extensions.Configuration.Implementation.ConfigObjects
{
    /// <summary>
    /// The specific field make up of the physical log line an <see cref="Appender"/> will produce.
    /// </summary>
    public class AppenderConversionPattern
    {
        /// <summary>
        /// The raw text line from the Log4Net config file that represents this conversion pattern.
        /// </summary>
        public string RawLine { get; set; }

        /// <summary>
        /// The number of individual fields that proceeds the message content.
        /// Note: Assumption is that no fields follow the message field.
        /// </summary>
        public int NumberOfFieldsBeforeMessageField { get; set; }

        /// <summary>
        /// The string value that separates all fields that proceed the message field.
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// The ordered enumerable of Field Names present in the <see cref="AppenderConversionPattern"/>
        /// </summary>
        public IEnumerable<LogLineField> Fields { get; set; }
    }
}
