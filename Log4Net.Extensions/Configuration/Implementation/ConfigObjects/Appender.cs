using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Implementation.ConfigObjects
{
    /// <summary>
    /// A configured Log4Net Log Appender
    /// </summary>
    public class Appender
    {
        /// <summary>
        /// The <see cref="AppenderType"/> of this <see cref="Appender"/>.
        /// </summary>
        public AppenderType Type { get; set; }

        /// <summary>
        /// The physical folder path of the log files for this <see cref="Appender"/>
        /// </summary>
        public string FolderPath { get; set; }

        /// <summary>
        /// The string value that represents the File Name Mask, including any rolling name appending, that all Log4Net Config Files will match.
        /// </summary>
        public string RollingFileNameMask { get; set; }

        /// <summary>
        /// The string value that represents the File Name Mask, excluding any rolling name appending, that all Log4Net Config Files will match.
        /// </summary>
        public string StaticFileNameMask { get; set; }

        /// <summary>
        /// Determines if the <see cref="Appender"/> will write to one statically named file.
        /// </summary>
        public bool UseStaticLogFileName { get; set; }

        /// <summary>
        /// The Layout of the log record this <see cref="Appender"/> will create.
        /// </summary>
        public AppenderLayout Layout { get; set; }
    }
}
