// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class SafeDirectoryCatalog : ComposablePartCatalog
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SafeDirectoryCatalog));
        private readonly AggregateCatalog catalog;

        /// <summary>Initializes a new instance of the <see cref="SafeDirectoryCatalog"/> class.</summary>
        /// <param name="directory">The directory path in which to search for assemblies (recursively).</param>
        public SafeDirectoryCatalog(string directory)
        {
			Logger.LogDebug(">>> Entering SafeDirectoryCatalog(directory={Directory})", directory);
            var files = Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories);

			Logger.LogDebug("SafeDirectoryCatalog(directory={Directory}): files → {Files}", directory, files);
            this.catalog = new AggregateCatalog();
			Logger.LogDebug("SafeDirectoryCatalog(directory={Directory}): catalog → {Catalog}", directory, catalog);

            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);
					Logger.LogDebug("SafeDirectoryCatalog(directory={Directory}): asmCat → {AsmCat}", directory, asmCat);

                    // Force MEF to load the plugin and figure out if there are any exports
                    // good assemblies will not throw the RTLE exception and can be added to the catalog
                    if (asmCat.Parts.ToList().Count > 0)
                    {
                        this.catalog.Catalogs.Add(asmCat);
                    }
                }
                catch (ReflectionTypeLoadException)
                {
				Logger.LogError("Exception in SafeDirectoryCatalog(directory={Directory}): {Arg1}", directory, ReflectionTypeLoadException.getMessage());
                }
                catch (BadImageFormatException)
                {
				Logger.LogError("Exception in SafeDirectoryCatalog(directory={Directory}): {Arg1}", directory, BadImageFormatException.getMessage());
                }
                catch (FileLoadException)
                {
					Logger.LogError("Exception in SafeDirectoryCatalog(directory={Directory}): {Arg1}", directory, FileLoadException.getMessage());
                    // ignore when the assembly load failed.
                }
            }
			Logger.LogDebug("<<< Exiting SafeDirectoryCatalog(directory={Directory})", directory);
        }

        /// <inheritdoc />
        public override IQueryable<ComposablePartDefinition> Parts
        {
            get
            {
                return this.catalog.Parts;
            }
        }
    }
}
