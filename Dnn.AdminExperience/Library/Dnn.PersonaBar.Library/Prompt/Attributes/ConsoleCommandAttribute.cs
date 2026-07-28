// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Attributes
{
    using System;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>An attribute to mark Prompt console commands.</summary>
    [DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project")]
    [AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
    public partial class ConsoleCommandAttribute : Attribute
#pragma warning restore CS3015 // Type has no accessible constructors which use only CLS-compliant types
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ConsoleCommandAttribute"/> class.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="category">The command category.</param>
        /// <param name="description">The command description.</param>
        public ConsoleCommandAttribute(string name, string category, string description)
        {
			Logger.LogDebug(">>> Entering ConsoleCommandAttribute(name={Name},category={Category},description={Description})", name, category, description);
            this.Name = name;
            this.Category = category;
			Logger.LogDebug("ConsoleCommandAttribute(name={Name},category={Category},description={Description}): this.Name → {Arg3}", name, category, description, this.Name);
			Logger.LogDebug("ConsoleCommandAttribute(name={Name},category={Category},description={Description}): this.Category → {Arg3}", name, category, description, this.Category);
            this.Description = description;
			Logger.LogDebug("<<< Exiting ConsoleCommandAttribute(name={Name},category={Category},description={Description})", name, category, description);
			Logger.LogDebug("ConsoleCommandAttribute(name={Name},category={Category},description={Description}): this.Description → {Arg3}", name, category, description, this.Description);
        }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }
    }
}
