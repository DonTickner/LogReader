using System;
using System.Collections.Generic;
using System.Text;

namespace Log4Net.Extensions.Configuration.Implementation.ConfigObjects
{
    /// <summary>
    /// Log4Net Appender types.
    /// Refer to: https://logging.apache.org/log4net/log4net-1.2.13/release/sdk/log4net.Appender.html
    /// </summary>
    public enum AppenderType
    {
        /// <summary>
        /// Appender that logs to a database.
        /// </summary>
        AdoNetAppender,
        /// <summary>
        /// Appends logging events to the terminal using ANSI color escape sequences.
        /// </summary>
        AnsiColorTerminalAppender,
        /// <summary>
        /// Appends log events to the ASP.NET TraceContext system.
        /// </summary>
        AspNetTraceAppender,
        /// <summary>
        /// Buffers events and then forwards them to attached appenders.
        /// </summary>
        BufferingForwardingAppender,
        /// <summary>
        /// Appends logging events to the console.
        /// </summary>
        ColoredConsoleAppender,
        /// <summary>
        /// Appends logging events to the console.
        /// </summary>
        ConsoleAppender,
        /// <summary>
        /// Appends log events to the Debug system.
        /// </summary>
        DebugAppender,
        /// <summary>
        /// Writes events to the system event log.
        /// </summary>
        EventLogAppender,
        /// <summary>
        /// Appends logging events to a file.
        /// </summary>
        FileAppender,
        /// <summary>
        /// This appender forwards logging events to attached appenders.
        /// </summary>
        ForwardingAppender,
        /// <summary>
        /// Logs events to a local syslog service.
        /// </summary>
        LocalSyslogAppender,
        /// <summary>
        /// Appends colorful logging events to the console, using the .NET 2 built-in capabilities.
        /// </summary>
        ManagedColoredConsoleAppender,
        /// <summary>
        /// Stores logging events in an array.
        /// </summary>
        MemoryAppender,
        /// <summary>
        /// Logs entries by sending network messages using the NetMessageBufferSend native function.
        /// </summary>
        NetSendAppender,
        /// <summary>
        /// Appends log events to the OutputDebugString system.
        /// </summary>
        OutputDebugStringAppender,
        /// <summary>
        /// Logs events to a remote syslog daemon.
        /// </summary>
        RemoteSyslogAppender,
        /// <summary>
        /// Delivers logging events to a remote logging sink.
        /// </summary>
        RemotingAppender,
        /// <summary>
        /// Appender that rolls log files based on size or date or both.
        /// </summary>
        RollingFileAppender,
        /// <summary>
        /// Send an e-mail when a specific logging event occurs, typically on errors or fatal errors.
        /// </summary>
        SmtpAppender,
        /// <summary>
        /// Send an email when a specific logging event occurs, typically on errors or fatal errors. Rather than sending via smtp it writes a file into the directory specified by PickupDir. This allows services such as the IIS SMTP agent to manage sending the messages.
        /// </summary>
        SmtpPickupDirAppender,
        /// <summary>
        /// Appender that allows clients to connect via Telnet to receive log messages
        /// </summary>
        TelnetAppender,
        /// <summary>
        /// Sends logging events to a TextWriter.
        /// </summary>
        TextWriterAppender,
        /// <summary>
        /// Appends log events to the Trace system.
        /// </summary>
        TraceAppender,
        /// <summary>
        /// Sends logging events as connectionless UDP datagrams to a remote host or a multicast group using an UdpClient.
        /// </summary>
        UdpAppender,

    }
}
