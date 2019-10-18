using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Xml;
using log4net;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using LogForwardService.Service.Config.Section;
using UDPSocketComService;

namespace LogForwardService.Service
{
    public class LogForwardService: ServiceBase
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(LogForwardService));

        private UDPSocketComReceiver _udpReceiver;
        private IPAddress _serviceIpAddress;
        private int _servicePort;
        private int _bufferSize;

        public LogForwardService()
        {
            ServiceName = "LogForwardService";
            LoadLog4NetConfig();

            Log.Debug($"Beginning LogForwardService");
        }

        protected override void OnStart(string[] args)
        {
            Console.WriteLine("Attempting to start LogForwardService");
            LoadConfiguration();

            Log.Debug($"Attempting to create UDPSocketComReceiver");

            try
            {
                _udpReceiver = new UDPSocketComReceiver(_serviceIpAddress, _servicePort);
                _udpReceiver.BeginReceive();
            }
            catch (Exception e)
            {
                Log.Error($"Exception beginning UDPSocketComReceiver: {e}");
            }


            Log.Debug($"Start process completed.");
            if (null != _udpReceiver)
            {
                Log.Debug($"UDPSocketComReceiver successfully started on IP {_serviceIpAddress}:{_servicePort}");
            }
            else
            {
                Log.Error($"UDPSocketComReceiver not started.");
            }
        }

        protected override void OnStop()
        {

        }

        /// <summary>
        /// Configures the Service based on the content of the config file
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                LoadLogForwardServiceConfig();
            }
            catch (Exception e)
            {
                Log.Error($"Exception loading LogForwardServiceConfig: {e}");
                throw;
            }
        }

        /// <summary>
        /// Manually loads the Log4Net Configuration
        /// </summary>
        private void LoadLog4NetConfig()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "\\log4net.config"));
            ILoggerRepository repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(Hierarchy));
            XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }

        /// <summary>
        /// Manually loads the LogForwardService Configuration
        /// </summary>
        private void LoadLogForwardServiceConfig()
        {
            Log.Debug($"Loading Service configuration from config file");

            if (!(ConfigurationManager.GetSection("ServiceSettings") is ServiceSettings serviceSettings))
            {
                throw new NullReferenceException(nameof(serviceSettings));
            }

            Log.Info($"Configuring Service:");
            Log.Info($"    Will listen for new messages on:");
            _serviceIpAddress = IPAddress.Parse(serviceSettings.ListeningSettings.ServiceListenIpAddress);
            Log.Info($"        - IP Address: {_serviceIpAddress}");
            _servicePort = serviceSettings.ListeningSettings.ServiceListenPort;
            Log.Info($"              - Port: {_servicePort}");
            _bufferSize = serviceSettings.ListeningSettings.BufferBytes;
            Log.Info($"        - BufferSize: {_servicePort}B");
            Log.Debug($"Finished loading configuration from config file");
        }
    }
}
