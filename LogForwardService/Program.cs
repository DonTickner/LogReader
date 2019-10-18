using System;
using System.ServiceProcess;

namespace LogForwardService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Service.LogForwardService service = new Service.LogForwardService())
            {
                ServiceBase.Run(service);
            }
        }
    }
}
