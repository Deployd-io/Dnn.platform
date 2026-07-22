// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.WebPages;

    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Razor.Helpers;

    /// <summary>Renders a razor file.</summary>
    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial class RazorEngine
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public RazorEngine(string razorScriptFile, ModuleInstanceContext moduleContext, string localResourceFile)
        {
			Logger.LogDebug(">>> Entering RazorEngine(razorScriptFile={RazorScriptFile},moduleContext={ModuleContext},localResourceFile={LocalResourceFile})", razorScriptFile, moduleContext, localResourceFile);
            this.RazorScriptFile = razorScriptFile;
            this.ModuleContext = moduleContext;
			Logger.LogDebug("RazorEngine(razorScriptFile={RazorScriptFile},moduleContext={ModuleContext},localResourceFile={LocalResourceFile}): this.RazorScriptFile → {Arg3}", razorScriptFile, moduleContext, localResourceFile, this.RazorScriptFile);
			Logger.LogDebug("RazorEngine(razorScriptFile={RazorScriptFile},moduleContext={ModuleContext},localResourceFile={LocalResourceFile}): this.ModuleContext → {Arg3}", razorScriptFile, moduleContext, localResourceFile, this.ModuleContext);
            this.LocalResourceFile = localResourceFile ?? Path.Combine(Path.GetDirectoryName(razorScriptFile), Localization.LocalResourceDirectory, Path.GetFileName(razorScriptFile) + ".resx");

            try
            {
                this.InitWebpage();
            }
            catch (HttpParseException)
            {
				Logger.LogError("Exception in RazorEngine(razorScriptFile={RazorScriptFile},moduleContext={ModuleContext},localResourceFile={LocalResourceFile}): {Arg3}", razorScriptFile, moduleContext, localResourceFile, ex.Message);
                throw;
            }
            catch (HttpCompileException)
            {
                throw;
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
			Logger.LogDebug("<<< Exiting RazorEngine(razorScriptFile={RazorScriptFile},moduleContext={ModuleContext},localResourceFile={LocalResourceFile})", razorScriptFile, moduleContext, localResourceFile);
        }

        public DotNetNukeWebPage Webpage { get; set; }

        protected HttpContextBase HttpContext
        {
            get { return new HttpContextWrapper(System.Web.HttpContext.Current); }
        }

        protected string RazorScriptFile { get; set; }

        protected ModuleInstanceContext ModuleContext { get; set; }

        protected string LocalResourceFile { get; set; }

        /// <summary>Get the type of the model.</summary>
        /// <returns>A type or <see langword="null"/>.</returns>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial Type RequestedModelType()
        {
			Logger.LogDebug(">>> Entering RequestedModelType()");
            if (this.Webpage != null)
            {
                var webpageType = this.Webpage.GetType();
                if (webpageType.BaseType.IsGenericType)
				Logger.LogDebug("RequestedModelType(): webpageType → {WebpageType}", webpageType);
                {
                    return webpageType.BaseType.GetGenericArguments()[0];
                }
            }

			Logger.LogDebug("<<< Exiting RequestedModelType()");
            return null;
        }

        /// <summary>Renders a razor file into a <see cref="TextWriter"/>.</summary>
        /// <param name="writer">The writer into which to render the HTML.</param>
        /// <param name="model">The model.</param>
        /// <typeparam name="T">The type of the model.</typeparam>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial void Render<T>(TextWriter writer, T model)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer},model={Model})", writer, model);
            try
            {
                this.Webpage.ExecutePageHierarchy(new WebPageContext(this.HttpContext, this.Webpage, model), writer, this.Webpage);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in Render(writer={Writer},model={Model}): {Arg2}", writer, model, ex.Message);
                Exceptions.LogException(exc);
            }
        }

        /// <summary>Renders a razor file into a <see cref="TextWriter"/>.</summary>
        /// <param name="writer">The writer into which to render the HTML.</param>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial void Render(TextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            try
			Logger.LogDebug("<<< Exiting Render(writer={Writer},model={Model})", writer, model);
            {
                this.Webpage.ExecutePageHierarchy(new WebPageContext(this.HttpContext, this.Webpage, null), writer, this.Webpage);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in Render(writer={Writer}): {Arg1}", writer, ex.Message);
                Exceptions.LogException(exc);
            }
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }

        /// <summary>Renders a razor file into a <see cref="TextWriter"/>.</summary>
        /// <param name="writer">The writer into which to render the HTML.</param>
        /// <param name="context">The web page context.</param>
        [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
        public partial void Render(TextWriter writer, WebPageContext context)
        {
            try
            {
                this.Webpage.ExecutePageHierarchy(context, writer, this.Webpage);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in Render(writer={Writer},context={Context}): {Arg2}", writer, context, ex.Message);
                Exceptions.LogException(exc);
            }
        }

        private object CreateWebPageInstance()
        {
			Logger.LogDebug(">>> Entering CreateWebPageInstance()");
            var compiledType = BuildManager.GetCompiledType(this.RazorScriptFile);
            object objectValue = null;
			Logger.LogDebug("CreateWebPageInstance(): compiledType → {CompiledType}", compiledType);
			Logger.LogDebug("CreateWebPageInstance(): objectValue → {ObjectValue}", objectValue);
            if (compiledType != null)
            {
                objectValue = RuntimeHelpers.GetObjectValue(Activator.CreateInstance(compiledType));
            }

			Logger.LogDebug("<<< Exiting CreateWebPageInstance()");
            return objectValue;
        }

        private void InitHelpers(DotNetNukeWebPage webPage)
        {
			Logger.LogDebug(">>> Entering InitHelpers(webPage={WebPage})", webPage);
            webPage.Dnn = new DnnHelper(this.ModuleContext);
            webPage.Html = new HtmlHelper(this.ModuleContext, this.LocalResourceFile);
            webPage.Url = new UrlHelper(this.ModuleContext);
			Logger.LogDebug("<<< Exiting InitHelpers(webPage={WebPage})", webPage);
        }

        private void InitWebpage()
        {
			Logger.LogDebug(">>> Entering InitWebpage()");
            if (!string.IsNullOrEmpty(this.RazorScriptFile))
            {
                var objectValue = RuntimeHelpers.GetObjectValue(this.CreateWebPageInstance());
                if (objectValue == null)
				Logger.LogDebug("InitWebpage(): objectValue → {ObjectValue}", objectValue);
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage found at '{0}' was not created.", new object[] { this.RazorScriptFile }));
                }

                this.Webpage = objectValue as DotNetNukeWebPage;
                if (this.Webpage == null)
				Logger.LogDebug("InitWebpage(): this.Webpage → {Arg0}", this.Webpage);
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The webpage at '{0}' must derive from DotNetNukeWebPage.", new object[] { this.RazorScriptFile }));
                }

                this.Webpage.Context = this.HttpContext;
                this.Webpage.VirtualPath = VirtualPathUtility.GetDirectory(this.RazorScriptFile);
                this.InitHelpers(this.Webpage);
            }
			Logger.LogDebug("<<< Exiting InitWebpage()");
        }
    }
}
