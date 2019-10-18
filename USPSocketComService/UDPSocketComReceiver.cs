using System;
using System.Net;
using System.Net.Sockets;
using UDPSocketComService.Interfaces;

namespace UDPSocketComService
{
    /// <summary>
    /// Receives and passes UDP Messages from a <see cref="UDPSocketComSender"/>
    /// </summary>
    public class UDPSocketComReceiver: UDPSocketCom, IUDPReceiver
    {
        #region Constants

        /// <summary>
        /// An int representing 1024 bytes.
        /// </summary>
        private const int KiloByte = 1024;

        #endregion

        #region Fields

        private EndPoint _endPointFrom;
        private readonly int _bufferSize;
        private AsyncCallback _asyncCallbackMethod;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="UDPSocketComReceiver"/> listening to passed IP Address and UDP Port while only receiving from the configured source messages of the passed size.
        /// </summary>
        /// <param name="ipAddressFrom">The <see cref="IPAddress"/> to receive messages from.</param>
        /// <param name="fromPort">The UDP Port to receive messages from.</param>
        /// <param name="serverAddress">The <see cref="IPAddress"/> for the Receiver to listen to.</param>
        /// <param name="serverPort">The <see cref="int"/> of the UDP Port for the Receiver to listen to.</param>
        /// <param name="bufferSize">The number of bytes to receive</param>
        public UDPSocketComReceiver(IPAddress ipAddressFrom, int fromPort, IPAddress listenAddress, int listenPort, int bufferSize)
            : base(listenAddress, listenPort)
        {
            _endPointFrom = new IPEndPoint(ipAddressFrom, fromPort);
            _bufferSize = bufferSize;
            UDPState.Buffer = new byte[_bufferSize];
        }

        /// <summary>
        /// Creates a new <see cref="UDPSocketComReceiver"/> listening on the passed IP Address and UDP Port that will receive one kilobyte messages.
        /// </summary>
        /// <param name="serverAddress">The <see cref="IPAddress"/> for the Receiver to listen to.</param>
        /// <param name="serverPort">The <see cref="int"/> of the UDP Port for the Receiver to listen to.</param>
        public UDPSocketComReceiver(IPAddress serverAddress, int serverPort) : 
            this(IPAddress.Any, 0, serverAddress, serverPort, KiloByte)
        {
            _bufferSize = KiloByte;
            UDPState.Buffer = new byte[_bufferSize];
        }

        /// <summary>
        /// Creates a new <see cref="UDPSocketComReceiver"/> listening on the passed IP Address and UDP Port that will receive messages of the configured size.
        /// </summary>
        /// <param name="serverAddress">The <see cref="IPAddress"/> for the Receiver to listen to.</param>
        /// <param name="serverPort">The <see cref="int"/> of the UDP Port for the Receiver to listen to.</param>
        /// <param name="bufferSize">The <see cref="int"/> that represents the number of bytes to listen to for a single message</param>
        public UDPSocketComReceiver(IPAddress serverAddress, int serverPort, int bufferSize) :
            this(IPAddress.Any, 0, serverAddress, serverPort, bufferSize)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Begins to asynchronously receive data from the configured source.
        /// </summary>
        public void BeginReceive()
        {
            Socket.BeginReceiveFrom(UDPState.Buffer
                , 0
                , _bufferSize
                , SocketFlags.None
                , ref _endPointFrom
                , _asyncCallbackMethod = (asyncResult) =>
                {
                    if (asyncResult.AsyncState is StateObject asyncStateObject)
                    {
                        Socket.EndReceiveFrom(asyncResult, ref _endPointFrom);
                        Socket.BeginReceiveFrom(asyncStateObject.Buffer, 0, _bufferSize, SocketFlags.None,
                            ref _endPointFrom, _asyncCallbackMethod, asyncStateObject);
                    }
                }
                ,UDPState);
        }

        #endregion
    }
}
