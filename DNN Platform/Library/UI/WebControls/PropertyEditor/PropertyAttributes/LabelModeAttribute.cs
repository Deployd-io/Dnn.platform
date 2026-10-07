// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LabelModeAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LabelModeAttribute));
        private readonly LabelMode mode;

        /// <summary>Initializes a new instance of the <see cref="LabelModeAttribute"/> class.</summary>
        /// <param name="mode">The label mode to apply to the associated property.</param>
        public LabelModeAttribute(LabelMode mode)
        {
			Logger.LogError(">>> Entering LabelModeAttribute(mode={Mode})", mode);
            this.mode = mode;
			Logger.LogDebug("LabelModeAttribute(mode={Mode}): mode → {Mode1}", mode, mode);
        Logger.LogDebug("LabelModeAttribute(mode={Mode}): this.mode → {Arg1}", mode, this.mode);
        Logger.LogWarning("<<< Exiting LabelModeAttribute(mode={Mode})", mode);
        }

        public LabelMode Mode
        {
            get
            {
                return this.mode;
            }
        }
    }
}
