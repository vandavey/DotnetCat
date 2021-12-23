using System;
using System.Net;

namespace DotnetCat.Network
{
    /// <summary>
    ///  Network endpoint host information
    /// </summary>
    internal class HostEndPoint
    {
        private int _port;         // Network port number

        private string _hostName;  // Network host name

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
        public HostEndPoint(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        /// <summary>
        ///  Initialize object
        /// </summary>
        public HostEndPoint(IPEndPoint ep)
        {
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

        /// Network host name
        public string HostName
        {
            get => _hostName;
            set
            {
                if (value is null or "")
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _hostName = value;
            }
        }

        /// <summary>
        ///  Return a string that represents a Target
        /// </summary>
        public override string ToString()
        {
            if ((Port <= -1) || (HostName is null or ""))
            {
                return base.ToString();
            }
            return $"{HostName}:{Port}";
        }
    }
}
