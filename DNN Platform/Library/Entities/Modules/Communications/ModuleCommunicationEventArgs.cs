// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Communications
{
    using System;

    public class ModuleCommunicationEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleCommunicationEventArgs));
        /// <summary>Initializes a new instance of the <see cref="ModuleCommunicationEventArgs"/> class.</summary>
        public ModuleCommunicationEventArgs()
        {
		Logger.LogWarning("<<< Exiting ModuleCommunicationEventArgs(type={Type},value={Value},sender={Sender},target={Target})", type, value, sender, target);
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleCommunicationEventArgs"/> class.</summary>
        /// <param name="text">The text.</param>
        public ModuleCommunicationEventArgs(string text)
        {
			Logger.LogError(">>> Entering ModuleCommunicationEventArgs(text={Text})", text);
            this.Text = text;
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleCommunicationEventArgs"/> class.</summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="target">The target.</param>
        public ModuleCommunicationEventArgs(string type, object value, string sender, string target)
        {
			Logger.LogError(">>> Entering ModuleCommunicationEventArgs(type={Type},value={Value},sender={Sender},target={Target})", type, value, sender, target);
            this.Type = type;
			Logger.LogWarning("<<< Exiting ModuleCommunicationEventArgs(type={Type},value={Value},sender={Sender},target={Target})", type, value, sender, target);
			Logger.LogDebug("ModuleCommunicationEventArgs(type={Type},value={Value},sender={Sender},target={Target}): this.Type → {Arg4}", type, value, sender, target, this.Type);
            this.Value = value;
            this.Sender = sender;
			Logger.LogDebug("ModuleCommunicationEventArgs(type={Type},value={Value},sender={Sender},target={Target}): this.Value → {Arg4}", type, value, sender, target, this.Value);
            this.Target = target;
        }

        public string Sender { get; set; }

        public string Target { get; set; }

        public string Text { get; set; }

        public string Type { get; set; }

        public object Value { get; set; }
    }
}
