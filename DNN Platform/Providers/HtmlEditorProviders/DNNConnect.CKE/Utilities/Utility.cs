// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    using DNNConnect.CKEditorProvider.Constants;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Utility Class for various helper Functions.</summary>
    public static class Utility
    {
        /// <summary>Path Validator Expression.</summary>
        private static readonly string PathValidatorExpression = string.Format(
            "^[^{0}]+$",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>Path Validator Regex.</summary>
        private static readonly Regex PathValidator = new Regex(PathValidatorExpression, RegexOptions.Compiled);

        /// <summary>FileName Validator Expression.</summary>
        private static readonly string FileNameValidatorExpression = string.Format(
            "^[^{0}]+$",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>FileName Validator Regex.</summary>
        private static readonly Regex FileNameValidator = new Regex(FileNameValidatorExpression, RegexOptions.Compiled);

        /// <summary>Path Cleaner Expression.</summary>
        private static readonly string PathCleanerExpression = string.Format(
            "[{0}]",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidPathChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>Path Cleaner Regex.</summary>
        private static readonly Regex PathCleaner = new Regex(PathCleanerExpression, RegexOptions.Compiled);

        /// <summary>FileName Cleaner Expression.</summary>
        private static readonly string FileNameCleanerExpression = string.Format(
            "[{0}]",
            string.Join(string.Empty, Array.ConvertAll(Path.GetInvalidFileNameChars(), x => Regex.Escape(x.ToString()))));

        /// <summary>FileName Cleaner Regex.</summary>
        private static readonly Regex FileNameCleaner = new Regex(FileNameCleanerExpression, RegexOptions.Compiled);

        /// <summary>Check if Object is a Unit.</summary>
        /// <param name="valueToCheck">
        /// Object to Check.
        /// </param>
        /// <returns>
        /// Returns if Object is a Unit.
        /// </returns>
        public static bool IsUnit(object valueToCheck)
        {
			eventLogger.LogError(">>> Entering IsUnit(valueToCheck={ValueToCheck})", valueToCheck);
            string sInputValue = Convert.ToString(valueToCheck);

            string sNewValue;

            if (sInputValue.EndsWith("px"))
            {
                sNewValue = sInputValue.Replace("px", string.Empty);
				eventLogger.LogError("IsUnit(valueToCheck={ValueToCheck}): sNewValue → {SNewValue}", valueToCheck, sNewValue);
				eventLogger.LogDebug("IsUnit(valueToCheck={ValueToCheck}): sNewValue → {SNewValue}", valueToCheck, sNewValue);

                if (IsNumeric(sNewValue))
                {
                    return true;
                }
            }
            else if (sInputValue.EndsWith("%"))
            {
                sNewValue = sInputValue.Replace("%", string.Empty);

                if (IsNumeric(sNewValue))
                {
                    return true;
                }
            }
            else
            {
                if (IsNumeric(sInputValue))
                {
                    return true;
                }
            }

			eventLogger.LogWarning("<<< Exiting IsUnit(valueToCheck={ValueToCheck})", valueToCheck);
            return false;
        }

        /// <summary>Check if Object is a Number.</summary>
        /// <param name="valueToCheck">
        /// Object to Check.
        /// </param>
        /// <returns>
        /// Returns boolean Value.
        /// </returns>
        public static bool IsNumeric(object valueToCheck)
        {
			eventLogger.LogError(">>> Entering IsNumeric(valueToCheck={ValueToCheck})", valueToCheck);
            double resultValue;
            string inputValue = Convert.ToString(valueToCheck);

            bool numeric = double.TryParse(inputValue, NumberStyles.Any, null, out resultValue);

			eventLogger.LogWarning("<<< Exiting IsNumeric(valueToCheck={ValueToCheck})", valueToCheck);
            return numeric;
        }

        /// <summary>Validates the <paramref name="path"/>.</summary>
        /// <param name="path">
        /// The <paramref name="path"/>.
        /// </param>
        /// <returns>
        /// The validate <paramref name="path"/>.
        /// </returns>
        public static bool ValidatePath(string path)
        {
			eventLogger.LogError(">>> Entering ValidatePath(path={Path})", path);
            eventLogger.LogWarning("<<< Exiting ValidatePath(path={Path})", path);
            return PathValidator.IsMatch(path);
        }

        /// <summary>Validates the name of the file.</summary>
        /// <param name="fileName">
        /// Name of the file.
        /// </param>
        /// <returns>
        /// The validate file name.
        /// </returns>
        public static bool ValidateFileName(string fileName)
        {
			eventLogger.LogError(">>> Entering ValidateFileName(fileName={FileName})", fileName);
            eventLogger.LogWarning("<<< Exiting ValidateFileName(fileName={FileName})", fileName);
            return FileNameValidator.IsMatch(fileName);
        }

        /// <summary>Cleans the <paramref name="path"/>.</summary>
        /// <param name="path">
        /// The <paramref name="path"/>.
        /// </param>
        /// <returns>
        /// The clean <paramref name="path"/>.
        /// </returns>
        public static string CleanPath(string path)
        {
			eventLogger.LogError(">>> Entering CleanPath(path={Path})", path);
            eventLogger.LogWarning("<<< Exiting CleanPath(path={Path})", path);
            return PathCleaner.Replace(path, string.Empty);
        }

        /// <summary>Cleans the name of the file.</summary>
        /// <param name="fileName">
        /// Name of the file.
        /// </param>
        /// <returns>
        /// The clean file name.
        /// </returns>
        public static string CleanFileName(string fileName)
        {
			eventLogger.LogError(">>> Entering CleanFileName(fileName={FileName})", fileName);
            eventLogger.LogWarning("<<< Exiting CleanFileName(fileName={FileName})", fileName);
            return FileNameCleaner.Replace(fileName, string.Empty);
        }

        /// <summary>Converts the Unicode chars to its ASCII equivalent.</summary>
        /// <param name="input">The <paramref name="input"/>.</param>
        /// <returns>The ASCII equivalent output.</returns>
        public static string ConvertUnicodeChars(string input)
        {
			eventLogger.LogError(">>> Entering ConvertUnicodeChars(input={Input})", input);
            Regex regA = new Regex("[ã|à|â|ä|á|å]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regA → {RegA}", input, regA);
            Regex regAA = new Regex("[Ã|À|Â|Ä|Á|Å]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regAA → {RegAA}", input, regAA);
            Regex regE = new Regex("[é|è|ê|ë]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regE → {RegE}", input, regE);
            Regex regEE = new Regex("[É|È|Ê|Ë]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regEE → {RegEE}", input, regEE);
            Regex regI = new Regex("[í|ì|î|ï]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regI → {RegI}", input, regI);
            Regex regII = new Regex("[Í|Ì|Î|Ï]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regII → {RegII}", input, regII);
            Regex regO = new Regex("[õ|ò|ó|ô|ö]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regO → {RegO}", input, regO);
            Regex regOO = new Regex("[Õ|Ó|Ò|Ô|Ö]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regOO → {RegOO}", input, regOO);
            Regex regU = new Regex("[ù|ú|û|ü|µ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regU → {RegU}", input, regU);
            Regex regUU = new Regex("[Ü|Ú|Ù|Û]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regUU → {RegUU}", input, regUU);
            Regex regY = new Regex("[ý|ÿ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regY → {RegY}", input, regY);
            Regex regYY = new Regex("[Ý]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regYY → {RegYY}", input, regYY);
            Regex regAE = new Regex("[æ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regAE → {RegAE}", input, regAE);
            Regex regAEAE = new Regex("[Æ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regAEAE → {RegAEAE}", input, regAEAE);
            Regex regOE = new Regex("[œ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regOE → {RegOE}", input, regOE);
            Regex regOEOE = new Regex("[Œ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regOEOE → {RegOEOE}", input, regOEOE);
            Regex regC = new Regex("[ç]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regC → {RegC}", input, regC);
            Regex regCC = new Regex("[Ç]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regCC → {RegCC}", input, regCC);
            Regex regDD = new Regex("[Ð]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regDD → {RegDD}", input, regDD);
            Regex regN = new Regex("[ñ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regN → {RegN}", input, regN);
            Regex regNN = new Regex("[Ñ]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regNN → {RegNN}", input, regNN);
            Regex regS = new Regex("[š]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regS → {RegS}", input, regS);
            Regex regSS = new Regex("[Š]");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regSS → {RegSS}", input, regSS);
            Regex regDot = new Regex("[\uFF0E]");                      // 'U+FF0E' = full-width dot
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regDot → {RegDot}", input, regDot);
            Regex regBackslash = new Regex("[\uFF3C|\u29F5|\uFE68]");  // backslash-like characters
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): regBackslash → {RegBackslash}", input, regBackslash);
            input = regA.Replace(input, "a");
			eventLogger.LogError("ConvertUnicodeChars(input={Input}): input → {Input1}", input, input);
            input = regAA.Replace(input, "A");
            input = regE.Replace(input, "e");
            input = regEE.Replace(input, "E");
            input = regI.Replace(input, "i");
            input = regII.Replace(input, "I");
            input = regO.Replace(input, "o");
            input = regOO.Replace(input, "O");
            input = regU.Replace(input, "u");
            input = regUU.Replace(input, "U");
            input = regY.Replace(input, "y");
            input = regYY.Replace(input, "Y");
            input = regAE.Replace(input, "ae");
            input = regAEAE.Replace(input, "AE");
            input = regOE.Replace(input, "oe");
            input = regOEOE.Replace(input, "OE");
            input = regC.Replace(input, "c");
            input = regCC.Replace(input, "C");
            input = regDD.Replace(input, "D");
            input = regN.Replace(input, "n");
            input = regNN.Replace(input, "N");
            input = regS.Replace(input, "s");
            input = regSS.Replace(input, "S");
            input = regDot.Replace(input, ".");         // full-width dot       -> ASCII period
            eventLogger.LogWarning("<<< Exiting ConvertUnicodeChars(input={Input})", input);
            input = regBackslash.Replace(input, @"\");  // backslash-like chars -> ASCII backslash

            input = input.Replace("�", string.Empty);

            char[] invalidChars = Path.GetInvalidFileNameChars();
            input = string.Join("_", input.Split(invalidChars));

            input = input.Replace("?", string.Empty); // replace the unknown char which created in above.
            input = input.Replace("�", string.Empty);
            input = input.Replace("\t", string.Empty);
            input = input.Replace("@", "at");
            input = input.Replace("\r", string.Empty);
            input = input.Replace("\n", string.Empty);
            input = input.Replace("+", "_");

            return input;
        }

        /// <summary>Checks if user has write access to the folder.</summary>
        /// <param name="folderId">The folder id.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>
        /// Returns if the user has write access to the folder.
        /// </returns>
        public static bool CheckIfUserHasFolderWriteAccess(int folderId, IPortalSettings portalSettings)
        {
			eventLogger.LogError(">>> Entering CheckIfUserHasFolderWriteAccess(folderId={FolderId},portalSettings={PortalSettings})", folderId, portalSettings);
            eventLogger.LogWarning("<<< Exiting CheckIfUserHasFolderWriteAccess(folderId={FolderId},portalSettings={PortalSettings})", folderId, portalSettings);
            return CheckIfUserHasFolderAccess(folderId, portalSettings, "WRITE");
        }

        /// <summary>Checks if user has read access to the folder.</summary>
        /// <param name="folderId">The folder id.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>
        /// Returns if the user has write access to the folder.
        /// </returns>
        public static bool CheckIfUserHasFolderReadAccess(int folderId, IPortalSettings portalSettings)
        {
			eventLogger.LogError(">>> Entering CheckIfUserHasFolderReadAccess(folderId={FolderId},portalSettings={PortalSettings})", folderId, portalSettings);
            eventLogger.LogWarning("<<< Exiting CheckIfUserHasFolderReadAccess(folderId={FolderId},portalSettings={PortalSettings})", folderId, portalSettings);
            return CheckIfUserHasFolderAccess(folderId, portalSettings, "READ");
        }

        /// <summary>Converts a File Path to a Folder Info.</summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns>
        /// Returns the Folder Info.
        /// </returns>
        public static IFolderInfo ConvertFilePathToFolderInfo(string folderPath, IPortalSettings portalSettings)
        {
			eventLogger.LogError(">>> Entering ConvertFilePathToFolderInfo(folderPath={FolderPath},portalSettings={PortalSettings})", folderPath, portalSettings);
            if (!string.IsNullOrEmpty(portalSettings.HomeDirectoryMapPath) && folderPath.Length >= portalSettings.HomeDirectoryMapPath.Length)
            {
                folderPath = folderPath.Substring(portalSettings.HomeDirectoryMapPath.Length).Replace(@"\", "/");
            }
            else
            {
				eventLogger.LogError("ConvertFilePathToFolderInfo(folderPath={FolderPath},portalSettings={PortalSettings}): folderPath → {FolderPath1}", folderPath, portalSettings, folderPath);
                folderPath = folderPath.Replace(@"\", "/");
            }

			eventLogger.LogWarning("<<< Exiting ConvertFilePathToFolderInfo(folderPath={FolderPath},portalSettings={PortalSettings})", folderPath, portalSettings);
            return FolderManager.Instance.GetFolder(portalSettings.PortalId, folderPath);
        }

        /// <summary>Converts a File Path to a file Info.</summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>
        /// Returns the file Info.
        /// </returns>
        public static int ConvertFilePathToFileId(string filePath, int portalID)
        {
			eventLogger.LogError(">>> Entering ConvertFilePathToFileId(filePath={FilePath},portalID={PortalID})", filePath, portalID);
            var fileName = Path.GetFileName(filePath);

			eventLogger.LogError("ConvertFilePathToFileId(filePath={FilePath},portalID={PortalID}): fileName → {FileName}", filePath, portalID, fileName);
            if (fileName == null)
            {
                return Null.NullInteger;
            }

            var folderPath = filePath.Substring(0, filePath.LastIndexOf(fileName, StringComparison.Ordinal));
			eventLogger.LogError("ConvertFilePathToFileId(filePath={FilePath},portalID={PortalID}): folderPath → {FolderPath}", filePath, portalID, folderPath);
            var folder = FolderManager.Instance.GetFolder(portalID, folderPath);
			eventLogger.LogError("ConvertFilePathToFileId(filePath={FilePath},portalID={PortalID}): folder → {Folder}", filePath, portalID, folder);

            var file = FileManager.Instance.GetFile(folder, fileName);
			eventLogger.LogError("ConvertFilePathToFileId(filePath={FilePath},portalID={PortalID}): file → {File}", filePath, portalID, file);

			eventLogger.LogWarning("<<< Exiting ConvertFilePathToFileId(filePath={FilePath},portalID={PortalID})", filePath, portalID);
            return file.FileId;
        }

        /// <summary>Sort a List Ascending.</summary>
        /// <param name="source">
        /// The <paramref name="source"/> List.
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <typeparam name="TSource">
        /// The TSource List Item.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The TValue List item.
        /// </typeparam>
        public static void SortAscending<TSource, TValue>(List<TSource> source, Func<TSource, TValue> selector)
        {
			eventLogger.LogError(">>> Entering SortAscending(source={Source},selector={Selector})", source, selector);
            var comparer = Comparer<TValue>.Default;
            source.Sort((x, y) => comparer.Compare(selector(x), selector(y)));
			eventLogger.LogWarning("<<< Exiting SortAscending(source={Source},selector={Selector})", source, selector);
        }

        /// <summary>Sort a List Descending.</summary>
        /// <param name="source">
        /// The <paramref name="source"/> List.
        /// </param>
        /// <param name="selector">
        /// The selector.
        /// </param>
        /// <typeparam name="TSource">
        /// The TSource List Item.
        /// </typeparam>
        /// <typeparam name="TValue">
        /// The TValue List item.
        /// </typeparam>
        public static void SortDescending<TSource, TValue>(List<TSource> source, Func<TSource, TValue> selector)
        {
			eventLogger.LogError(">>> Entering SortDescending(source={Source},selector={Selector})", source, selector);
            var comparer = Comparer<TValue>.Default;
            source.Sort((x, y) => comparer.Compare(selector(y), selector(x)));
			eventLogger.LogWarning("<<< Exiting SortDescending(source={Source},selector={Selector})", source, selector);
        }

        /// <summary>Determines whether user is in the specified role.</summary>
        /// <param name="roles">The roles.</param>
        /// <param name="settings">The settings.</param>
        /// <returns>
        ///   <see langword="true"/> if [is in roles] [the specified roles]; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsInRoles(string roles, IPortalSettings settings)
        {
			eventLogger.LogError(">>> Entering IsInRoles(roles={Roles},settings={Settings})", roles, settings);
            var objUserInfo = UserController.Instance.GetCurrentUserInfo();

			eventLogger.LogError("IsInRoles(roles={Roles},settings={Settings}): objUserInfo → {ObjUserInfo}", roles, settings, objUserInfo);
            var isInRoles = objUserInfo.IsSuperUser;
			eventLogger.LogError("IsInRoles(roles={Roles},settings={Settings}): isInRoles → {IsInRoles}", roles, settings, isInRoles);

            if (isInRoles)
            {
                // super user
                return true;
            }

            var context = HttpContext.Current;

            foreach (var role in roles.Split(';').Where(role => !string.IsNullOrEmpty(role)))
            {
                if (role.StartsWith("!"))
                {
                    var denyRole = role.Replace("!", string.Empty);
					eventLogger.LogError("IsInRoles(roles={Roles},settings={Settings}): denyRole → {DenyRole}", roles, settings, denyRole);

                    if ((!context.Request.IsAuthenticated && denyRole.Equals(Globals.glbRoleUnauthUserName)) || denyRole.Equals(Globals.glbRoleAllUsersName) ||
                        objUserInfo.IsInRole(denyRole))
                    {
                        break;
                    }
                }
                else
                {
                    if (settings.PortalId == objUserInfo.PortalID
                        && settings.AdministratorId == objUserInfo.UserID)
                    {
                        isInRoles = true;
                        break;
                    }

                    if ((context.Request.IsAuthenticated || !role.Equals(Globals.glbRoleUnauthUserName))
                        && !role.Equals(Globals.glbRoleAllUsersName) && !objUserInfo.IsInRole(role))
                    {
                        continue;
                    }

                    isInRoles = true;
                    break;
                }
            }

			eventLogger.LogWarning("<<< Exiting IsInRoles(roles={Roles},settings={Settings})", roles, settings);
            return isInRoles;
        }

        /// <summary>Finds the control.</summary>
        /// <typeparam name="T">The Control Type.</typeparam>
        /// <param name="startingControl">The starting control.</param>
        /// <param name="id">The <paramref name="id"/>.</param>
        /// <returns>Returns the Control.</returns>
        public static T FindControl<T>(Control startingControl, string id)
            where T : Control
        {
			eventLogger.LogError(">>> Entering FindControl(startingControl={StartingControl},id={Id})", startingControl, id);
            // this is null by default
            T found = null;
			eventLogger.LogError("FindControl(startingControl={StartingControl},id={Id}): found → {Found}", startingControl, id, found);

            int controlCount = startingControl.Controls.Count;

            if (controlCount > 0)
            {
                for (int i = 0; i < controlCount; i++)
                {
                    Control activeControl = startingControl.Controls[i];

                    if (activeControl is T)
                    {
                        found = startingControl.Controls[i] as T;

                        if (found.ID.Contains(id, StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }

                        found = null;
                    }
                    else
                    {
                        found = FindControl<T>(activeControl, id);

                        if (found != null)
                        {
                            break;
                        }
                    }
                }
            }

			eventLogger.LogWarning("<<< Exiting FindControl(startingControl={StartingControl},id={Id})", startingControl, id);
            return found;
        }

        /// <summary>Gets the size of the max upload.</summary>
        /// <param name="inkilobytes">if set to <see langword="true"/> Returns Value as kilobyte otherwise as byte.</param>
        /// <returns>
        /// Returns the Max. Upload Size.
        /// </returns>
        public static long GetMaxUploadSize(bool inkilobytes = false)
        {
			eventLogger.LogError(">>> Entering GetMaxUploadSize(inkilobytes={Inkilobytes})", inkilobytes);
            long result;

            try
            {
                var section =
                    HttpContext.Current.GetSection("system.web/httpRuntime") as
                    HttpRuntimeSection;
                result = section.MaxRequestLength;
				eventLogger.LogError("GetMaxUploadSize(inkilobytes={Inkilobytes}): result → {Result}", inkilobytes, result);
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in GetMaxUploadSize(inkilobytes={Inkilobytes}): {Arg1}", inkilobytes, Exception.Message);
                result = 0;
            }

			eventLogger.LogWarning("<<< Exiting GetMaxUploadSize(inkilobytes={Inkilobytes})", inkilobytes);
            return inkilobytes ? result : (result * 1024);
        }

        /// <summary>Converts the enum value for Link Protocol to the string value used for CKE config.</summary>
        /// <param name="protocol">The protocol enum value.</param>
        /// <returns>The protocol string value.</returns>
        public static string ToSettingValue(this LinkProtocol protocol)
        {
			eventLogger.LogError(">>> Entering ToSettingValue(protocol={Protocol})", protocol);
            switch (protocol)
            {
                case LinkProtocol.Http:
                    return "http://";
                case LinkProtocol.Ftp:
                    return "ftp://";
                case LinkProtocol.News:
                    return "news://";
                case LinkProtocol.Other:
                    return string.Empty;
                case LinkProtocol.Https:
                default:
					eventLogger.LogWarning("<<< Exiting ToSettingValue(protocol={Protocol})", protocol);
                    return "https://";
            }
        }

        public static IFolderInfo EnsureGetFolder(int portalId, string folderPath)
        {
			eventLogger.LogError(">>> Entering EnsureGetFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            if (FolderManager.Instance.FolderExists(portalId, folderPath))
            {
                return FolderManager.Instance.GetFolder(portalId, folderPath);
            }

            if (!string.IsNullOrEmpty(folderPath))
            {
                var newFolderPath = string.Empty;
				eventLogger.LogError("EnsureGetFolder(portalId={PortalId},folderPath={FolderPath}): newFolderPath → {NewFolderPath}", portalId, folderPath, newFolderPath);
                IFolderInfo retval = null;
				eventLogger.LogError("EnsureGetFolder(portalId={PortalId},folderPath={FolderPath}): retval → {Retval}", portalId, folderPath, retval);
                foreach (var folderName in folderPath.Split('/'))
                {
                    newFolderPath = $"{newFolderPath}{folderName}/";
                    retval = FolderManager.Instance.AddFolder(portalId, newFolderPath);
                }

                return retval;
            }

			eventLogger.LogWarning("<<< Exiting EnsureGetFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            return null;
        }

        private static bool CheckIfUserHasFolderAccess(int folderId, IPortalSettings portalSettings, string permissionKey)
        {
			eventLogger.LogError(">>> Entering CheckIfUserHasFolderAccess(folderId={FolderId},portalSettings={PortalSettings},permissionKey={PermissionKey})", folderId, portalSettings, permissionKey);
            try
            {
                var checkFolder = folderId.Equals(-1)
                    ? ConvertFilePathToFolderInfo(portalSettings.HomeDirectoryMapPath, portalSettings)
                    : FolderManager.Instance.GetFolder(folderId);

                return FolderPermissionController.HasFolderPermission(checkFolder.FolderPermissions, permissionKey);
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in CheckIfUserHasFolderAccess(folderId={FolderId},portalSettings={PortalSettings},permissionKey={PermissionKey}): {Arg3}", folderId, portalSettings, permissionKey, Exception.Message);
				eventLogger.LogWarning("<<< Exiting CheckIfUserHasFolderAccess(folderId={FolderId},portalSettings={PortalSettings},permissionKey={PermissionKey})", folderId, portalSettings, permissionKey);
                return false;
            }
        }

        private static bool Contains(this string @this, string value, StringComparison comparisonType)
        {
			eventLogger.LogError(">>> Entering Contains(value={Value},comparisonType={ComparisonType})", value, comparisonType);
            eventLogger.LogWarning("<<< Exiting Contains(value={Value},comparisonType={ComparisonType})", value, comparisonType);
            return @this.IndexOf(value, comparisonType) > -1;
        }
    }
}
