using System;
using System.Collections.Generic;
using System.Text;

namespace UDPSocketComService.Interfaces
{
    public interface IUDPSender<in T>
    {
        void Send(T toSend);
    }
}
