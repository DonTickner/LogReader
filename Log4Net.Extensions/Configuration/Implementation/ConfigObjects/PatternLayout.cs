using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Log4Net.Extensions.Configuration.Implementation.ConfigObjects
{
    public static class PatternLayout
    {
        public static string[] AllConversionPatterns
        {
            get { return ConversionPatterns
                .Concat(MessageConversionPatterns)
                .Concat(AppDomainConversionPatterns)
                .ToArray(); }
        }

        /// <summary>
        /// A <see cref="string"/>[] of all ConversionPattern notations that Log4Net accepts.
        /// </summary>
        public static readonly string[] ConversionPatterns = new[]
        {
            "a"
            ,"appdomain"
            ,"aspnet-cache"
            ,"aspnet-context"
            ,"aspnet-request"
            ,"aspnet-session"
            ,"c"
            ,"C"
            ,"class"
            ,"d"
            ,"date"
            ,"exception"
            ,"F"
            ,"file"
            ,"identity"
            ,"l"
            ,"L"
            ,"location"
            ,"level"
            ,"line"
            ,"logger"
            ,"M"
            ,"mdc"
            ,"method"
            ,"n"
            ,"newline"
            ,"ndc"
            ,"p"
            ,"P"
            ,"properties"
            ,"property"
            ,"r"
            ,"stacktrace"
            ,"stacktracedetail"
            ,"t"
            ,"timestamp"
            ,"thread"
            ,"type"
            ,"u"
            ,"username"
            ,"utcdate"
            ,"w"
            ,"x"
            ,"X"
            ,"%"
        };

        /// <summary>
        /// A string[] of all ConversionPatterns for the 'Message' element.
        /// </summary>
        public static readonly string[] MessageConversionPatterns = new[]
        {
            "m"
            , "message"
        };

        /// <summary>
        /// A string[] of all ConversionPatterns for the 'AppDomain' element.
        /// </summary>
        public static readonly string[] AppDomainConversionPatterns = new[]
        {
            "a"
            , "appdomain"
        };

        /// <summary>
        /// Receives a <see cref="string"/> and strips all KeyValue declarations represented as '{KeyValue}'.
        /// </summary>
        /// <param name="stringToClean">The <see cref="string"/> to be cleaned.</param>
        public static string CleanStringOfAllKeyValues(string stringToClean)
        {
            string cleanedString = Regex.Replace(stringToClean, @"\{[^}]*\}", string.Empty);

            return cleanedString;
        }

        /// <summary>
        /// Receives a <see cref="string"/> and strips all unrequired identifiers from the value.
        /// </summary>
        /// <param name="stringToClean">The <see cref="string"/> to be cleaned.</param>
        public static string CleanStringOfAllUnnecessaryIdentifiers(string stringToClean)
        {
            string cleanedString = Regex.Replace(stringToClean,"[^a-zA-Z0-9.{} ]", string.Empty);

            return cleanedString;
        }

        /// <summary>
        /// Receives a raw field pattern from the Log4Net config file, and cleans it, ready for processing.
        /// </summary>
        /// <param name="fieldPattern">The raw field pattern to be cleaned.</param>
        public static string CleanConversionPatternField(string fieldPattern)
        {
            string cleanedString = CleanStringOfAllKeyValues(fieldPattern);
            cleanedString = CleanStringOfAllUnnecessaryIdentifiers(cleanedString);
            cleanedString = cleanedString.Replace("  ", " ");

            return cleanedString;
        }

        public static string RetrieveFieldNameFromConversionPattern(string conversionPattern)
        {
            if (string.IsNullOrEmpty(conversionPattern))
            {
                return "NULL ConversionPattern";
            }

            if (MessageConversionPatterns.Contains(conversionPattern))
            {
                return "Message";
            }
            else if (AppDomainConversionPatterns.Contains(conversionPattern))
            {
                return "AppDomain";
            }

            switch (conversionPattern)
            {
                case "aspnetcache":
                {
                    return "ASPNet Cache Item(s)";
                }
                case "aspnetcontext":
                {
                    return "ASPNet Context Item(s)";
                }
                case "aspnetsession":
                {
                    return "ASPNet Session Item(s)";
                }
                case "c":
                case "logger":
                {
                    return "Logger";
                }
                case "C":
                case "type":
                case "class":
                {
                    return "Type";
                }
                case "d":
                case "date":
                {
                    return "Date";
                }
                case "exception":
                {
                    return "Exception";
                }
                case "F":
                case "file":
                {
                    return "File";
                }
                case "Identity":
                case "u":
                {
                    return "Username";
                }
                case "l":
                case "location":
                {
                    return "Location";
                }
                case "Line":
                case "line":
                {
                    return "Line Number";
                }
                case "p":
                case "level":
                {
                    return "Event Level";
                }
                case "M":
                case "method":
                {
                    return "Method";
                }
                case "mdc":
                case "property":
                case "properties":
                case "P":
                case "X":
                {
                    return "Event Property(s)";
                }
                case "ndc":
                case "x":
                {
                    return "Nested Diagnostic Context";
                }
                case "r":
                case "timestamp":
                {
                    return "Timestamp";
                }
                case "stacktrace":
                {
                    return "Stacktrace";
                }
                case "stacktracedetail":
                {
                    return "Stack Trace Detail";
                }
                case "t":
                case "thread":
                {
                    return "Thread";
                }
                case "username":
                case "w":
                {
                    return "Windows Username";
                }
                case "utcdate":
                {
                    return "UTC Date";
                }
                default:
                {
                    return $"Unknown Pattern: '{conversionPattern}'";
                }
            }
        }
    }
}
