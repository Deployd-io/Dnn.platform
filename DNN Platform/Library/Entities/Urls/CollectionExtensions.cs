// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Collections.Internal;

    public static class CollectionExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CollectionExtensions));
        public static void LoadFromXmlFile(this Dictionary<int, List<ParameterReplaceAction>> actions, string fileName, int portalId, bool portalSpecific, ref List<string> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            messages = [];
            if (File.Exists(fileName))
            {
                var rdr = new XmlTextReader(fileName)
                {
                    XmlResolver = null,
                    DtdProcessing = DtdProcessing.Prohibit,
                };
                while (rdr.Read())
                {
                    switch (rdr.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (rdr.Name == "parameterReplace")
                            {
                                // now set up the action
                                string portalIdRaw = rdr.GetAttribute("portalId");
                                int rulePortalId = -1;
                                if (portalIdRaw != null)
                                {
                                    if (!int.TryParse(portalIdRaw, out rulePortalId))
                                    {
                                        rulePortalId = -1;
                                    }
                                }

                                // 807 : if portal specific then import all regardless of portal id specified
                                if (rulePortalId == portalId || rulePortalId == -1 || portalSpecific)
                                {
                                    int actionCount = 0;
                                    string tabIdRaw = rdr.GetAttribute("tabIds") ?? rdr.GetAttribute("tabId");
                                    string tabNames = rdr.GetAttribute("tabNames");
                                    string name = rdr.GetAttribute("name");
                                    List<int> tabIds = XmlHelpers.TabIdsFromAttributes(tabIdRaw, tabNames, portalId, ref messages);
                                    foreach (int tabId in tabIds)
                                    {
                                        var action = new ParameterReplaceAction
                                        {
                                            LookFor = rdr.GetAttribute("lookFor"),
                                            ReplaceWith = rdr.GetAttribute("replaceWith"),
                                            PortalId = portalId,
                                            Name = name,
                                            TabId = tabId,
                                        };
                                        string changeToSiteRootRaw = rdr.GetAttribute("changeToSiteRoot");
                                        if (bool.TryParse(changeToSiteRootRaw, out var changeToSiteRoot))
                                        {
                                            action.ChangeToSiteRoot = changeToSiteRoot;
                                        }

                                        List<ParameterReplaceAction> tabActionCol;
                                        if (actions.TryGetValue(action.TabId, out var parameterReplaceActions))
                                        {
                                            tabActionCol = parameterReplaceActions;
                                        }
                                        else
                                        {
                                            tabActionCol = [];
                                            actions.Add(action.TabId, tabActionCol);
                                        }

                                        tabActionCol.Add(action);

                                        actionCount++;
                                        messages.Add(name + " replace actions added:" + actionCount.ToString(CultureInfo.InvariantCulture));
                                    }
                                }
                            }

                            break;

                        case XmlNodeType.EndElement:
                            break;
                    }
                }

                rdr.Close();
            }
            else
            {
                messages.Add("File not Found: " + fileName);
            }
        }

        /// <summary>Returns all the redirect rules for the specified portal.</summary>
        /// <param name="actions">A dictionary of <see cref="ParameterRedirectAction"/> instances mapped by tab ID.</param>
        /// <param name="fileName">The file path of the XML file.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="portalSpecific">If true, all rules belong to supplied portalId, even if not specified.</param>
        /// <param name="messages">A list to which debugging messages will be added.</param>
        /// <remarks>807 : change to allow specification of assumption that all rules belong to the supplied portal.</remarks>
        public static void LoadFromXmlFile(this Dictionary<int, List<ParameterRedirectAction>> actions, string fileName, int portalId, bool portalSpecific, ref List<string> messages)
        {
			Logger.LogError(">>> Entering LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages})", actions, fileName, portalId, portalSpecific, messages);
            if (messages == null)
            {
                messages = new List<string>();
				Logger.LogError("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): messages → {Messages1}", actions, fileName, portalId, portalSpecific, messages, messages);
            }

            if (File.Exists(fileName))
            {
                var rdr = new XmlTextReader(fileName)
                {
                    XmlResolver = null,
                    DtdProcessing = DtdProcessing.Prohibit,
                };
                while (rdr.Read())
                {
                    switch (rdr.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (rdr.Name == "parameterRedirect")
                            {
                                var tabMessages = new List<string>();
								Logger.LogError("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): tabMessages → {TabMessages}", actions, fileName, portalId, portalSpecific, messages, tabMessages);
                                int actionCount = 0;

                                // now set up the action
                                string portalIdRaw = rdr.GetAttribute("rulePortalId");
								Logger.LogError("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): portalIdRaw → {PortalIdRaw}", actions, fileName, portalId, portalSpecific, messages, portalIdRaw);
                                if (string.IsNullOrEmpty(portalIdRaw))
                                {
                                    portalIdRaw = rdr.GetAttribute("portalId");
                                }

                                int rulePortalId = -1;
								Logger.LogError("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): rulePortalId → {RulePortalId}", actions, fileName, portalId, portalSpecific, messages, rulePortalId);
                                if (portalIdRaw != null)
                                {
                                    if (!int.TryParse(portalIdRaw, out rulePortalId))
                                    {
                                        rulePortalId = -1;
                                    }
                                }

                                if (rulePortalId == portalId || rulePortalId == -1 || portalSpecific)
                                {
                                    // if portal specific, all rules are assumed to belong to the portal
                                    string tabIdRaw = rdr.GetAttribute("tabIds");
                                    string tabNames = rdr.GetAttribute("tabNames");
                                    string name = rdr.GetAttribute("name");
                                    string fromDefaultRaw = rdr.GetAttribute("fromDefault");
                                    if (!bool.TryParse(fromDefaultRaw, out var fromDefault))
                                    {
                                        fromDefault = false;
										Logger.LogDebug("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): fromDefault → {FromDefault}", actions, fileName, portalId, portalSpecific, messages, fromDefault);
                                    }

                                    string fromSiteRootRaw = rdr.GetAttribute("fromSiteRoot");
                                    if (!bool.TryParse(fromSiteRootRaw, out var fromSiteRoot))
                                    {
                                        fromSiteRoot = false;
										Logger.LogDebug("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): fromSiteRoot → {FromSiteRoot}", actions, fileName, portalId, portalSpecific, messages, fromSiteRoot);
                                    }

                                    string changeToSiteRootRaw = rdr.GetAttribute("changeToSiteRoot");
                                    if (!bool.TryParse(changeToSiteRootRaw, out var changeToSiteRoot))
                                    {
                                        changeToSiteRoot = false;
										Logger.LogDebug("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): changeToSiteRoot → {ChangeToSiteRoot}", actions, fileName, portalId, portalSpecific, messages, changeToSiteRoot);
                                    }

                                    List<int> tabIds = XmlHelpers.TabIdsFromAttributes(tabIdRaw, tabNames, portalId, ref tabMessages);
									Logger.LogError("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): tabIds → {TabIds}", actions, fileName, portalId, portalSpecific, messages, tabIds);
                                    foreach (int tabId in tabIds)
                                    {
                                        var action = new ParameterRedirectAction
                                        {
                                            PortalId = portalId,
                                            LookFor = rdr.GetAttribute("lookFor"),
                                            RedirectTo = rdr.GetAttribute("redirectTo"),
                                            Name = name,
                                            Action = rdr.GetAttribute("action"),
                                            ChangeToSiteRoot = changeToSiteRoot,
                                            TabId = tabId,
                                        };
                                        if (fromDefault)
                                        {
                                            // check for 'fromDefault' attribute
                                            action.ForDefaultPage = true;
											Logger.LogDebug("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): action.ForDefaultPage → {Arg5}", actions, fileName, portalId, portalSpecific, messages, action.ForDefaultPage);
                                            action.TabId = -2;
											Logger.LogDebug("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): action.TabId → {Arg5}", actions, fileName, portalId, portalSpecific, messages, action.TabId);
                                        }
                                        else
                                        {
                                            // or support the older convention, which was to include a tabid of -2
                                            action.ForDefaultPage = tabId == -2;
                                        }

                                        if (fromSiteRoot)
                                        {
                                            action.TabId = -3; // site root marker
                                        }

                                        List<ParameterRedirectAction> tabActionCol;
                                        if (actions.TryGetValue(action.TabId, out var parameterRedirectActions))
                                        {
                                            tabActionCol = parameterRedirectActions;
											Logger.LogError("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): tabActionCol → {TabActionCol}", actions, fileName, portalId, portalSpecific, messages, tabActionCol);
                                        }
                                        else
                                        {
                                            tabActionCol = [];
											Logger.LogDebug("LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages}): tabActionCol → {TabActionCol}", actions, fileName, portalId, portalSpecific, messages, tabActionCol);
                                            actions.Add(action.TabId, tabActionCol);
                                        }

                                        tabActionCol.Add(action);
                                        actionCount++;
                                    }

                                    messages.Add(name + " redirect actions added:" + actionCount.ToString(CultureInfo.InvariantCulture));
                                }

                                if (tabMessages.Count > 0)
                                {
                                    messages.AddRange(tabMessages);
                                }
                            }

                            break;

                        case XmlNodeType.EndElement:
                            break;
                    }
                }

                rdr.Close();
			Logger.LogWarning("<<< Exiting LoadFromXmlFile(actions={Actions},fileName={FileName},portalId={PortalId},portalSpecific={PortalSpecific},messages={Messages})", actions, fileName, portalId, portalSpecific, messages);
            }
        }

        public static void LoadFromXmlFile(this Dictionary<int, SharedList<ParameterRewriteAction>> actions, string fileName, int portalId, bool portalSpecific, ref List<string> messages)
        {
            if (messages == null)
            {
                messages = [];
            }

            if (File.Exists(fileName))
            {
                var rdr = new XmlTextReader(fileName)
                {
                    XmlResolver = null,
                    DtdProcessing = DtdProcessing.Prohibit,
                };
                while (rdr.Read())
                {
                    switch (rdr.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (rdr.Name == "parameterRewrite")
                            {
                                string portalIdRaw = rdr.GetAttribute("portalId");
                                int rulePortalId = -1;
                                int actionCount = 0;
                                if (portalIdRaw != null)
                                {
                                    if (!int.TryParse(portalIdRaw, out rulePortalId))
                                    {
                                        rulePortalId = -1;
                                    }
                                }

                                if (rulePortalId == portalId || rulePortalId == -1 || portalId == -1 || portalSpecific)
                                {
                                    // now set up the action
                                    string tabIdRaw = rdr.GetAttribute("tabIds");
                                    string tabNames = rdr.GetAttribute("tabNames");
                                    string name = rdr.GetAttribute("name");
                                    string fromSiteRootRaw = rdr.GetAttribute("fromSiteRoot");
                                    if (!bool.TryParse(fromSiteRootRaw, out var fromSiteRoot))
                                    {
                                        fromSiteRoot = false;
                                    }

                                    List<int> tabIds = XmlHelpers.TabIdsFromAttributes(tabIdRaw, tabNames, portalId, ref messages);
                                    foreach (int tabId in tabIds)
                                    {
                                        var action = new ParameterRewriteAction
                                        {
                                            LookFor = rdr.GetAttribute("lookFor"),
                                            RewriteTo = rdr.GetAttribute("rewriteTo"),
                                            Name = name,
                                            TabId = tabId,
                                        };
                                        if (fromSiteRoot)
                                        {
                                            action.ForSiteRoot = true;
                                            action.TabId = -3;
                                        }
                                        else
                                        {
                                            // older rule specified tabid -3 meant site root
                                            action.ForSiteRoot = tabId == -3;
                                        }

                                        action.PortalId = portalId;
                                        SharedList<ParameterRewriteAction> tabActionCol;
                                        if (actions.TryGetValue(action.TabId, out var parameterRewriteActions))
                                        {
                                            tabActionCol = parameterRewriteActions;
                                        }
                                        else
                                        {
                                            tabActionCol = new SharedList<ParameterRewriteAction>();
                                            actions.Add(action.TabId, tabActionCol);
                                        }

                                        tabActionCol.Add(action);
                                        actionCount++;
                                    }

                                    messages.Add(name + " rewrite actions added:" + actionCount.ToString(CultureInfo.InvariantCulture));
                                }
                            }

                            break;

                        case XmlNodeType.EndElement:
                            break;
                    }
                }

                rdr.Close();
            }
            else
            {
                messages.Add("Filename does not exist:" + fileName);
            }
        }

        public static Dictionary<string, string> CreateDictionaryFromString(string delimitedString, char pairSeparator, char separator)
        {
            var dictionary = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(delimitedString))
            {
                var pairs = delimitedString.Split(pairSeparator);
                foreach (string pair in pairs)
                {
                    if (!string.IsNullOrEmpty(pair))
                    {
                        var chars = pair.Split(separator);
                        dictionary[chars[0]] = chars[1];
                    }
                }
            }

            return dictionary;
        }
    }
}
