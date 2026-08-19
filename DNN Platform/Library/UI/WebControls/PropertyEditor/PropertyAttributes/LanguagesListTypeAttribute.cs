// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;

    using DotNetNuke.Services.Localization;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LanguagesListTypeAttribute : Attribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesListTypeAttribute));
        private readonly LanguagesListType listType;

        /// <summary>Initializes a new instance of the <see cref="LanguagesListTypeAttribute"/> class.</summary>
        /// <param name="type">The type of List.</param>
        public LanguagesListTypeAttribute(LanguagesListType type)
        {
			Logger.LogDebug(">>> Entering LanguagesListTypeAttribute(type={Type})", type);
            this.listType = type;
			Logger.LogDebug("LanguagesListTypeAttribute(type={Type}): this.listType → {Arg1}", type, this.listType);
			Logger.LogDebug("LanguagesListTypeAttribute(type={Type}): listType → {ListType}", type, listType);
			Logger.LogDebug("<<< Exiting LanguagesListTypeAttribute(type={Type})", type);
        }

        public LanguagesListType ListType
        {
            get
            {
                return this.listType;
            }
        }
    }
}
