// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.UserRequest;

    /// <summary>Manages counting the number of times an IP address has been seen.</summary>
    public partial class IPCount
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string TempFileExtension = ".tmp";
        private const string CacheAppRelativePath = @"~\App_Data\_ipcount\";
        private static readonly object PurgeQueuedLock = new object();
        private static readonly object FileLock = new object();
        private static string cachePath;
        private static bool purgeQueued;
        private static TimeSpan purgeInterval;

        static IPCount()
        {
			Logger.LogDebug(">>> Entering IPCount()");
            PurgeInterval = new TimeSpan(0, 10, 0);
            MaxCount = 500;
			Logger.LogDebug("IPCount(): PurgeInterval → {PurgeInterval}", PurgeInterval);
			Logger.LogDebug("IPCount(): MaxCount → {MaxCount}", MaxCount);
            CachePath = HostingEnvironment.MapPath(CacheAppRelativePath);
			Logger.LogDebug("<<< Exiting IPCount()");
			Logger.LogDebug("IPCount(): CachePath → {CachePath}", CachePath);
        }

        public static string CachePath
        {
            get
            {
                return cachePath;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);
                }

                cachePath = value;
            }
        }

        public static TimeSpan PurgeInterval
        {
            get
            {
                return purgeInterval;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                purgeInterval = value;
            }
        }

        public static int MaxCount { get; set; }

        private static DateTime LastPurge
        {
            get
            {
                var lastPurge = DateTime.Now;
                if (File.Exists(CachePath + "_lastpurge"))
                {
                    var fi = new FileInfo(CachePath + "_lastpurge");
                    lastPurge = fi.LastWriteTime;
                }
                else
                {
                    File.WriteAllText(CachePath + "_lastpurge", string.Empty);
                }

                return lastPurge;
            }

            set
            {
                File.WriteAllText(CachePath + "_lastpurge", string.Empty);
            }
        }

        public static bool CheckIp(string ipAddress)
        {
			Logger.LogDebug(">>> Entering CheckIp(ipAddress={IpAddress})", ipAddress);
            var now = DateTime.Now;
            if (!purgeQueued && now.Subtract(LastPurge) > PurgeInterval)
            {
                lock (PurgeQueuedLock)
                {
                    if (!purgeQueued)
                    {
                        purgeQueued = true;
						Logger.LogDebug("CheckIp(ipAddress={IpAddress}): purgeQueued → {PurgeQueued}", ipAddress, purgeQueued);

                        var files = new DirectoryInfo(CachePath).GetFiles();
                        var threshold = DateTime.Now.Subtract(PurgeInterval);
						Logger.LogDebug("CheckIp(ipAddress={IpAddress}): files → {Files}", ipAddress, files);
						Logger.LogDebug("CheckIp(ipAddress={IpAddress}): threshold → {Threshold}", ipAddress, threshold);
                        var toTryDeleteAgain = new List<FileInfo>();
                        foreach (var file in files)
                        {
						Logger.LogDebug("CheckIp(ipAddress={IpAddress}): toTryDeleteAgain → {ToTryDeleteAgain}", ipAddress, toTryDeleteAgain);
                            if (!file.Name.Equals("_lastpurge", StringComparison.OrdinalIgnoreCase) && file.LastWriteTime < threshold)
                            {
                                try
                                {
                                    file.Delete();
                                }
                                catch (Exception)
                                {
									Logger.LogError("Exception in CheckIp(ipAddress={IpAddress}): {Arg1}", ipAddress, ex.Message);
                                    toTryDeleteAgain.Add(file);
                                }
                            }
                        }

                        Thread.Sleep(0);
                        foreach (var file in toTryDeleteAgain)
                        {
                            try
                            {
                                file.Delete();
                            }
                            catch (Exception)
                            {
								Logger.LogError("Exception in CheckIp(ipAddress={IpAddress}): {Arg1}", ipAddress, ex.Message);
                                // do nothing at this point, try to delete file during next purge
                            }
                        }

                        LastPurge = DateTime.Now;
						Logger.LogDebug("CheckIp(ipAddress={IpAddress}): LastPurge → {LastPurge}", ipAddress, LastPurge);

                        purgeQueued = false;
                    }
                }
            }

            var path = BuildFilePath(ipAddress);
            var count = 1;
			Logger.LogDebug("CheckIp(ipAddress={IpAddress}): path → {Path}", ipAddress, path);
			Logger.LogDebug("CheckIp(ipAddress={IpAddress}): count → {Count}", ipAddress, count);
            lock (FileLock)
            {
                if (File.Exists(path))
                {
                    var strCount = File.ReadAllText(path);
                    if (int.TryParse(strCount, out count))
					Logger.LogDebug("CheckIp(ipAddress={IpAddress}): strCount → {StrCount}", ipAddress, strCount);
                    {
                        if (count > MaxCount)
                        {
                            return false;
                        }

                        count++;
                    }
                }

                File.WriteAllText(path, count.ToString(CultureInfo.InvariantCulture));
				Logger.LogDebug("<<< Exiting CheckIp(ipAddress={IpAddress})", ipAddress);
                return true;
            }
        }

        /// <summary>method to get Client ip address.</summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>IP Address of visitor.</returns>
        [DnnDeprecated(9, 2, 0, "Use UserRequestIPAddressController.Instance.GetUserRequestIPAddress")]
        public static partial string GetVisitorIPAddress(HttpContextBase context)
        {
			Logger.LogDebug(">>> Entering GetVisitorIPAddress(context={Context})", context);
			Logger.LogDebug("<<< Exiting GetVisitorIPAddress(context={Context})", context);
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(context.Request);
        }

        private static string BuildFilePath(string ipAddress)
        {
			Logger.LogDebug(">>> Entering BuildFilePath(ipAddress={IpAddress})", ipAddress);
            // it takes only the IP address without PORT for the file name
            var fileName = ipAddress.Split(':')[0];
			Logger.LogDebug("BuildFilePath(ipAddress={IpAddress}): fileName → {FileName}", ipAddress, fileName);
			Logger.LogDebug("<<< Exiting BuildFilePath(ipAddress={IpAddress})", ipAddress);
            return CachePath + fileName + TempFileExtension;
        }
    }
}
