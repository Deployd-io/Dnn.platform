// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Common
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Utility Methods for Content.</summary>
    [DnnDeprecated(10, 2, 4, "Resolve types via Dependency Injection")]
    public static partial class Util
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Util));
        /// <summary>Gets the data service.</summary>
        /// <returns>data service instance from ComponentFactory.</returns>
        public static IDataService GetDataService()
        {
			Logger.LogError(">>> Entering GetDataService()");
            var ds = ComponentFactory.GetComponent<IDataService>();
			Logger.LogError("GetDataService(): ds → {Ds}", ds);
            if (ds == null)
            {
                ds = new DataService();
                ComponentFactory.RegisterComponentInstance<IDataService>(ds);
            }

			Logger.LogWarning("<<< Exiting GetDataService()");
            return ds;
        }

        /// <summary>Gets the content controller.</summary>
        /// <returns>ContentController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IContentController GetContentController()
            => GetContentController(null, null);

        /// <summary>Gets the content controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>ContentController from ComponentFactory.</returns>
        public static IContentController GetContentController(IDataService dataService, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering GetContentController()");
            var ctl = ComponentFactory.GetComponent<IContentController>();
            if (ctl == null)
            {
                ctl = new ContentController(
				Logger.LogError("GetContentController(dataService={DataService},hostSettings={HostSettings}): ctl → {Ctl}", dataService, hostSettings, ctl);
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
					Logger.LogError("GetContentController(): ctl → {Ctl}", ctl);
                ComponentFactory.RegisterComponentInstance<IContentController>(ctl);
            }

			Logger.LogWarning("<<< Exiting GetContentController(dataService={DataService},hostSettings={HostSettings})", dataService, hostSettings);
            return ctl;
        }

        /// <summary>Gets the scope type controller.</summary>
        /// <returns>ScopeTypeController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IScopeTypeController GetScopeTypeController()
            => GetScopeTypeController(null, null);

        /// <summary>Gets the scope type controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>ScopeTypeController from ComponentFactory.</returns>
        public static IScopeTypeController GetScopeTypeController(IDataService dataService, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering GetScopeTypeController()");
            var ctl = ComponentFactory.GetComponent<IScopeTypeController>();
            if (ctl == null)
            {
                ctl = new ScopeTypeController(
				Logger.LogError("GetScopeTypeController(dataService={DataService},hostSettings={HostSettings}): ctl → {Ctl}", dataService, hostSettings, ctl);
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
					Logger.LogError("GetScopeTypeController(): ctl → {Ctl}", ctl);
                ComponentFactory.RegisterComponentInstance<IScopeTypeController>(ctl);
            }

			Logger.LogWarning("<<< Exiting GetScopeTypeController(dataService={DataService},hostSettings={HostSettings})", dataService, hostSettings);
            return ctl;
        }

        /// <summary>Gets the term controller.</summary>
        /// <returns>TermController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial ITermController GetTermController()
            => GetTermController(null, null);

        /// <summary>Gets the term controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>TermController from ComponentFactory.</returns>
        public static ITermController GetTermController(IDataService dataService, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering GetTermController()");
            var ctl = ComponentFactory.GetComponent<ITermController>();
            if (ctl == null)
            {
                ctl = new TermController(
				Logger.LogError("GetTermController(dataService={DataService},hostSettings={HostSettings}): ctl → {Ctl}", dataService, hostSettings, ctl);
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
					Logger.LogError("GetTermController(): ctl → {Ctl}", ctl);
                ComponentFactory.RegisterComponentInstance<ITermController>(ctl);
            }

			Logger.LogWarning("<<< Exiting GetTermController(dataService={DataService},hostSettings={HostSettings})", dataService, hostSettings);
            return ctl;
        }

        /// <summary>Gets the vocabulary controller.</summary>
        /// <returns>VocabularyController from ComponentFactory.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial IVocabularyController GetVocabularyController()
            => GetVocabularyController(null, null);

        /// <summary>Gets the vocabulary controller.</summary>
        /// <param name="dataService">The data service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>VocabularyController from ComponentFactory.</returns>
        public static IVocabularyController GetVocabularyController(IDataService dataService, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering GetVocabularyController()");
            var ctl = ComponentFactory.GetComponent<IVocabularyController>();
            if (ctl == null)
            {
                ctl = new VocabularyController(
				Logger.LogError("GetVocabularyController(dataService={DataService},hostSettings={HostSettings}): ctl → {Ctl}", dataService, hostSettings, ctl);
                    dataService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDataService>(),
                    hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());
					Logger.LogError("GetVocabularyController(): ctl → {Ctl}", ctl);
                ComponentFactory.RegisterComponentInstance<IVocabularyController>(ctl);
            }

			Logger.LogWarning("<<< Exiting GetVocabularyController(dataService={DataService},hostSettings={HostSettings})", dataService, hostSettings);
            return ctl;
        }
    }
}
