using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSocketComService
{
    /// <summary>
    /// An implementable class that contains the basics for sending and receiving bytes via UDP
    /// </summary>
    public class UDPSocketCom
    {
        #region Sub-Classes

        /// <summary>
        /// Represents the current state of the UDPSocketCom message received
        /// </summary>
        public class StateObject
        {
            /// <summary>
            /// The buffer of bytes read
            /// </summary>
            public byte[] Buffer { get; set; }
        }

        #endregion

        public Socket Socket { get; private set; }
        public StateObject UDPState { get; set; }

        protected UDPSocketCom(IPAddress serverAddress, int serverPort)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            Socket.Bind(new IPEndPoint(serverAddress, serverPort));
            UDPState = new StateObject();
        }
    }
}
