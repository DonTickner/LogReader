using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Log4Net.Extensions.Configuration.Exceptions;
using Log4Net.Extensions.Configuration.Implementation.ConfigObjects;

namespace Log4Net.Extensions.Configuration.Implementation
{
    /// <summary>
    /// A static helper class that can create a <see cref="Log4NetConfig"/> object based on the content of a physical Log4Net .config file.
    /// </summary>
    public static class Log4NetConfigLoader
    {
        #region Fields

        /// <summary>
        /// The Log4Net config file to be loaded.
        /// </summary>
        private static XDocument _configFile;

        /// <summary>
        /// The <see cref="Log4NetConfig"/> to be created and returned.
        /// </summary>
        private static Log4NetConfig _log4NetCongConfig;

        #endregion
        
        #region Methods

        /// <summary>
        /// Creates a <see cref="Log4NetConfig"/> object from a physical Log4Net config file.
        /// </summary>
        /// <param name="configFilePath">The physical file path of the Log4Net config file to be used.</param>
        public static Log4NetConfig CreateLog4NetConfig(string configFilePath)
        {
            _log4NetCongConfig = new Log4NetConfig();

            LoadLog4NetConfigFile(configFilePath);
            LoadAppenders();

            return _log4NetCongConfig;
        }

        /// <summary>
        /// Loads the content of a Log4Net config file into the local <see cref="XDocument"/>.
        /// </summary>
        /// <param name="configFilePath"></param>
        private static void LoadLog4NetConfigFile(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                throw new Log4NetConfigFileAccessErrorException($"The file {configFilePath} does not exist.");
            }

            try
            {
                using (FileStream stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
                {
                    _configFile = XDocument.Load(stream);
                }
            }
            catch (Exception e)
            {
                throw new Log4NetConfigFileAccessErrorException($"Error attempting to load Log4NetConfig File. See Inner Exception.", e);
            }

            if (!_configFile.Elements("log4net").Any())
            {
                throw new InvalidLog4NetConfigStructureException("Unable to locate <log4net> node within config file. Please review file structure and try again.");
            }
        }
        
        #region Load Appender Methods

        /// <summary>
        /// Loads all <see cref="Appender"/>s present in the Log4Net Config file.
        /// </summary>
        private static void LoadAppenders()
        {
            IEnumerable<XElement> appenders = _configFile
                .Elements("log4net")
                .Elements("appender");

            if (!appenders.Any())
            {
                throw new InvalidLog4NetConfigStructureException("Unable to locate an <appender> element within <log4net> node. Unable to proceed.");
            }

            foreach (XElement appender in appenders)
            {
                _log4NetCongConfig.AddAppender(LoadAppenderFromElement(appender));
            }
        }

        /// <summary>
        /// Loads an <see cref="Appender"/> based on the content of an appropriate Log4Net Config <see cref="XElement"/>.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static Appender LoadAppenderFromElement(XElement appenderElement)
        {
            if (!appenderElement.Descendants().Any())
            {
                throw new InvalidLog4NetConfigStructureException("<appender> node has no descendent nodes. Unable to proceed.");
            }

            Appender newAppender = new Appender
            {
                Type = LoadAppenderTypeFromElement(appenderElement),
                FolderPath = LoadAppenderFolderPathFromElement(appenderElement),
                RollingFileNameMask = LoadAppenderRollingFileMaskFromElement(appenderElement),
                StaticFileNameMask = LoadAppenderStaticFileNameMaskFromElement(appenderElement),
                UseStaticLogFileName = LoadAppenderUseStaticLogFileNameFromElement(appenderElement),
                Layout = LoadAppenderLayoutFromElement(appenderElement)
            };

            return newAppender;
        }

        #endregion

        #region XElement Retrieval Methods

        /// <summary>
        /// Retrieves the attribute value from an <see cref="XElement"/> using the attribute string provided.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> to retrieve an attribute value from.</param>
        /// <param name="attribute">The attribute whose value is to be retrieved.</param>
        private static string RetrieveElementAttributeValue(XElement element, string attribute = "value")
        {
            return element.Attribute(attribute)?.Value ?? string.Empty;
        }

        /// <summary>
        /// Retrieves a collection of <see cref="XElement"/>s being the Descendants of the passed <see cref="XElement"/> based on their Name.
        /// </summary>
        /// <param name="element">The <see cref="XElement"/> whose Descendants are to be retrieved.</param>
        /// <param name="descendantName">The <see cref="string"/> Name value to use to locate the Descendants.</param>
        private static IEnumerable<XElement> RetrieveElementDescendantsByName(XElement element, string descendantName)
        {
            List<XElement> matchedDescendants = new List<XElement>();
            foreach (XElement descendant in element.Descendants())
            {
                string name = descendant.Name.ToString();
                if (descendant.Name == AppenderDescendants.Param)
                {
                    name = RetrieveElementAttributeValue(descendant, "name");
                }

                if (string.Equals(name, descendantName, StringComparison.CurrentCultureIgnoreCase))
                {
                    matchedDescendants.Add(descendant);
                }
            }

            return matchedDescendants;
        }

