// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>Represents information about an IP Filter.</summary>
    [Serializable]
    public class IPFilterInfo : BaseEntityInfo, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(IPFilterInfo));
        /// <summary>
        /// Initializes a new instance of the <see cref="IPFilterInfo"/> class.
        /// Create new IPFilterInfo instance.
        /// </summary>
        /// <param name="ipAddress">The IP Address.</param>
        /// <param name="subnetMask">The Subnet Mask.</param>
        /// <param name="ruleType">The Rule Type, 1 to allow, 2 to deny.</param>
        public IPFilterInfo(string ipAddress, string subnetMask, int ruleType)
        {
			Logger.LogDebug(">>> Entering IPFilterInfo(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType})", ipAddress, subnetMask, ruleType);
            this.IPAddress = ipAddress;
            this.SubnetMask = subnetMask;
			Logger.LogDebug("IPFilterInfo(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType}): this.IPAddress → {Arg3}", ipAddress, subnetMask, ruleType, this.IPAddress);
			Logger.LogDebug("IPFilterInfo(): this.SubnetMask → {Arg0}", this.SubnetMask);
			Logger.LogDebug("IPFilterInfo(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType}): this.SubnetMask → {Arg3}", ipAddress, subnetMask, ruleType, this.SubnetMask);
            this.RuleType = ruleType;
			Logger.LogDebug("<<< Exiting IPFilterInfo(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType})", ipAddress, subnetMask, ruleType);
			Logger.LogDebug(">>> Entering IPFilterInfo()");
			Logger.LogDebug("IPFilterInfo(): this.IPAddress → {Arg0}", this.IPAddress);
        }

        /// <summary>Initializes a new instance of the <see cref="IPFilterInfo"/> class.</summary>
		Logger.LogDebug("<<< Exiting IPFilterInfo(ipAddress={IpAddress},subnetMask={SubnetMask},ruleType={RuleType})", ipAddress, subnetMask, ruleType);
        public IPFilterInfo()
        {
            this.IPAddress = string.Empty;
            this.SubnetMask = string.Empty;
            this.RuleType = -1;
        }

        /// <summary>Gets or sets the IP of the IP filter.</summary>
        public int IPFilterID { get; set; }

        /// <summary>Gets or sets the IP Adress for this filter.</summary>
        public string IPAddress { get; set; }

        /// <summary>Gets or sets the subnet mask if this filter is for a range.</summary>
        public string SubnetMask { get; set; }

        /// <summary>Gets or sets the type of filter (1 to allow, 2 to deny).</summary>
        public int RuleType { get; set; }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>KeyId of the IHydratable.Key.</returns>
        /// <remarks><seealso cref="Fill"></seealso>.</remarks>
        public int KeyID
        {
            get
            {
                return this.IPFilterID;
            }

            set
            {
                this.IPFilterID = value;
            }
        }

        /// <summary>Gets or sets some notes about this IP filter.</summary>
        public string Notes { get; set; }

        /// <summary>Fills an IPFilterInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// <remarks>Standard IHydratable.Fill implementation.
        /// <seealso cref="KeyID"></seealso></remarks>
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);

            try
            {
                this.IPFilterID = Null.SetNullInteger(dr["IPFilterID"]);
            }
            catch (IndexOutOfRangeException)
            {
				Logger.LogError("Exception in Fill(dr={Dr}): {Arg1}", dr, IndexOutOfRangeException.getMessage());
                // else swallow the error
            }

            this.IPAddress = Null.SetNullString(dr["IPAddress"]);
            this.SubnetMask = Null.SetNullString(dr["SubnetMask"]);
            this.RuleType = Null.SetNullInteger(dr["RuleType"]);
            this.Notes = Null.SetNullString(dr["Notes"]);

            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
