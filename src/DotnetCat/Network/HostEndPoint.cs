using System;
using System.Net;
using DotnetCat.Utils;

namespace DotnetCat.Network
{
    /// <summary>
    ///  Network endpoint host information
    /// </summary>
    internal class HostEndPoint
    {
        private int _port;          // Network port number

        private string? _hostName;  // Network hostname

        /// <summary>
        ///  Initialize object
        /// </summary>
        public HostEndPoint()
        {
            _hostName = default;
            _port = -1;
        }

        /// <summary>
        ///  Initialize object
        /// </summary>
        public HostEndPoint(string? hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        /// <summary>
        ///  Initialize object
        /// </summary>
        public HostEndPoint(IPAddress address, int port)
        {
            HostName = address.ToString();
            Port = port;
        }

        /// <summary>
        ///  Initialize object
        /// </summary>
        public HostEndPoint(IPEndPoint? ep)
        {
            _  = ep ?? throw new ArgumentNullException(nameof(ep));

            HostName = ep.Address.ToString();
            Port = ep.Port;
        }

        /// Network port number
        public int Port
        {
            get => _port;
            set
            {
                if (!Net.IsValidPort(value))
                {
                    throw new ArgumentException("Invalid port", nameof(value));
                }
                _port = value;
            }
        }

        /// Network hostname
        public string? HostName
        {
            get => _hostName;
            set
            {
                if (value.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _hostName = value;
            }
        }

        /// <summary>
        ///  Get a string that represents the underlying IPv4 endpoint information
        /// </summary>
        public override string? ToString()
        {
            if (Port <= -1 || HostName.IsNullOrEmpty())
            {
                return base.ToString();
            }
            return $"{HostName}:{Port}";
        }
    }
}