        /// <summary>
        /// Retrieves the exact value of of the <see cref="Appender"/>'s 'file' node's value.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> appender whose file node's value is to be retrieved.</param>
        private static string RetrieveAppenderFilePathValue(XElement appenderElement)
        {
            XElement fileNode = RetrieveElementDescendantsByName(appenderElement, AppenderDescendants.File)
                .FirstOrDefault();

            if (null == fileNode)
            {
                return string.Empty;
            }

            return RetrieveElementAttributeValue(fileNode);
        }

        /// <summary>
        /// Retrieves the exact value of of the <see cref="Appender"/>'s 'datepattern' node's value.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> appender whose file node's value is to be retrieved.</param>
        private static string RetrieveAppenderDatePatternValue(XElement appenderElement)
        {
            XElement datePatternNode = RetrieveElementDescendantsByName(appenderElement, AppenderDescendants.DatePattern)
                .FirstOrDefault();

            if (null == datePatternNode)
            {
                return string.Empty;
            }

            return RetrieveElementAttributeValue(datePatternNode);
        }

        /// <summary>
        /// Retrieves the exact value of of the <see cref="Appender"/>'s 'staticlogfilename' node's value.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> appender whose file node's value is to be retrieved.</param>
        private static bool RetrieveAppenderStaticLogFileNameValue(XElement appenderElement)
        {
            XElement staticLogFileName = RetrieveElementDescendantsByName(appenderElement, AppenderDescendants.StaticLogFileName)
                .FirstOrDefault();

            if (null == staticLogFileName)
            {
                return false;
            }

            string staticLogFileNameValueString = RetrieveElementAttributeValue(staticLogFileName);

            if (bool.TryParse(staticLogFileNameValueString, out bool appenderStaticFileNameValue))
            {
                return appenderStaticFileNameValue;
            }

            return false;
        }

        #endregion

        #region Load Appender Value Methods

        /// <summary>
        /// Retrieves the <see cref="AppenderType"/> from a Log4Net config file 'appender' element.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static AppenderType LoadAppenderTypeFromElement(XElement appenderElement)
        {
            string typeStringValue = RetrieveElementAttributeValue(appenderElement, AppenderAttributes.Type).Replace("log4net.Appender.", "");
            string[] typeComponents = typeStringValue.Split(',');
            string appenderTypeValue = typeComponents[0];

            if (Enum.TryParse(appenderTypeValue, out AppenderType returnType))
            {
                return returnType;
            }

            throw new InvalidLog4NetConfigAttributeValueException($"<appender> element contained a 'type' with value '{appenderTypeValue}'. This is not a supported Appender.");
        }

        /// <summary>
        /// Retrieves the FolderPath from a Log4Net config file 'appender' element.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static string LoadAppenderFolderPathFromElement(XElement appenderElement)
        {
            string fileNodeValue = RetrieveAppenderFilePathValue(appenderElement);

            if (string.IsNullOrEmpty(fileNodeValue))
            {
                return string.Empty;
            }

            char escapedBackSlash = '\\';
            int locationOfBeginningOfFileName = fileNodeValue.LastIndexOf(escapedBackSlash) + escapedBackSlash.ToString().Length;
            return fileNodeValue.Substring(0, locationOfBeginningOfFileName);
        }

        /// <summary>
        /// Retrieves the FileMask from a Log4Net config file 'appender' element.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static string LoadAppenderRollingFileMaskFromElement(XElement appenderElement)
        {
            string staticFileName = LoadAppenderStaticFileNameMaskFromElement(appenderElement);

            string datePatternValue = RetrieveAppenderDatePatternValue(appenderElement);
            if (!string.IsNullOrEmpty(datePatternValue))
            {
                datePatternValue = datePatternValue.Replace("'", "");
                return staticFileName + datePatternValue;
            }

            return staticFileName;
        }

        /// <summary>
        /// Retrieves the Static FileMask from a Log4Net config file 'appender' element.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static string LoadAppenderStaticFileNameMaskFromElement(XElement appenderElement)
        {
            string fileNodeValue = RetrieveAppenderFilePathValue(appenderElement);

            if (string.IsNullOrEmpty(fileNodeValue))
            {
                return string.Empty;
            }

            char escapedBackSlash = '\\';
            int locationOfBeginningOfFileName = fileNodeValue.LastIndexOf(escapedBackSlash) + escapedBackSlash.ToString().Length;
            string startOfLog4NetFileNameMask = fileNodeValue.Substring(locationOfBeginningOfFileName);

            return startOfLog4NetFileNameMask;
        }

