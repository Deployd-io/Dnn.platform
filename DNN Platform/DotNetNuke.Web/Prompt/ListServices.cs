// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Prompt
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Prompt;
    using DotNetNuke.Abstractions.Users;
    using DotNetNuke.Prompt;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This is a (Prompt) Console Command. You should not reference this class directly. It is to be used solely through Prompt.</summary>
    [ConsoleCommand("list-services", "Prompt_DebugCategory", "Prompt_ListServices_Description")]
    public class ListServices : ConsoleCommand
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ListServices));
        /// <inheritdoc />
        public override string LocalResourceFile => Constants.DefaultPromptResourceFile;

        /// <inheritdoc />
        public override void Initialize(string[] args, IPortalSettings portalSettings, IUserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            if (!userInfo.IsSuperUser)
            {
                this.AddMessage(this.LocalizeString("Prompt_ListServices_Unauthorized"));
            }

            this.ParseParameters(this);
			Logger.LogDebug("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override IConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            if (!this.User.IsSuperUser)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_ListServices_Unauthorized"));
            }

            var services = DependencyInjectionInitialize.ServiceCollection.Select(
                    descriptor => new
                    {
                        LifeTime = descriptor.Lifetime.ToString("G"),
                        Service = this.GetTypeName(descriptor.ServiceType),
                        Implementation = this.GetImplementationText(descriptor),
                    })
                .OrderBy(desc => desc.Service.Output)
                .ThenBy(desc => desc.Implementation.Output)
                .ToList();
				Logger.LogDebug("Run(): Implementation → {Implementation}", Implementation);
				Logger.LogDebug("Run(): LifeTime → {LifeTime}", LifeTime);
				Logger.LogDebug("Run(): Service → {Service}", Service);
			Logger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel
            {
                Data = services,
                Records = services.Count,
            };
        }

        private IConsoleOutput GetImplementationText(ServiceDescriptor descriptor)
        {
			Logger.LogDebug(">>> Entering GetImplementationText(descriptor={Descriptor})", descriptor);
            if (descriptor.ImplementationInstance != null)
            {
                return new HtmlOutput(this.LocalizeString("Prompt_ListServices_ImplementationInstance"));
            }

            if (descriptor.ImplementationFactory != null)
            {
			Logger.LogDebug("<<< Exiting GetImplementationText(descriptor={Descriptor})", descriptor);
                return new HtmlOutput(this.LocalizeString("Prompt_ListServices_ImplementationFactory"));
            }

            return this.GetTypeName(descriptor.ImplementationType);
        }

        private IConsoleOutput GetTypeName(Type type)
        {
			Logger.LogDebug(">>> Entering GetTypeName(type={Type})", type);
            if (type == null)
            {
			Logger.LogDebug("<<< Exiting GetTypeName(type={Type})", type);
                return new HtmlOutput(this.LocalizeString("Prompt_ListServices_None"));
            }

            return new TextOutput(type.FullName ?? type.Name);
        }
    }
}
