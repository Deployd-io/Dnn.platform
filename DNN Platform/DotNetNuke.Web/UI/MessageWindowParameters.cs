// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI
{
    using System.Globalization;
    using System.Web.UI.WebControls;

    /// <summary>Information about a message displayed in a window.</summary>
    public class MessageWindowParameters
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string message = string.Empty;
        private string title = string.Empty;
        private Unit windowHeight = Unit.Pixel(175);
        private Unit windowWidth = Unit.Pixel(350);

        /// <summary>Initializes a new instance of the <see cref="MessageWindowParameters"/> class.</summary>
        /// <param name="message">The message.</param>
        public MessageWindowParameters(string message)
        {
			Logger.LogDebug(">>> Entering MessageWindowParameters(message={Message})", message);
            this.message = message;
			Logger.LogDebug("<<< Exiting MessageWindowParameters(message={Message})", message);
			Logger.LogDebug("MessageWindowParameters(message={Message}): this.message → {Arg1}", message, this.message);
        }

        /// <summary>Initializes a new instance of the <see cref="MessageWindowParameters"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        public MessageWindowParameters(string message, string title)
        {
			Logger.LogDebug(">>> Entering MessageWindowParameters(message={Message},title={Title})", message, title);
            this.message = message;
            this.title = title;
			Logger.LogDebug("<<< Exiting MessageWindowParameters(message={Message},title={Title})", message, title);
			Logger.LogDebug("MessageWindowParameters(message={Message},title={Title}): this.message → {Arg2}", message, title, this.message);
			Logger.LogDebug("MessageWindowParameters(message={Message},title={Title}): this.title → {Arg2}", message, title, this.title);
        }

        /// <summary>Initializes a new instance of the <see cref="MessageWindowParameters"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <param name="windowWidth">The window width (in a format that can be parsed by <see cref="Unit.Parse(string)" />).</param>
        /// <param name="windowHeight">The window height (in a format that can be parsed by <see cref="Unit.Parse(string)" />).</param>
        public MessageWindowParameters(string message, string title, string windowWidth, string windowHeight)
        {
			Logger.LogDebug(">>> Entering MessageWindowParameters(message={Message},title={Title},windowWidth={WindowWidth},windowHeight={WindowHeight})", message, title, windowWidth, windowHeight);
            this.message = message;
            this.title = title;
			Logger.LogDebug("MessageWindowParameters(message={Message},title={Title},windowWidth={WindowWidth},windowHeight={WindowHeight}): this.message → {Arg4}", message, title, windowWidth, windowHeight, this.message);
			Logger.LogDebug("MessageWindowParameters(message={Message},title={Title},windowWidth={WindowWidth},windowHeight={WindowHeight}): this.title → {Arg4}", message, title, windowWidth, windowHeight, this.title);
			Logger.LogDebug("MessageWindowParameters(message={Message},title={Title},windowWidth={WindowWidth},windowHeight={WindowHeight}): windowHeight → {WindowHeight1}", message, title, windowWidth, windowHeight, windowHeight);
            this.windowWidth = Unit.Parse(windowWidth, CultureInfo.InvariantCulture);
            this.windowHeight = Unit.Parse(windowHeight, CultureInfo.InvariantCulture);
			Logger.LogDebug("<<< Exiting MessageWindowParameters(message={Message},title={Title},windowWidth={WindowWidth},windowHeight={WindowHeight})", message, title, windowWidth, windowHeight);
			Logger.LogDebug("MessageWindowParameters(message={Message},title={Title},windowWidth={WindowWidth},windowHeight={WindowHeight}): windowWidth → {WindowWidth1}", message, title, windowWidth, windowHeight, windowWidth);
        }

        /// <summary>Gets or sets the message text.</summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                // todo: javascript encode for onclick events
                this.message = value;
                this.message = this.message.Replace("'", "\\'");
                this.message = this.message.Replace("\"", "\\\"");
            }
        }

        /// <summary>Gets or sets the message title.</summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                // todo: javascript encode for onclick events
                this.title = value;
                this.title = this.title.Replace("'", "\\'");
                this.title = this.title.Replace("\"", "\\\"");
            }
        }

        /// <summary>Gets or sets the width of the window in which to display the message.</summary>
        public Unit WindowWidth
        {
            get
            {
                return this.windowWidth;
            }

            set
            {
                this.windowWidth = value;
            }
        }

        /// <summary>Gets or sets the height of the window in which to display the message.</summary>
        public Unit WindowHeight
        {
            get
            {
                return this.windowHeight;
            }

            set
            {
                this.windowHeight = value;
            }
        }
    }
}
