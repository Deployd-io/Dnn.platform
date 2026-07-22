// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.CoreMessaging.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.CoreMessaging.ViewModels;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Subscriptions;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides a web service to manage subscriptions.</summary>
    /// <param name="hostSettings">The host settings.</param>
    [DnnAuthorize]
    public class SubscriptionsController(IHostSettings hostSettings)
        : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string SharedResources = "~/DesktopModules/CoreMessaging/App_LocalResources/SharedResources.resx";
		Logger.LogDebug(">>> Entering SubscriptionsController(hostSettings={HostSettings})", hostSettings);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        private const string ViewControlResources = "~/DesktopModules/CoreMessaging/App_LocalResources/View.ascx.resx";
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("SubscriptionsController(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="SubscriptionsController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
		Logger.LogDebug("<<< Exiting SubscriptionsController(hostSettings={HostSettings})", hostSettings);
        public SubscriptionsController()
            : this(null)
        {
        }

        private string LocalizationFolder
        {
            get
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this.hostSettings, "DotNetNuke.Modules.CoreMessaging", this.PortalSettings.PortalId);
                return $"~/DesktopModules/{desktopModule.FolderName}/App_LocalResources/";
            }
        }

        /// <summary>Perform a search on Scoring Activities registered in the system.</summary>
        /// <param name="pageIndex">Page index to begin from (0, 1, 2).</param>
        /// <param name="pageSize">Number of records to return per page.</param>
        /// <param name="sortExpression">The sort expression in the form [Description|SubscriptionType] [Asc|Desc].</param>
        /// <returns>The sorted and paged list of subscriptions.</returns>
        [HttpGet]
        public HttpResponseMessage GetSubscriptions(int pageIndex, int pageSize, string sortExpression)
        {
			Logger.LogDebug(">>> Entering GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression})", pageIndex, pageSize, sortExpression);
            try
            {
                var subscriptions = from s in SubscriptionController.Instance.GetUserSubscriptions(this.UserInfo, this.PortalSettings.PortalId)
                                    select GetSubscriptionViewModel(s);

                List<SubscriptionViewModel> sortedList;
                if (string.IsNullOrEmpty(sortExpression))
                {
                    sortedList = subscriptions.ToList();
					Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): sortedList → {SortedList}", pageIndex, pageSize, sortExpression, sortedList);
                }
                else
                {
                    var sort = sortExpression.Split(' ');
                    var desc = sort.Length == 2 && sort[1] == "desc";
					Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): sort → {Sort}", pageIndex, pageSize, sortExpression, sort);
					Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): desc → {Desc}", pageIndex, pageSize, sortExpression, desc);
                    switch (sort[0])
                    {
                        case "Description":
                            sortedList = desc
                                ? subscriptions.OrderByDescending(s => s.Description).ToList()
                                : subscriptions.OrderBy(s => s.Description).ToList();
                            break;

                        case "SubscriptionType":
                            sortedList = desc
                                ? subscriptions.OrderByDescending(s => s.SubscriptionType).ToList()
                                : subscriptions.OrderBy(s => s.SubscriptionType).ToList();
                            break;

                        default:
                            sortedList = subscriptions.ToList();
                            break;
                    }
                }

                var response = new
                {
                    Success = true,
                    Results = sortedList.Skip(pageIndex * pageSize).Take(pageSize).ToList(),
                    TotalResults = sortedList.Count(),
                };
					Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): Success → {Success}", pageIndex, pageSize, sortExpression, Success);
					Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): Results → {Results}", pageIndex, pageSize, sortExpression, Results);
					Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): TotalResults → {TotalResults}", pageIndex, pageSize, sortExpression, TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SubscriptionsController(hostSettings={HostSettings}): {Arg1}", hostSettings, ex.Message);
                var message = "An unexpected error occurred while attempting to get the subscriptions, consult the server logs for more information.";
				Logger.LogError("Exception in GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): {Arg3}", pageIndex, pageSize, sortExpression, ex.Message);
				Logger.LogDebug("GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression}): message → {Message}", pageIndex, pageSize, sortExpression, message);
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting GetSubscriptions(pageIndex={PageIndex},pageSize={PageSize},sortExpression={SortExpression})", pageIndex, pageSize, sortExpression);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Updates system sybscriptions.</summary>
        /// <param name="post"><see cref="InboxSubscriptionViewModel"/>.</param>
        /// <returns><see cref="UserPreference"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSystemSubscription(InboxSubscriptionViewModel post)
        {
			Logger.LogDebug(">>> Entering UpdateSystemSubscription(post={Post})", post);
            try
            {
                var userPreferencesController = UserPreferencesController.Instance;
                var userPreference = new UserPreference
                {
                    PortalId = this.UserInfo.PortalID,
                    UserId = this.UserInfo.UserID,
                    MessagesEmailFrequency = (Frequency)post.MsgFreq,
                    NotificationsEmailFrequency = (Frequency)post.NotifyFreq,
                };
                userPreferencesController.SetUserPreference(userPreference);

                return this.Request.CreateResponse(HttpStatusCode.OK, userPreferencesController.GetUserPreference(this.UserInfo));
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to update the system subscriptions, consult the server logs for more information.";
				Logger.LogError("Exception in UpdateSystemSubscription(post={Post}): {Arg1}", post, ex.Message);
				Logger.LogDebug("UpdateSystemSubscription(post={Post}): message → {Message}", post, message);
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting UpdateSystemSubscription(post={Post})", post);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Deletes a content subscriptions.</summary>
        /// <param name="subscription"><see cref="Subscription"/>.</param>
        /// <returns>"unsubscribed" or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentSubscription(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering DeleteContentSubscription(subscription={Subscription})", subscription);
            try
            {
                var sub = SubscriptionController.Instance.GetUserSubscriptions(this.UserInfo, this.PortalSettings.PortalId)
                                          .SingleOrDefault(s => s.SubscriptionId == subscription.SubscriptionId);
                if (sub != null)
                {
                    SubscriptionController.Instance.DeleteSubscription(sub);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, "unsubscribed");
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to delete a content subscription, consult the server logs for more information.";
				Logger.LogError("Exception in DeleteContentSubscription(subscription={Subscription}): {Arg1}", subscription, ex.Message);
				Logger.LogDebug("DeleteContentSubscription(subscription={Subscription}): message → {Message}", subscription, message);
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting DeleteContentSubscription(subscription={Subscription})", subscription);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets a localized table for a given culture.</summary>
        /// <param name="culture">The culture for which to get the localization.</param>
        /// <returns>A dictionnary of localization keys and their localized values.</returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLocalizationTable(string culture)
        {
			Logger.LogDebug(">>> Entering GetLocalizationTable(culture={Culture})", culture);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (!string.IsNullOrEmpty(culture))
                {
                    Localization.SetThreadCultures(new CultureInfo(culture), (IPortalSettings)this.PortalSettings);
                }

                var dictionary = new Dictionary<string, string>();

				Logger.LogDebug("GetLocalizationTable(culture={Culture}): dictionary → {Dictionary}", culture, dictionary);
                var resourcesPath = this.LocalizationFolder;
                var files =
                    Directory.GetFiles(System.Web.HttpContext.Current.Server.MapPath(resourcesPath)).Select(x => new FileInfo(x).Name).Where(f => !IsLanguageSpecific(f)).ToList();
					Logger.LogInformation("SubscriptionsController(hostSettings)={HostSettings}: find query executed in {Arg1} ms", hostSettings, __logStart.ElapsedMilliseconds);

				Logger.LogInformation("GetLocalizationTable(culture)={Culture}: find query executed in {Arg1} ms", culture, __logStart.ElapsedMilliseconds);
                foreach (var kvp in files.SelectMany(f => GetLocalizationValues(resourcesPath, f, culture)).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary.Add(kvp.Key, kvp.Value);
                }

                foreach (var kvp in GetLocalizationValues(SharedResources, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary.Add(kvp.Key, kvp.Value);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Table = dictionary });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to get the localization table, consult the server logs for more information.";
				Logger.LogError("Exception in GetLocalizationTable(culture={Culture}): {Arg1}", culture, ex.Message);
				Logger.LogDebug("GetLocalizationTable(culture={Culture}): message → {Message}", culture, message);
                Exceptions.LogException(ex);
				Logger.LogDebug("<<< Exiting GetLocalizationTable(culture={Culture})", culture);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        private static SubscriptionViewModel GetSubscriptionViewModel(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering GetSubscriptionViewModel(subscription={Subscription})", subscription);
            var model = new SubscriptionViewModel
            {
                SubscriptionId = subscription.SubscriptionId,
                Description = subscription.Description,
            };

            // localize the type name
            var subscriptionType = SubscriptionTypeController.Instance.GetSubscriptionType(
                t => t.SubscriptionTypeId == subscription.SubscriptionTypeId);

            if (subscriptionType != null)
            {
                var localizedName = Localization.GetString(subscriptionType.SubscriptionName, ViewControlResources);
                model.SubscriptionType = localizedName ?? subscriptionType.FriendlyName;
				Logger.LogDebug("GetSubscriptionViewModel(subscription={Subscription}): localizedName → {LocalizedName}", subscription, localizedName);
				Logger.LogDebug("GetSubscriptionViewModel(subscription={Subscription}): model.SubscriptionType → {Arg1}", subscription, model.SubscriptionType);
            }

			Logger.LogDebug("<<< Exiting GetSubscriptionViewModel(subscription={Subscription})", subscription);
            return model;
        }

        private static bool IsLanguageSpecific(string fileName)
        {
			Logger.LogDebug(">>> Entering IsLanguageSpecific(fileName={FileName})", fileName);
            var components = fileName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (components.Length > 1)
			Logger.LogDebug("IsLanguageSpecific(fileName={FileName}): components → {Components}", fileName, components);
            {
                var language = components[components.Length - 2];

                if (!string.IsNullOrEmpty(language))
                {
                    try
                    {
                        CultureInfo.GetCultureInfo(language);

                        return true;
                    }
                    catch (CultureNotFoundException)
                    {
						Logger.LogError("Exception in SubscriptionsController(hostSettings={HostSettings}): {Arg1}", hostSettings, ex.Message);
                        return false;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting IsLanguageSpecific(fileName={FileName})", fileName);
            return false;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string path, string file, string culture)
        {
            return GetLocalizationValues(string.Format("{0}/{1}", path, file), culture);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
        {
            using var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(fullPath), FileMode.Open, FileAccess.Read);
            var document = new XmlDocument { XmlResolver = null };
            using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { XmlResolver = null, }))
            {
                document.Load(xmlReader);
            }

            var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();

            AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

            foreach (var xmlNode in document.SelectNodes("/root/data").Cast<XmlNode>())
            {
                var name = GetNameAttribute(xmlNode).Replace(".Text", string.Empty);

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var value = Localization.GetString(string.Format("{0}.Text", name), fullPath, culture);

                yield return new KeyValuePair<string, string>(name, value);
            }
        }

        private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
        {
            var header = headers.FirstOrDefault(x => GetNameAttribute(x).Equals(key, StringComparison.InvariantCultureIgnoreCase));
            if (header != null)
            {
                if (!header.InnerText.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ApplicationException(string.Format("Resource header '{0}' != '{1}'", key, value));
                }
				Logger.LogDebug(">>> Entering GetNameAttribute(node={Node})", node);
				Logger.LogDebug("<<< Exiting GetNameAttribute(node={Node})", node);
            }
            else
            {
                throw new ApplicationException(string.Format("Resource header '{0}' is missing", key));
            }
        }

        private static string GetNameAttribute(XmlNode node)
        {
			Logger.LogDebug(">>> Entering GetNameAttribute(node={Node})", node);
            if (node.Attributes != null)
            {
                var attribute = node.Attributes.GetNamedItem("name");
                if (attribute != null)
				Logger.LogDebug("GetNameAttribute(node={Node}): attribute → {Attribute}", node, attribute);
                {
                    return attribute.Value;
                }
            }

			Logger.LogDebug("<<< Exiting GetNameAttribute(node={Node})", node);
            return null;
        }
    }
}
