using System.Configuration;

namespace LogForwardService.Service.Config.Element
{
    /// <summary>
    /// Represents the config file settings within the ListeningSettings <see cref="ConfigurationElement"/>
    /// </summary>
    public class ListeningSettings: ConfigurationElement
    {
        /// <summary>
        /// The IPAddress that the Service is configured to listen for Log messages on
        /// </summary>
        [ConfigurationProperty("ServiceListenIPAddress", IsRequired = true)]
        public string ServiceListenIpAddress => (string) this["ServiceListenIPAddress"];

        /// <summary>
        /// The UDP Port that the Service is configured to listen for Log messages on.
        /// </summary>
        [ConfigurationProperty("ServiceListenPort", DefaultValue = 0, IsRequired = true)]
        public int ServiceListenPort => (int) this["ServiceListenPort"];

        /// <summary>
        /// The number of bytes for the Service to read at a time.
        /// </summary>
        [ConfigurationProperty("BufferBytes", DefaultValue = 1024, IsRequired = false)]
        public int BufferBytes => (int) this["BufferBytes"];

        /// <summary>
        /// The name of the service to appear in the windows service list
        /// </summary>
        [ConfigurationProperty("ServiceName", DefaultValue = "LogListeningService", IsRequired = false)]
        public string ServiceName => this["ServiceName"].ToString();
    }
}
