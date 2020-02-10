namespace Log4Net.Extensions.Configuration.Implementation.LogObjects
{
    /// <summary>
    /// Represents a Field within a Log4Net Log File
    /// </summary>
    public class LogLineField
    {
        /// <summary>
        /// The name of the field, taken from the <see cref="PatternLayout"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="char"/> that marks the end of the field within the <see cref="raw line"/>
        /// </summary>
        public char EndingCharacter { get; set; }

        /// <summary>
        /// The number of characters for this field, if the <see cref="Type"/> is <see cref="LogLineType.FixedWidth"/>
        /// </summary>
        public int FixedWidth { get; set; }

        /// <summary>
        /// The minimum width for this field.
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// The <see cref="LogLineType"/> for this field.
        /// </summary>
        public LogLineType Type { get; set; }

        /// <summary>
        /// The content of the specific field, if read from the log line.
        /// </summary>
        public string Content { get; set; }
    }
}
