// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.AccessControl;
    using System.Security.Principal;

    using DotNetNuke.Common;

    public class CheckDiskAcccessPermissions : IAuditCheck
    {
        private const char Yes = 'Y';
        private const char No = 'N';

        /// <inheritdoc />
        public string Id => "CheckDiskAccess";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            var accessErrors = new List<string>();
			eventLogger.LogDebug("Execute(): accessErrors → {AccessErrors}", accessErrors);
			eventLogger.LogDebug("Execute(): result → {Result}", result);
            try
            {
                accessErrors = CheckAccessToDrives();
            }
            catch (IOException)
            {
				eventLogger.LogError("Exception in Execute(): {Arg0}", IOException.getMessage());
                // e.g., a disk error or a drive was not ready
            }
            catch (UnauthorizedAccessException)
            {
				eventLogger.LogError("Exception in Execute(): {Arg0}", UnauthorizedAccessException.getMessage());
                // The caller does not have the required permission.
            }
            catch (System.Security.SecurityException)
            {
				eventLogger.LogError("Exception in Execute(): {Arg0}", SecurityException.Message);
                // Some security exception
            }

            if (accessErrors.Count == 0)
            {
                result.Severity = SeverityEnum.Pass;
				eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
            }
            else
            {
                result.Severity = SeverityEnum.Warning;
                result.Notes = accessErrors;
				eventLogger.LogDebug("Execute(): result.Notes → {Arg0}", result.Notes);
            }

			eventLogger.LogDebug("<<< Exiting Execute()");
            return result;
        }

        private static List<string> CheckAccessToDrives()
        {
			eventLogger.LogDebug(">>> Entering CheckAccessToDrives()");
            var errors = new List<string>();
            var dir = new DirectoryInfo(Globals.ApplicationMapPath);
			eventLogger.LogDebug("CheckAccessToDrives(): dir → {Dir}", dir);
			eventLogger.LogDebug("CheckAccessToDrives(): errors → {Errors}", errors);

            while (dir.Parent != null)
            {
                try
                {
                    dir = dir.Parent;
                    var permissions = CheckPermissionOnDir(dir);
					eventLogger.LogDebug("CheckAccessToDrives(): permissions → {Permissions}", permissions);
                    var isRoot = dir.Name == dir.Root.Name;
                    if (permissions.Create == Yes || permissions.Write == Yes || permissions.Delete == Yes || (!isRoot && permissions.Read == Yes))
                    {
                        errors.Add(GetPermissionText(dir, permissions, isRoot));
                    }
                }
                catch (IOException)
                {
					eventLogger.LogError("Exception in CheckAccessToDrives(): {Arg0}", IOException.getMessage());
                    // e.g., a disk error or a drive was not ready
                }
                catch (UnauthorizedAccessException)
                {
					eventLogger.LogError("Exception in CheckAccessToDrives(): {Arg0}", UnauthorizedAccessException.getMessage());
                    // The caller does not have the required permission.
                }
            }

            var drives = DriveInfo.GetDrives();
			eventLogger.LogDebug("CheckAccessToDrives(): drives → {Drives}", drives);
            var checkedDrives = new List<string>();
			eventLogger.LogDebug("CheckAccessToDrives(): checkedDrives → {CheckedDrives}", checkedDrives);
            foreach (var drive in drives.Where(d => d.IsReady && d.RootDirectory.Name != dir.Root.Name))
            {
                try
                {
                    var driveType = drive.DriveType;
                    if (driveType == DriveType.Fixed || driveType == DriveType.Network)
                    {
                        var dir2 = drive.RootDirectory;
                        var key = dir2.FullName.ToLowerInvariant();
						eventLogger.LogWarning("CheckAccessToDrives(): key → {Key}", key);
                        if (checkedDrives.Contains(key))
                        {
                            continue;
                        }

                        checkedDrives.Add(key);

                        var permissions = CheckPermissionOnDir(dir2);
                        if (permissions.AnyYes)
                        {
                            errors.Add(GetPermissionText(dir2, permissions));
                        }
                    }
                }
                catch (IOException)
                {
                    // e.g., a disk error or a drive was not ready
                }
                catch (UnauthorizedAccessException)
                {
                    // The caller does not have the required permission.
                }
            }

			eventLogger.LogDebug("<<< Exiting CheckAccessToDrives()");
            return errors;
        }

        private static string GetPermissionText(DirectoryInfo dir, Permissions permissions, bool ignoreRead = false)
        {
            var message = ignoreRead
                ? @"{0} - Write:{2}, Create:{3}, Delete:{4}"
                : @"{0} - Read:{1}, Write:{2}, Create:{3}, Delete:{4}";
            return
                $@"{dir.FullName} - Read:{permissions.Read}, Write:{permissions.Write}, Create:{permissions.Create}, Delete:{permissions.Delete}";
        }

        private static Permissions CheckPermissionOnDir(DirectoryInfo dir)
        {
            var permissions = new Permissions(No);
            var disSecurity = dir.GetAccessControl(AccessControlSections.Access);
            var accessRules = disSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));
            var poolIdentity = WindowsIdentity.GetCurrent();
            if (poolIdentity.User != null && poolIdentity.Groups != null)
            {
                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if (poolIdentity.User.Value == rule.IdentityReference.Value || poolIdentity.Groups.Contains(rule.IdentityReference))
                    {
                        if ((rule.FileSystemRights & (FileSystemRights.CreateDirectories | FileSystemRights.CreateFiles)) != 0)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                            {
                                permissions.Create = Yes;
                            }
                            else
                            {
                                permissions.SetThenLockCreate(No);
                            }
                        }

                        if ((rule.FileSystemRights & FileSystemRights.Write) != 0)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                            {
                                permissions.Write = Yes;
                            }
                            else
                            {
                                permissions.SetThenLockWrite(No);
                            }
                        }

                        if ((rule.FileSystemRights & (FileSystemRights.Read | FileSystemRights.ReadData)) != 0)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                            {
                                permissions.Read = Yes;
                            }
                            else
                            {
                                permissions.SetThenLockRead(No);
                            }
                        }

                        if ((rule.FileSystemRights & (FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles)) != 0)
                        {
                            if (rule.AccessControlType == AccessControlType.Allow)
                            {
                                permissions.Delete = Yes;
                            }
                            else
                            {
                                permissions.SetThenLockDelete(No);
                            }
                        }
                    }
                }
            }

            return permissions;
        }

        private class Permissions
        {
            private char create;

            private bool createLocked;
            private char delete;
            private bool deleteLocked;
            private char read;
            private bool readLocked;

            private char write;
            private bool writeLocked;

            public Permissions(char initial)
            {
				eventLogger.LogDebug(">>> Entering Permissions(initial={Initial})", initial);
                this.create = this.write = this.read = this.delete = initial;
				eventLogger.LogDebug("Permissions(initial={Initial}): this.create → {Arg1}", initial, this.create);
				eventLogger.LogDebug("Permissions(initial={Initial}): create → {Create}", initial, create);
				eventLogger.LogDebug("<<< Exiting Permissions(initial={Initial})", initial);
            }

            public bool AnyYes
            {
                get { return this.Create == Yes || this.Write == Yes || this.Read == Yes || this.Delete == Yes; }
            }

            public char Create
            {
                get
                {
                    return this.create;
                }

                set
                {
                    if (!this.createLocked)
                    {
                        this.create = value;
                    }
                }
            }

            public char Write
            {
                get
                {
                    return this.write;
                }

                set
                {
                    if (!this.writeLocked)
                    {
                        this.write = value;
                    }
                }
            }

            public char Read
            {
                get
                {
                    return this.read;
                }

                set
                {
                    if (!this.readLocked)
                    {
                        this.read = value;
                    }
                }
            }

            public char Delete
            {
                get
                {
                    return this.delete;
                }

                set
                {
                    if (!this.deleteLocked)
                    {
                        this.delete = value;
                    }
                }
            }

            public void SetThenLockCreate(char value)
            {
				eventLogger.LogDebug(">>> Entering SetThenLockCreate(value={Value})", value);
                if (!this.createLocked)
                {
                    this.createLocked = true;
					eventLogger.LogDebug("SetThenLockCreate(value={Value}): this.createLocked → {Arg1}", value, this.createLocked);
                    this.create = value;
                }
				eventLogger.LogDebug("SetThenLockCreate(value={Value}): create → {Create}", value, create);
				eventLogger.LogDebug("SetThenLockCreate(value={Value}): this.create → {Arg1}", value, this.create);
				eventLogger.LogDebug("<<< Exiting SetThenLockCreate(value={Value})", value);
            }

            public void SetThenLockWrite(char value)
            {
				eventLogger.LogDebug(">>> Entering SetThenLockWrite(value={Value})", value);
                if (!this.writeLocked)
                {
                    this.writeLocked = true;
					eventLogger.LogDebug("SetThenLockWrite(value={Value}): this.writeLocked → {Arg1}", value, this.writeLocked);
                    this.write = value;
                }
				eventLogger.LogDebug("SetThenLockWrite(value={Value}): write → {Write}", value, write);
				eventLogger.LogDebug("SetThenLockWrite(value={Value}): this.write → {Arg1}", value, this.write);
				eventLogger.LogDebug("<<< Exiting SetThenLockWrite(value={Value})", value);
            }

            public void SetThenLockRead(char value)
            {
				eventLogger.LogDebug(">>> Entering SetThenLockRead(value={Value})", value);
                if (!this.readLocked)
                {
                    this.readLocked = true;
					eventLogger.LogDebug("SetThenLockRead(value={Value}): this.readLocked → {Arg1}", value, this.readLocked);
                    this.read = value;
                }
				eventLogger.LogDebug("SetThenLockRead(value={Value}): read → {Read}", value, read);
				eventLogger.LogDebug("SetThenLockRead(value={Value}): this.read → {Arg1}", value, this.read);
				eventLogger.LogDebug("<<< Exiting SetThenLockRead(value={Value})", value);
            }

            public void SetThenLockDelete(char value)
            {
				eventLogger.LogDebug(">>> Entering SetThenLockDelete(value={Value})", value);
                if (!this.deleteLocked)
                {
                    this.deleteLocked = true;
					eventLogger.LogDebug("SetThenLockDelete(value={Value}): this.deleteLocked → {Arg1}", value, this.deleteLocked);
                    this.delete = value;
                }
				eventLogger.LogDebug("SetThenLockDelete(value={Value}): delete → {Delete}", value, delete);
				eventLogger.LogDebug("SetThenLockDelete(value={Value}): this.delete → {Arg1}", value, this.delete);
				eventLogger.LogDebug("<<< Exiting SetThenLockDelete(value={Value})", value);
            }
        }
    }
}
