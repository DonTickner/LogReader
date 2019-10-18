using System.Collections.Generic;
using System.Configuration;
using System.Reflection.Metadata.Ecma335;
using LogForwardService.Service.Config.Element;

namespace LogForwardService.Service.Config.Section
{
    /// <summary>
    /// Represents the config file settings within the ServiceSettings <see cref="ConfigurationSection"/>
    /// </summary>
    public class ServiceSettings: ConfigurationSection
    {
        /// <summary>
        /// The settings to control how the service is configured to listen for log messages
        /// </summary>
        [ConfigurationProperty("ListeningSettings")]
        public ListeningSettings ListeningSettings
        {
            get => (ListeningSettings) this["ListeningSettings"];
            set => value = (ListeningSettings)this["ListeningSettings"];
        }
    }
}
