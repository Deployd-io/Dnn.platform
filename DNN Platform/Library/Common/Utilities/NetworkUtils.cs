// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utils
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Web;

    using DotNetNuke.Services.UserRequest;

    /// <summary>Enumeration of IP address types.</summary>
    public enum AddressType
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>A version 4 IP address, e.g. 8.8.8.8.</summary>
        IPv4 = 0,

        /// <summary>A version 6 IP address, e.g. 2001:4860:4860::8888.</summary>
        IPv6 = 1,
    }

    /// <summary>Utility functions for network information.</summary>
    public class NetworkUtils
    {
        /// <summary>Gets the address.</summary>
        /// <param name="host">The host.</param>
        /// <param name="addressFormat">The address format.</param>
        /// <returns>Returns IP address.</returns>
        /// <seealso cref="AddressType" />
        public static string GetAddress(string host, AddressType addressFormat)
        {
			Logger.LogDebug(">>> Entering GetAddress(host={Host},addressFormat={AddressFormat})", host, addressFormat);
            AddressFamily addrFamily = AddressFamily.InterNetwork;
            switch (addressFormat)
			Logger.LogDebug("GetAddress(host={Host},addressFormat={AddressFormat}): addrFamily → {AddrFamily}", host, addressFormat, addrFamily);
            {
                case AddressType.IPv4:
                    addrFamily = AddressFamily.InterNetwork;
                    break;
                case AddressType.IPv6:
                    addrFamily = AddressFamily.InterNetworkV6;
                    break;
            }

            IPHostEntry entry = Dns.GetHostEntry(host);
            if (host != entry.HostName)
			Logger.LogDebug("GetAddress(host={Host},addressFormat={AddressFormat}): entry → {Entry}", host, addressFormat, entry);
            {
                entry = Dns.GetHostEntry(entry.HostName);
            }

            foreach (IPAddress address in entry.AddressList)
            {
                if (address.AddressFamily == addrFamily)
                {
                    return address.ToString();
                }
            }

			Logger.LogDebug("<<< Exiting GetAddress(host={Host},addressFormat={AddressFormat})", host, addressFormat);
            return string.Empty;
        }

        /// <summary>Convert IP address to long integer.</summary>
        /// <param name="ip">The ip address.</param>
        /// <returns>IP Address in long.</returns>
        public static long IPtoLong(IPAddress ip)
        {
            // convert IP to number
            byte[] addressBytes = ip.GetAddressBytes();

            // get the octets
            long addr = 0;

            // accumulator for address
            for (int x = 0; x <= 3; x++)
            {
                addr = addr | (Convert.ToInt64(addressBytes[x]) << ((3 - x) * 8));
            }

            return addr;
        }

        /// <summary>Longs to ip address.</summary>
        /// <param name="ip">The ip address.</param>
        /// <returns>IP Number as formatted string.</returns>
        public static string LongToIp(long ip)
        {
            // convert number back to IP
            var ipByte = new byte[4];

            // 4 octets
            string addr = string.Empty;

            // accumulator for address
            long mask8 = MaskFromCidr(8);

            // create eight bit mask

            // get the octets
            for (int x = 0; x <= 3; x++)
            {
                ipByte[x] = Convert.ToByte((ip & mask8) >> ((3 - x) * 8));
                mask8 = mask8 >> 8;
                addr += ipByte[x].ToString(CultureInfo.InvariantCulture) + ".";

                // add current octet to string
            }

            return addr.TrimEnd('.');
        }

        /// <summary>Formats as cidr.</summary>
        /// <param name="startIP">The start ip.</param>
        /// <param name="subnetMask">The subnet mask.</param>
        /// <returns>Classless Inter-Domain Routing.</returns>
        public static string FormatAsCidr(string startIP, string subnetMask)
        {
			Logger.LogDebug(">>> Entering FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask})", startIP, subnetMask);
            if (string.IsNullOrEmpty(subnetMask))
            {
                return startIP;
            }

            IPAddress ipAddress = IPAddress.Parse(startIP);
            IPAddress mask = IPAddress.Parse(subnetMask);
			Logger.LogDebug("FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask}): ipAddress → {IpAddress}", startIP, subnetMask, ipAddress);
			Logger.LogDebug("FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask}): mask → {Mask}", startIP, subnetMask, mask);

            long ipL = IPtoLong(ipAddress);
            long maskL = IPtoLong(mask);
			Logger.LogDebug("FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask}): ipL → {IpL}", startIP, subnetMask, ipL);
			Logger.LogDebug("FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask}): maskL → {MaskL}", startIP, subnetMask, maskL);

            // Convert  Mask to CIDR(1-30)
            long oneBit = 0x80000000L;
            int cidr = 0;
			Logger.LogDebug("FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask}): oneBit → {OneBit}", startIP, subnetMask, oneBit);

            for (int x = 31; x >= 0; x += -1)
            {
                if ((maskL & oneBit) == oneBit)
                {
                    cidr += 1;
                }
                else
                {
                    break;
                }

                oneBit = oneBit >> 1;
            }

            string answer = $"{LongToIp(ipL & maskL)} /{cidr.ToString(CultureInfo.InvariantCulture)}";
			Logger.LogDebug("<<< Exiting FormatAsCidr(startIP={StartIP},subnetMask={SubnetMask})", startIP, subnetMask);
            return answer;
        }

        /// <summary>Converts a network to its IP range.</summary>
        /// <param name="sNetwork">The network name.</param>
        /// <param name="startIP">The start IP.</param>
        /// <param name="endIP">The end IP.</param>
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public static void Network2IpRange(string sNetwork, out uint startIP, out uint endIP)
#pragma warning restore CS3001
        {
			Logger.LogDebug(">>> Entering Network2IpRange(sNetwork={SNetwork})", sNetwork);
            try
            {
                string[] elements = sNetwork.Split('/');

                uint ip = IP2Int(elements[0]);
                int bits = Convert.ToInt32(elements[1], CultureInfo.InvariantCulture);
				Logger.LogDebug("Network2IpRange(sNetwork={SNetwork}): ip → {Ip}", sNetwork, ip);
				Logger.LogDebug("Network2IpRange(sNetwork={SNetwork}): bits → {Bits}", sNetwork, bits);

                uint mask = ~(0xffffffff >> bits);

				Logger.LogDebug("Network2IpRange(sNetwork={SNetwork}): mask → {Mask}", sNetwork, mask);
                uint network = ip & mask;
                uint broadcast = network + ~mask;

                uint usableIps = (bits > 30) ? 0 : (broadcast - network - 1);

                if (usableIps <= 0)
                {
                    startIP = endIP = 0;
					Logger.LogDebug("Network2IpRange(sNetwork={SNetwork}): startIP → {StartIP}", sNetwork, startIP);
					Logger.LogDebug("Network2IpRange(sNetwork={SNetwork}): endIP → {EndIP}", sNetwork, endIP);
                }
                else
                {
                    startIP = network + 1;
                    endIP = broadcast - 1;
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in Network2IpRange(sNetwork={SNetwork}): {Arg1}", sNetwork, ex.Message);
                // catch case where IP cannot be resolved such as when debugger is attached
                startIP = 0;
                endIP = 0;
            }
			Logger.LogDebug("<<< Exiting Network2IpRange(sNetwork={SNetwork})", sNetwork);
        }

        /// <summary>Convert IP to Integer.</summary>
        /// <param name="ipNumber">The ip number.</param>
        /// <returns>IP number as integer.</returns>
#pragma warning disable CS3002 // Return type is not CLS-compliant
        public static uint IP2Int(string ipNumber)
#pragma warning restore CS3002
        {
			Logger.LogDebug(">>> Entering IP2Int(ipNumber={IpNumber})", ipNumber);
            uint ip = 0;
            string[] elements = ipNumber.Split('.');
			Logger.LogDebug("IP2Int(ipNumber={IpNumber}): ip → {Ip}", ipNumber, ip);
            if (elements.Length == 4)
            {
                ip = Convert.ToUInt32(elements[0], CultureInfo.InvariantCulture) << 24;
                ip += Convert.ToUInt32(elements[1], CultureInfo.InvariantCulture) << 16;
                ip += Convert.ToUInt32(elements[2], CultureInfo.InvariantCulture) << 8;
                ip += Convert.ToUInt32(elements[3], CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting IP2Int(ipNumber={IpNumber})", ipNumber);
            return ip;
        }

        /// <summary>Determines whether ip is in range.</summary>
        /// <param name="currentIP">The current ip.</param>
        /// <param name="startIP">The start ip.</param>
        /// <param name="subnetmask">The subnet mask.</param>
        /// <returns>True or False.</returns>
        public static bool IsIPInRange(string currentIP, string startIP, string subnetmask)
        {
			Logger.LogDebug(">>> Entering IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask})", currentIP, startIP, subnetmask);
            try
            {
                // handle case where local adapter is localhost
                if (currentIP == "::1")
                {
                    currentIP = "127.0.0.1";
                }
				Logger.LogDebug("IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask}): currentIP → {CurrentIP1}", currentIP, startIP, subnetmask, currentIP);

                // handle case where we are matching on a single IP
                if (string.IsNullOrEmpty(subnetmask))
                {
                    if (currentIP == startIP)
                    {
                        return true;
                    }
                }

                // handle case where we have to build a CIDR, convert to an IP range and compare
                string cidr = FormatAsCidr(startIP, subnetmask);
                uint fromIP, toIP;
				Logger.LogDebug("IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask}): cidr → {Cidr}", currentIP, startIP, subnetmask, cidr);
                Network2IpRange(cidr, out fromIP, out toIP);
                IPAddress currentIPAsInt = IPAddress.Parse(currentIP);
                long myip = IPtoLong(currentIPAsInt);
				Logger.LogDebug("IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask}): currentIPAsInt → {CurrentIPAsInt}", currentIP, startIP, subnetmask, currentIPAsInt);
				Logger.LogDebug("IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask}): myip → {Myip}", currentIP, startIP, subnetmask, myip);
                if (myip >= fromIP & myip <= toIP)
                {
                    return true;
                }
            }
            catch (Exception)
            {
				Logger.LogError("Exception in IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask}): {Arg3}", currentIP, startIP, subnetmask, ex.Message);
                // catch case where IP cannot be resolved such as when debugger is attached
                return false;
            }

			Logger.LogDebug("<<< Exiting IsIPInRange(currentIP={CurrentIP},startIP={StartIP},subnetmask={Subnetmask})", currentIP, startIP, subnetmask);
            return false;
        }

        /// <summary>Gets the Client IP address of the current request, from server variables if available, otherwise returns Request.UserHostAddress.</summary>
        /// <param name="request">The current http request.</param>
        /// <returns>The current client ip address.</returns>
        public static string GetClientIpAddress(HttpRequest request)
        {
			Logger.LogDebug(">>> Entering GetClientIpAddress(request={Request})", request);
            var userRequestIpAddressController = UserRequestIPAddressController.Instance;
            var ipAddress = userRequestIpAddressController.GetUserRequestIPAddress(new HttpRequestWrapper(request));

			Logger.LogDebug("GetClientIpAddress(request={Request}): ipAddress → {IpAddress}", request, ipAddress);
            // If there is no proxy, get the standard remote address
            if (string.IsNullOrWhiteSpace(ipAddress) || ipAddress.Equals("unknown", StringComparison.OrdinalIgnoreCase))
            {
                ipAddress = request.UserHostAddress;
            }

			Logger.LogDebug("<<< Exiting GetClientIpAddress(request={Request})", request);
            return ipAddress;
        }

        /// <summary>Masks from cidr.</summary>
        /// <param name="cidr">The Classless Inter-Domain Routing (cidr).</param>
        /// <returns>The mask value.</returns>
        private static long MaskFromCidr(int cidr)
        {
            return Convert.ToInt64(Math.Pow(2, 32 - cidr) - 1) ^ 4294967295L;
        }
    }
}
