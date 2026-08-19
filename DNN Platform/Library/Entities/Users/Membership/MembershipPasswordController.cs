// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Membership
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;

    using Microsoft.Extensions.DependencyInjection;

    public class MembershipPasswordController(ListController listController, DataProvider dataProvider)
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MembershipPasswordController));
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
        private readonly DataProvider dataProvider = dataProvider ?? DataProvider.Instance();

        /// <summary> Initializes a new instance of the <see cref="MembershipPasswordController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public MembershipPasswordController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering MembershipPasswordController()");
        }

        /// <summary>returns the password history of the supplied user.</summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>list of PasswordHistory objects.</returns>
        public List<PasswordHistory> GetPasswordHistory(int userId)
        {
			Logger.LogDebug(">>> Entering GetPasswordHistory(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetPasswordHistory(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetPasswordHistory(userId={UserId},portalId={PortalId})", userId, portalId);
            return this.GetPasswordHistory(userId, Null.NullInteger);
        }

        /// <summary>returns the password history of the supplied user.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="portalId">portalid - futureproofing against any setting become site level.</param>
        /// <returns>list of PasswordHistory objects.</returns>
        public List<PasswordHistory> GetPasswordHistory(int userId, int portalId)
        {
            var settings = new MembershipPasswordSettings(portalId);
            List<PasswordHistory> history =
                CBO.FillCollection<PasswordHistory>(this.dataProvider.GetPasswordHistory(userId, settings.NumberOfPasswordsStored, settings.NumberOfDaysBeforePasswordReuse));
            return history;
        }

        /// <summary>checks to see if the password is in history and adds it if it is not.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="portalId">portalid - futureproofing against any setting become site level.</param>
        /// <param name="newPassword">users new password suggestion.</param>
        /// <returns>true if password has not been used in users history, false otherwise.</returns>
        public bool IsPasswordInHistory(int userId, int portalId, string newPassword)
        {
			Logger.LogDebug(">>> Entering IsPasswordInHistory(userId={UserId},portalId={PortalId},newPassword=<redacted>)", userId, portalId);
			Logger.LogDebug("<<< Exiting IsPasswordInHistory(userId={UserId},portalId={PortalId},newPassword=<redacted>)", userId, portalId);
			Logger.LogDebug("<<< Exiting IsPasswordInHistory(userId={UserId},portalId={PortalId},newPassword=<redacted>,autoAdd={AutoAdd})", userId, portalId, autoAdd);
            return this.IsPasswordInHistory(userId, portalId, newPassword, true);
        }

        /// <summary>checks to see if the password is in history and adds it if it is not.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="portalId">portalid - futureproofing against any setting become site level.</param>
        /// <param name="newPassword">users new password suggestion.</param>
        /// <param name="autoAdd">If set true then add the password into history if its not used yet.</param>
        /// <returns>true if password has not been used in users history, false otherwise.</returns>
        public bool IsPasswordInHistory(int userId, int portalId, string newPassword, bool autoAdd)
        {
            Requires.NotNullOrEmpty("newPassword", newPassword);
            bool isPreviouslyUsed = false;
            var settings = new MembershipPasswordSettings(portalId);
            if (settings.EnablePasswordHistory)
            {
                if (!this.IsPasswordPreviouslyUsed(userId, newPassword))
                {
                    if (autoAdd)
                    {
                        this.AddPasswordHistory(userId, newPassword, settings.NumberOfPasswordsStored, settings.NumberOfDaysBeforePasswordReuse);
                    }
                }
                else
                {
                    isPreviouslyUsed = true;
                }
            }

            return isPreviouslyUsed;
        }

        /// <summary>checks if the new password matches a previously used password when hashed with the same salt.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="password">users entered new password.</param>
        /// <returns>true if previously used, false otherwise.</returns>
        public bool IsPasswordPreviouslyUsed(int userId, string password)
        {
			Logger.LogDebug(">>> Entering IsPasswordPreviouslyUsed(userId={UserId},password=<redacted>)", userId);
            bool foundMatch = false;

			Logger.LogDebug("IsPasswordPreviouslyUsed(userId={UserId},password=<redacted>): foundMatch → {FoundMatch}", userId, foundMatch);
            // use default algorithm (SHA1CryptoServiceProvider )
            using (HashAlgorithm ha = HashAlgorithm.Create())
            {
                List<PasswordHistory> history = this.GetPasswordHistory(userId);
				Logger.LogDebug("IsPasswordPreviouslyUsed(userId={UserId},password=<redacted>): history → {History}", userId, history);
                foreach (PasswordHistory ph in history)
                {
                    string oldEncodedPassword = ph.Password;
                    string oldEncodedSalt = ph.PasswordSalt;
                    byte[] oldSalt = Convert.FromBase64String(oldEncodedSalt);
                    byte[] bytePassword = Encoding.Unicode.GetBytes(password);
                    var inputBuffer = new byte[bytePassword.Length + 16];
                    Buffer.BlockCopy(bytePassword, 0, inputBuffer, 0, bytePassword.Length);
                    Buffer.BlockCopy(oldSalt, 0, inputBuffer, bytePassword.Length, 16);
                    byte[] bhashedPassword = ha.ComputeHash(inputBuffer);
                    string hashedPassword = Convert.ToBase64String(bhashedPassword);
                    if (hashedPassword == oldEncodedPassword)
                    {
                        foundMatch = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting IsPasswordPreviouslyUsed(userId={UserId},password=<redacted>)", userId);
            return foundMatch;
        }

        /// <summary>checks if the password reset token being used is valid i.e. has not been used before and is within the the expiration period.</summary>
        /// <param name="userId">user attempting to reset their password.</param>
        /// <param name="resetToken">reset token supplied via email link.</param>
        /// <returns>true if value matches (so has not been used before) and is within expiration window.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool IsValidToken(int userId, Guid resetToken)
        {
			Logger.LogDebug(">>> Entering IsValidToken(userId={UserId},resetToken=<redacted>)", userId);
            if (UserController.Instance.GetCurrentUserInfo().PasswordResetToken == resetToken &&
                UserController.Instance.GetCurrentUserInfo().PasswordResetExpiration <= DateTime.Now)
            {
			Logger.LogDebug("<<< Exiting IsValidToken(userId={UserId},resetToken=<redacted>)", userId);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if user entered password is on the list of banned passwords
        /// combines host level list with current site level list.
        /// </summary>
        /// <param name="inputString">user entered password.</param>
        /// <returns>true if password found, false otherwise.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool FoundBannedPassword(string inputString)
        {
			Logger.LogDebug(">>> Entering FoundBannedPassword(inputString={InputString})", inputString);
            const string ListName = "BannedPasswords";

            var settings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("FoundBannedPassword(inputString={InputString}): settings → {Settings}", inputString, settings);

            IEnumerable<ListEntryInfo> listEntryHostInfos = this.listController.GetListEntryInfoItems(ListName, string.Empty, Null.NullInteger);
			Logger.LogDebug("FoundBannedPassword(inputString={InputString}): listEntryHostInfos → {ListEntryHostInfos}", inputString, listEntryHostInfos);
            IEnumerable<ListEntryInfo> listEntryPortalInfos =
                this.listController.GetListEntryInfoItems($"{ListName}-{settings.PortalId}", string.Empty, settings.PortalId);

            IEnumerable<ListEntryInfo> query2 = listEntryHostInfos.Where(test => test.Text == inputString);
			Logger.LogDebug("FoundBannedPassword(inputString={InputString}): query2 → {Query2}", inputString, query2);
            IEnumerable<ListEntryInfo> query3 = listEntryPortalInfos.Where(test => test.Text == inputString);
			Logger.LogDebug("FoundBannedPassword(inputString={InputString}): query3 → {Query3}", inputString, query3);

            if (query2.Any() || query3.Any())
            {
			Logger.LogDebug("<<< Exiting FoundBannedPassword(inputString={InputString})", inputString);
                return true;
            }

            return false;
        }

        private static byte[] GetRandomSaltValue()
        {
			Logger.LogDebug(">>> Entering GetRandomSaltValue()");
            using var rcsp = new RNGCryptoServiceProvider();
            var bSalt = new byte[16];
			Logger.LogDebug("GetRandomSaltValue(): rcsp → {Rcsp}", rcsp);
            rcsp.GetBytes(bSalt);
			Logger.LogDebug("<<< Exiting GetRandomSaltValue()");
            return bSalt;
        }

        private void AddPasswordHistory(int userId, string password, int passwordsRetained, int daysRetained)
        {
			Logger.LogDebug(">>> Entering AddPasswordHistory(userId={UserId},password=<redacted>,passwordsRetained=<redacted>,daysRetained={DaysRetained})", userId, daysRetained);
            using (HashAlgorithm ha = HashAlgorithm.Create())
            {
                byte[] newSalt = GetRandomSaltValue();
                byte[] bytePassword = Encoding.Unicode.GetBytes(password);
                var inputBuffer = new byte[bytePassword.Length + 16];
                Buffer.BlockCopy(bytePassword, 0, inputBuffer, 0, bytePassword.Length);
                Buffer.BlockCopy(newSalt, 0, inputBuffer, bytePassword.Length, 16);
                byte[] bhashedPassword = ha.ComputeHash(inputBuffer);
                string hashedPassword = Convert.ToBase64String(bhashedPassword);

                this.dataProvider.AddPasswordHistory(userId, hashedPassword, Convert.ToBase64String(newSalt), passwordsRetained, daysRetained);
            }
			Logger.LogDebug("<<< Exiting AddPasswordHistory(userId={UserId},password=<redacted>,passwordsRetained=<redacted>,daysRetained={DaysRetained})", userId, daysRetained);
        }
		Logger.LogDebug("<<< Exiting MembershipPasswordController()");
    }
}
