// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System.Collections;
    using System.Web.UI;

    /// <summary>An RSS <see cref="DataSourceView"/>.</summary>
    public class RssDataSourceView : DataSourceView
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly RssDataSource owner;

        /// <summary>Initializes a new instance of the <see cref="RssDataSourceView"/> class.</summary>
        /// <inheritdoc cref="DataSourceView(IDataSource, string)"/>
        internal RssDataSourceView(RssDataSource owner, string viewName)
            : base(owner, viewName)
        {
			Logger.LogDebug(">>> Entering RssDataSourceView(owner={Owner},viewName={ViewName})", owner, viewName);
            this.owner = owner;
			Logger.LogDebug("<<< Exiting RssDataSourceView(owner={Owner},viewName={ViewName})", owner, viewName);
			Logger.LogDebug("RssDataSourceView(owner={Owner},viewName={ViewName}): owner → {Owner1}", owner, viewName, owner);
			Logger.LogDebug("RssDataSourceView(owner={Owner},viewName={ViewName}): this.owner → {Arg2}", owner, viewName, this.owner);
        }

        /// <inheritdoc />
        public override void Select(DataSourceSelectArguments arguments, DataSourceViewSelectCallback callback)
        {
			Logger.LogDebug(">>> Entering Select(arguments={Arguments},callback={Callback})", arguments, callback);
            callback(this.ExecuteSelect(arguments));
			Logger.LogDebug("<<< Exiting Select(arguments={Arguments},callback={Callback})", arguments, callback);
        }

        /// <inheritdoc />
        protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
        {
			Logger.LogDebug(">>> Entering ExecuteSelect(arguments={Arguments})", arguments);
			Logger.LogDebug("<<< Exiting ExecuteSelect(arguments={Arguments})", arguments);
            return this.owner.Channel.SelectItems(this.owner.MaxItems);
        }
    }
}
