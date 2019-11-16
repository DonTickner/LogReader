using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Implementation.ConfigObjects
{
    /// <summary>
    /// A helper object of <see cref="XElement"/> Descendant names present in a Log4Net config file.
    /// </summary>
    public static class AppenderDescendants
    {
        /// <summary>
        /// The attribute value for the FilePath
        /// </summary>
        public static string File => "file";

        /// <summary>
        /// The attribute value for the ConversionPattern
        /// </summary>
        public static string ConversionPattern => "conversionpattern";

        /// <summary>
        /// The attribute value for the Header
        /// </summary>
        public static string Header => "header";

        /// <summary>
        /// The attribute value for the Footer
        /// </summary>
        public static string Footer => "footer";

        /// <summary>
        /// The attribute value for the Layout
        /// </summary>
        public static string Layout => "layout";

        /// <summary>
        /// The attribute value for the DatePattern
        /// </summary>
        public static string DatePattern => "datepattern";

        /// <summary>
        /// The attribute value for the StaticLogFileName
        /// </summary>
        public static string StaticLogFileName => "staticlogfilename";

        /// <summary>
        /// The attribute value an <see cref="AppenderLayout"/> paramter.
        /// </summary>
        public static string Param => "param";

        /// <summary>
        /// Returns a collection of all <see cref="Appender"/> Descendant Node Names
        /// </summary>
        public static IEnumerable<string> AllDescendantNames => new List<string> {File, ConversionPattern, Header, Footer, Layout, DatePattern, StaticLogFileName};
    }
}
