using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UDPSocketComService.Interfaces;

namespace UDPSocketComService
{
    /// <summary>
    /// Sends messages via UDP
    /// </summary>
    public class UDPSocketComSender: UDPSocketCom, IUDPSender<string>
    {
        /// <summary>
        /// Creates a new <see cref="UDPSocketComSender"/> to send messages to the passed <see cref="UDPSocketComReceiver"/>.
        /// </summary>
        /// <param name="serverAddress">The <see cref="IPAddress"/> of the <see cref="UDPSocketComReceiver"/> listening for UDP messages.</param>
        /// <param name="serverPort">The <see cref="int"/> representing the UDP port of the <see cref="UDPSocketComReceiver"/> listening.</param>
        public UDPSocketComSender(IPAddress serverAddress, int serverPort) : base(serverAddress, serverPort)
        {

        }

        /// <summary>
        /// Sends a message in <see cref="string"/> format to the configured Server Address
        /// </summary>
        /// <param name="textToSend">The string to be sent.</param>
        public void Send(string textToSend)
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes(textToSend);

            Socket.BeginSend(bytesToSend
                , 0
                , bytesToSend.Length
                , SocketFlags.None
                , (asyncResult) =>
                {
                    if (asyncResult.AsyncState is StateObject asyncStateObject)
                    {
                        Socket.EndSend(asyncResult);
                    }
                }
                , UDPState);
        }
    }
}