        /// <summary>
        /// Retrieves the UseStaticLogFileName from a Log4Net config file 'appender' element.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static bool LoadAppenderUseStaticLogFileNameFromElement(XElement appenderElement)
        {
            return RetrieveAppenderStaticLogFileNameValue(appenderElement);
        }

        /// <summary>
        /// Creates an <see cref="AppenderLayout"/> based on the content of the <see cref="Appender"/>'s <see cref="XElement"/>.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static AppenderLayout LoadAppenderLayoutFromElement(XElement appenderElement)
        {
            string headerLine = string.Empty;
            XElement headerDescendant =
                RetrieveElementDescendantsByName(appenderElement, AppenderDescendants.Header).FirstOrDefault();

            if (null != headerDescendant)
            {
                headerLine = RetrieveElementAttributeValue(headerDescendant);
            }

            string footerLine = string.Empty;
            XElement footerDescendant=
                RetrieveElementDescendantsByName(appenderElement, AppenderDescendants.Footer).FirstOrDefault();

            if (null != footerDescendant)
            {
                footerLine = RetrieveElementAttributeValue(footerDescendant);
            }

            AppenderLayout newAppenderLayout = new AppenderLayout
            {
                Header = headerLine,
                Footer = footerLine,
                ConversionPattern = LoadAppenderConversionPatternFromElement(appenderElement)
            };

            return newAppenderLayout;
        }

        /// <summary>
        /// Creates a <see cref="AppenderConversionPattern"/> based on the content of the <see cref="Appender"/>'s <see cref="XElement"/>.
        /// </summary>
        /// <param name="appenderElement">The <see cref="XElement"/> that represents an appender element.</param>
        private static AppenderConversionPattern LoadAppenderConversionPatternFromElement(XElement appenderElement)
        {
            XElement conversionPatternDescendant =
                RetrieveElementDescendantsByName(appenderElement, AppenderDescendants.ConversionPattern).FirstOrDefault();

            if (null == conversionPatternDescendant)
            {
                throw new InvalidLog4NetConfigStructureException($"Unable to locate a <param> element with name of '{AppenderDescendants.ConversionPattern}'. Unable to proceed.");
            }

            string conversionPattern = RetrieveElementAttributeValue(conversionPatternDescendant);

            AppenderConversionPattern newAppenderConversionPattern = new AppenderConversionPattern()
            {
                RawLine = conversionPattern
            };

            int numberOfFieldsBeforeMessage = 0;
            bool foundMessage = false;

            for (int i = 0; i < conversionPattern.Length - 1; i++)
            {
                if (!conversionPattern[i].Equals('%'))
                {
                    continue;
                }

                if (conversionPattern[i + 1].Equals('%'))
                {
                    i += 2;
                    continue;
                }

                string currentField = string.Empty;
                bool innerOpen = false;
                for (int x = i + 1; x < conversionPattern.Length; x++)
                {
                    if (char.IsDigit(conversionPattern[x]))
                    {
                        continue;
                    }

                    currentField += conversionPattern[x];

                    if (string.Equals(currentField, "m", StringComparison.InvariantCulture))
                    {
                        foundMessage = true;
                        break;
                    }

                    if (conversionPattern[x].Equals('{'))
                    {
                        innerOpen = true;
                        continue;
                    }

                    if (conversionPattern[x].Equals(' ') &&
                        !innerOpen)
                    {
                        currentField = currentField.Substring(0, currentField.Length - 1);

                        if (string.Equals(currentField, "d") ||
                            string.Equals(currentField, "date"))
                        {
                            numberOfFieldsBeforeMessage++;
                        }

                        if (currentField[0].Equals('d') &&
                            currentField.Contains("{"))
                        {
                            int spaces = currentField.Count(c => c.Equals(' '));
                            numberOfFieldsBeforeMessage += spaces;
                        }
                        else
                        {
                            numberOfFieldsBeforeMessage++;
                        }

                        i = x;
                        break;
                    }

                    if (conversionPattern[x].Equals('}'))
                    {
                        innerOpen = false;
                    }
                }

                if (foundMessage)
                {
                    break;
                }
            }

            newAppenderConversionPattern.NumberOfFieldsBeforeMessageField = numberOfFieldsBeforeMessage;
            newAppenderConversionPattern.Delimiter = " ";

            return newAppenderConversionPattern;
        }

        #endregion

        #endregion
    }
}
