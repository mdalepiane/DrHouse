using DrHouse.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimS.Telnet;
using System.Threading;

namespace DrHouse.Telnet
{
    /// <summary>
    /// Telnet Health Dependency uses telnet to check if it is possible to connect
    /// to a given hostname and port.
    /// </summary>
    public class TelnetHealthDependency : IHealthDependency
    {
        private readonly string _hostname;
        private readonly int _port;
        private readonly string _serviceName;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="hostname">The hostname to test the connection</param>
        /// <param name="port">The port to test the connection</param>
        /// <param name="serviceName">Optional service name, to enchant result readability</param>
        public TelnetHealthDependency(string hostname, int port, string serviceName = null)
        {
            _hostname = hostname;
            _port = port;
            _serviceName = serviceName;
        }

        public HealthData CheckHealth()
        {
            HealthData healthData = new HealthData($"{_hostname}:{_port} [{_serviceName ?? ""}]");
            healthData.Type = "Telnet";

            try
            {
                CancellationToken token = new CancellationToken();
                Client telnetClient = new Client(_hostname, _port, token);

                if (telnetClient.IsConnected)
                {
                    healthData.IsOK = true;
                }
            } catch(Exception ex)
            {
                healthData.IsOK = false;
                healthData.ErrorMessage = ex.Message;
            }

            return healthData;
        }

        public HealthData CheckHealth(Action check)
        {
            throw new NotImplementedException();
        }
    }
}
