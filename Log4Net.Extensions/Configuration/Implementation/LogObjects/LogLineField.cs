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
        /// The raw content of the field, taken from the log file.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The <see cref="char"/> that marks the end of the field within the <see cref="Content"/>
        /// </summary>
        public char EndingCharacter { get; set; }

        /// <summary>
        /// The number of characters for this field, if the <see cref="Type"/> is <see cref="LogLineType.FixedWidth"/>
        /// </summary>
        public long FixedWidth { get; set; }

        /// <summary>
        /// The minimum width for this field.
        /// </summary>
        public long MinWidth { get; set; }

        /// <summary>
        /// The <see cref="LogLineType"/> for this field.
        /// </summary>
        public LogLineType Type { get; set; }
    }
}
