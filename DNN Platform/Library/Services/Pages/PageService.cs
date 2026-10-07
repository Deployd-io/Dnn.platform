// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Pages;

    /// <summary>
    /// Provides services for managing page-level elements including meta tags, head tags, messages, and SEO properties.
    /// This service supports priority-based management where higher priority values take precedence.
    /// </summary>
    /// <remarks>
    /// The PageService manages various page-level elements:
    /// - Page metadata (title, description, keywords) with priority-based overrides
    /// - Custom head tags for additional HTML head content
    /// - Meta tags for SEO and page information
    /// - Page messages for user notifications
    /// Priority system: Higher priority values (e.g., 200) will override lower priority values (e.g., 100).
    /// Default priority is 100 for most operations.
    /// </remarks>
    public class PageService : IPageService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PageService));
        /// <summary>
        /// Collection of head tags to be included in the HTML head section, ordered by priority.
        /// </summary>
        private readonly List<PageTag> headTags = new List<PageTag>();

        /// <summary>
        /// Collection of meta tags for SEO and page information, ordered by priority.
        /// </summary>
        private readonly List<PageMeta> metaTags = new List<PageMeta>();

        /// <summary>
        /// Collection of page messages for user notifications, ordered by priority.
        /// </summary>
        private readonly List<PageMessage> messages = new List<PageMessage>();

        /// <summary>
        /// The current page title with the highest priority.
        /// </summary>
        private PageTag title;

        /// <summary>
        /// The current page description with the highest priority.
        /// </summary>
        private PageTag description;

        /// <summary>
        /// The current page keywords with the highest priority.
        /// </summary>
        private PageTag keywords;

        /// <summary>
        /// The current page canonical link URL with the highest priority.
        /// </summary>
        private PageTag canonicalLinkUrl;

        /// <summary>
        /// Adds a message to the page's message collection.
        /// </summary>
        /// <param name="messageItem">The message item to add to the page. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageItem"/> is null.</exception>
        /// <remarks>
        /// Messages are used to display notifications, alerts, or informational content to users.
        /// They are ordered by priority when retrieved, with lower priority values appearing first.
        /// </remarks>
        public void AddMessage(PageMessage messageItem)
        {
			Logger.LogError(">>> Entering AddMessage(messageItem={MessageItem})", messageItem);
            if (messageItem == null)
            {
				Logger.LogError("Throwing ArgumentNullException from AddMessage(messageItem={MessageItem})", messageItem);
                throw new ArgumentNullException(nameof(messageItem));
            }

            this.messages.Add(messageItem);
			Logger.LogWarning("<<< Exiting AddMessage(messageItem={MessageItem})", messageItem);
        }

        /// <summary>
        /// Adds a meta tag to the page's meta tag collection.
        /// </summary>
        /// <param name="metaItem">The meta tag item to add to the page. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="metaItem"/> is null.</exception>
        /// <remarks>
        /// Meta tags provide metadata about the HTML document and are typically used for SEO,
        /// social media sharing, and browser behavior. They are rendered in the HTML head section
        /// and ordered by priority when retrieved.
        /// </remarks>
        public void AddMeta(PageMeta metaItem)
        {
			Logger.LogError(">>> Entering AddMeta(metaItem={MetaItem})", metaItem);
            if (metaItem == null)
            {
				Logger.LogError("Throwing ArgumentNullException from AddMeta(metaItem={MetaItem})", metaItem);
                throw new ArgumentNullException(nameof(metaItem));
            }

            this.metaTags.Add(metaItem);
			Logger.LogWarning("<<< Exiting AddMeta(metaItem={MetaItem})", metaItem);
        }

        /// <summary>
        /// Adds a custom tag to the page's HTML head section.
        /// </summary>
        /// <param name="tagItem">The tag item to add to the head section. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="tagItem"/> is null.</exception>
        /// <remarks>
        /// Head tags allow you to include custom HTML elements in the page's head section,
        /// such as custom CSS links, JavaScript files, or other HTML elements.
        /// Tags are ordered by priority when retrieved, with lower priority values appearing first.
        /// </remarks>
        public void AddToHead(PageTag tagItem)
        {
			Logger.LogError(">>> Entering AddToHead(tagItem={TagItem})", tagItem);
            if (tagItem == null)
            {
				Logger.LogError("Throwing ArgumentNullException from AddToHead(tagItem={TagItem})", tagItem);
                throw new ArgumentNullException(nameof(tagItem));
            }

            this.headTags.Add(tagItem);
			Logger.LogWarning("<<< Exiting AddToHead(tagItem={TagItem})", tagItem);
        }

        /// <summary>
        /// Sets the page description with the specified priority.
        /// </summary>
        /// <param name="value">The description text for the page.</param>
        /// <param name="priority">The priority level for this description. Higher values take precedence. Default is 100.</param>
        /// <remarks>
        /// The page description is typically used in meta tags for SEO purposes and may appear
        /// in search engine results. Only the description with the highest priority will be used.
        /// If multiple descriptions have the same priority, the last one set will be used.
        /// </remarks>
        public void SetDescription(string value, int priority = 100)
        {
			Logger.LogError(">>> Entering SetDescription(value={Value},priority={Priority})", value, priority);
            SetHighestPriorityValue(ref this.description, value, priority);
			Logger.LogWarning("<<< Exiting SetDescription(value={Value},priority={Priority})", value, priority);
        }

        /// <summary>
        /// Sets the page keywords with the specified priority.
        /// </summary>
        /// <param name="value">The keywords for the page, typically comma-separated.</param>
        /// <param name="priority">The priority level for these keywords. Higher values take precedence. Default is 100.</param>
        /// <remarks>
        /// Page keywords are used for SEO purposes and help categorize the page content.
        /// Only the keywords with the highest priority will be used.
        /// If multiple keyword sets have the same priority, the last one set will be used.
        /// </remarks>
        public void SetKeyWords(string value, int priority = 100)
        {
			Logger.LogError(">>> Entering SetKeyWords(value={Value},priority={Priority})", value, priority);
            SetHighestPriorityValue(ref this.keywords, value, priority);
			Logger.LogWarning("<<< Exiting SetKeyWords(value={Value},priority={Priority})", value, priority);
        }

        /// <summary>
        /// Sets the page title with the specified priority.
        /// </summary>
        /// <param name="value">The title text for the page.</param>
        /// <param name="priority">The priority level for this title. Higher values take precedence. Default is 100.</param>
        /// <remarks>
        /// The page title appears in the browser tab, search engine results, and when sharing on social media.
        /// Only the title with the highest priority will be used.
        /// If multiple titles have the same priority, the last one set will be used.
        /// </remarks>
        public void SetTitle(string value, int priority = 100)
        {
			Logger.LogError(">>> Entering SetTitle(value={Value},priority={Priority})", value, priority);
            SetHighestPriorityValue(ref this.title, value, priority);
			Logger.LogWarning("<<< Exiting SetTitle(value={Value},priority={Priority})", value, priority);
        }

        /// <summary>
        /// Sets the page canonical link URL with the specified priority.
        /// </summary>
        /// <param name="value">The canonical link URL for the page.</param>
        /// <param name="priority">The priority level for this canonical link URL. Higher values take precedence. Default is 100.</param>
        public void SetCanonicalLinkUrl(string value, int priority = 100)
        {
			Logger.LogError(">>> Entering SetCanonicalLinkUrl(value={Value},priority={Priority})", value, priority);
            SetHighestPriorityValue(ref this.canonicalLinkUrl, value, priority);
			Logger.LogWarning("<<< Exiting SetCanonicalLinkUrl(value={Value},priority={Priority})", value, priority);
        }

        /// <summary>
        /// Gets the current canonical link URL value with highest priority.
        /// </summary>
        /// <returns>The canonical link URL value or null if not set.</returns>
        public string GetCanonicalLinkUrl()
        {
			Logger.LogError(">>> Entering GetCanonicalLinkUrl()");
            Logger.LogWarning("<<< Exiting GetCanonicalLinkUrl()");
            return this.canonicalLinkUrl?.Value;
        }

        /// <summary>
        /// Gets the current title value with highest priority.
        /// </summary>
        /// <returns>The title value or null if not set.</returns>
        public string GetTitle()
        {
			Logger.LogError(">>> Entering GetTitle()");
            Logger.LogWarning("<<< Exiting GetTitle()");
            return this.title?.Value;
        }

        /// <summary>
        /// Gets the current description value with highest priority.
        /// </summary>
        /// <returns>The description value or null if not set.</returns>
        public string GetDescription()
        {
			Logger.LogError(">>> Entering GetDescription()");
            Logger.LogWarning("<<< Exiting GetDescription()");
            return this.description?.Value;
        }

        /// <summary>
        /// Gets the current keywords value with highest priority.
        /// </summary>
        /// <returns>The keywords value or null if not set.</returns>
        public string GetKeyWords()
        {
			Logger.LogError(">>> Entering GetKeyWords()");
            Logger.LogWarning("<<< Exiting GetKeyWords()");
            return this.keywords?.Value;
        }

        /// <summary>
        /// Gets all head tags ordered by priority (lowest priority first).
        /// </summary>
        /// <returns>List of head tags ordered by priority.</returns>
        public IEnumerable<PageTag> GetHeadTags()
        {
			Logger.LogError(">>> Entering GetHeadTags()");
			Logger.LogWarning("<<< Exiting GetHeadTags()");
            return this.headTags.OrderBy(x => x.Priority).ToList();
        }

        /// <summary>
        /// Gets all meta tags ordered by priority (lowest priority first).
        /// </summary>
        /// <returns>List of meta tags ordered by priority.</returns>
        public IEnumerable<PageMeta> GetMetaTags()
        {
			Logger.LogError(">>> Entering GetMetaTags()");
			Logger.LogWarning("<<< Exiting GetMetaTags()");
            return this.metaTags.OrderBy(x => x.Priority).ToList();
        }

        /// <summary>
        /// Gets all messages ordered by priority (lowest priority first).
        /// </summary>
        /// <returns>List of messages ordered by priority.</returns>
        public IEnumerable<PageMessage> GetMessages()
        {
			Logger.LogError(">>> Entering GetMessages()");
			Logger.LogWarning("<<< Exiting GetMessages()");
            return this.messages.OrderBy(x => x.Priority).ToList();
        }

        /// <summary>
        /// Clears all stored page data. Useful for testing or resetting state.
        /// </summary>
        public void Clear()
        {
			Logger.LogError(">>> Entering Clear()");
            this.title = null;
			Logger.LogDebug("Clear(): this.title → {Arg0}", this.title);
			Logger.LogDebug("Clear(): title → {Title}", title);
        Logger.LogWarning("<<< Exiting Clear()");
            this.description = null;
            this.keywords = null;
			Logger.LogDebug("Clear(): description → {Description}", description);
			Logger.LogDebug("Clear(): this.description → {Arg0}", this.description);
			Logger.LogDebug("Clear(): keywords → {Keywords}", keywords);
            this.headTags.Clear();
            this.metaTags.Clear();
            this.messages.Clear();
        }

        /// <summary>
        /// Sets the highest priority value for a PageTag field.
        /// </summary>
        /// <param name="currentItem">Reference to the current PageTag field to potentially update.</param>
        /// <param name="value">The new value to set.</param>
        /// <param name="priority">The priority of the new value.</param>
        /// <remarks>
        /// This method implements the priority-based override system. It only updates the current item
        /// if the new priority is higher than the existing priority, or if no item is currently set.
        /// This ensures that higher priority values always take precedence.
        /// </remarks>
        private static void SetHighestPriorityValue(ref PageTag currentItem, string value, int priority)
        {
			Logger.LogError(">>> Entering SetHighestPriorityValue(currentItem={CurrentItem},value={Value},priority={Priority})", currentItem, value, priority);
            if (currentItem == null || priority > currentItem.Priority)
            {
                currentItem = new PageTag(value, priority);
				Logger.LogError("SetHighestPriorityValue(currentItem={CurrentItem},value={Value},priority={Priority}): currentItem → {CurrentItem1}", currentItem, value, priority, currentItem);
            }
			Logger.LogWarning("<<< Exiting SetHighestPriorityValue(currentItem={CurrentItem},value={Value},priority={Priority})", currentItem, value, priority);
        }
    }
}
