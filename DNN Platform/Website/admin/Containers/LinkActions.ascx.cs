// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which renders module actions as link buttons.</summary>
    public partial class LinkActions : ActionBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1309:FieldNamesMustNotBeginWithUnderscore", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected string _itemSeparator = string.Empty;

        /// <summary>Initializes a new instance of the <see cref="LinkActions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public LinkActions()
            : this(null)
        {
		Logger.LogDebug(">>> Entering LinkActions()");
        }
		Logger.LogDebug("<<< Exiting LinkActions()");

        /// <summary>Initializes a new instance of the <see cref="LinkActions"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        public LinkActions(IEventLogger eventLogger)
            : base(eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
        }

        public string ItemSeparator
        {
            get => this._itemSeparator;
            set => this._itemSeparator = value;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            try
            {
                {
                    if (this.ActionRoot.Visible)
                    {
                        // Is Root Menu visible?
                        if (this.Controls.Count > 0)
                        {
                            this.Controls.Clear();
                        }

                        var preSpacer = new LiteralControl(this.ItemSeparator);
                        this.Controls.Add(preSpacer);
						Logger.LogDebug("OnLoad(e={E}): preSpacer → {PreSpacer}", e, preSpacer);

                        // Add Menu Items
                        foreach (ModuleAction action in this.ActionRoot.Actions)
                        {
                            if (action.Title == "~")
                            {
                                // not supported in this Action object
                            }
                            else
                            {
                                if (action.Visible)
                                {
                                    if ((this.ModuleControl.ModuleContext.EditMode && Globals.IsAdminControl() == false) ||
                                        (action.Secure != SecurityAccessLevel.Anonymous && action.Secure != SecurityAccessLevel.View))
                                    {
                                        var moduleActionLink = new LinkButton();
                                        moduleActionLink.Text = action.Title;
										Logger.LogDebug("OnLoad(e={E}): moduleActionLink → {ModuleActionLink}", e, moduleActionLink);
										Logger.LogDebug("OnLoad(e={E}): moduleActionLink.Text → {Arg1}", e, moduleActionLink.Text);
										Logger.LogDebug("OnLoad(e={E}): moduleActionLink.CssClass → {Arg1}", e, moduleActionLink.CssClass);
										Logger.LogDebug("OnLoad(e={E}): moduleActionLink.ID → {Arg1}", e, moduleActionLink.ID);
                                        moduleActionLink.CssClass = "CommandButton";
                                        moduleActionLink.ID = "lnk" + action.ID;

                                        moduleActionLink.Click += this.LinkAction_Click;

                                        this.Controls.Add(moduleActionLink);
                                        var spacer = new LiteralControl(this.ItemSeparator);
                                        this.Controls.Add(spacer);
										Logger.LogDebug("OnLoad(e={E}): spacer → {Spacer}", e, spacer);
                                    }
                                }
                            }
                        }
                    }
                }

                // Need to determine if this action list actually has any items.
                if (this.Controls.Count > 0)
                {
                    this.Visible = true;
					Logger.LogDebug("OnLoad(e={E}): this.Visible → {Arg1}", e, this.Visible);
                }
                else
                {
                    this.Visible = false;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            if (string.IsNullOrEmpty(this._itemSeparator))
            {
                this._itemSeparator = "&nbsp;&nbsp;";
				Logger.LogDebug("OnPreRender(e={E}): this._itemSeparator → {Arg1}", e, this._itemSeparator);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private void LinkAction_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering LinkAction_Click(sender={Sender},e={E})", sender, e);
            try
            {
                this.ProcessAction(((LinkButton)sender).ID.Substring(3));
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in LinkAction_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting LinkAction_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
