// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Roles.Components.Prompt.Models
{
    using System.Diagnostics.CodeAnalysis;

    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Security.Roles;

    public class RoleModel : RoleModelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="RoleModel"/> class.</summary>
        public RoleModel()
        {
		Logger.LogDebug(">>> Entering RoleModel()");
		Logger.LogDebug("<<< Exiting RoleModel()");
        }

        /// <summary>Initializes a new instance of the <see cref="RoleModel"/> class.</summary>
        /// <param name="role">The role info.</param>
        public RoleModel(RoleInfo role)
            : base(role)
        {
            this.ModifiedDate = role.LastModifiedOnDate.ToPromptLongDateString();
            this.CreatedDate = role.CreatedOnDate.ToPromptLongDateString();
            this.CreatedBy = role.CreatedByUserID;
			Logger.LogDebug("RoleModel(role={Role}): this.CreatedBy → {Arg1}", role, this.CreatedBy);
            this.Description = role.Description;
        }

		Logger.LogDebug("RoleModel(role={Role}): this.Description → {Arg1}", role, this.Description);
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __CreatedBy => $"get-user {this.CreatedBy}";

        public string Description { get; set; }

        public string CreatedDate { get; set; }

        public int CreatedBy { get; set; }
    }
}
