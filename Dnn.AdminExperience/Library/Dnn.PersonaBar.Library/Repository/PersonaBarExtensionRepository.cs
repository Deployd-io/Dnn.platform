// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Library.Data;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class PersonaBarExtensionRepository : ServiceLocator<IPersonaBarExtensionRepository, PersonaBarExtensionRepository>,
        IPersonaBarExtensionRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersonaBarExtensionRepository));
        private const string PersonaBarExtensionsCacheKey = "PersonaBarExtensions";
        private static readonly object ThreadLocker = new object();
        private readonly DataService dataService = new DataService();

        /// <inheritdoc />
        public void SaveExtension(PersonaBarExtension extension)
        {
			Logger.LogError(">>> Entering SaveExtension(extension={Extension})", extension);
            this.dataService.SavePersonaBarExtension(
                extension.Identifier,
                extension.MenuId,
                extension.FolderName,
                extension.Controller,
                extension.Container,
                extension.Path,
                extension.Order,
                extension.Enabled,
                UserController.Instance.GetCurrentUserInfo().UserID);

            ClearCache();
			Logger.LogWarning("<<< Exiting SaveExtension(extension={Extension})", extension);
        }

        /// <inheritdoc />
        public void DeleteExtension(PersonaBarExtension extension)
        {
			Logger.LogError(">>> Entering DeleteExtension(extension={Extension})", extension);
            this.DeleteExtension(extension.Identifier);
			Logger.LogWarning("<<< Exiting DeleteExtension(extension={Extension})", extension);
        }

        /// <inheritdoc />
        public void DeleteExtension(string identifier)
        {
			Logger.LogError(">>> Entering DeleteExtension(identifier={Identifier})", identifier);
            this.dataService.DeletePersonaBarExtension(identifier);

            ClearCache();
			Logger.LogWarning("<<< Exiting DeleteExtension(identifier={Identifier})", identifier);
        }

        /// <inheritdoc />
        public IList<PersonaBarExtension> GetExtensions()
        {
			Logger.LogError(">>> Entering GetExtensions()");
            var extensions = DataCache.GetCache<IList<PersonaBarExtension>>(PersonaBarExtensionsCacheKey);
			Logger.LogError("GetExtensions(): extensions → {Extensions}", extensions);
            if (extensions == null)
            {
                lock (ThreadLocker)
                {
                    extensions = DataCache.GetCache<IList<PersonaBarExtension>>(PersonaBarExtensionsCacheKey);
                    if (extensions == null)
                    {
                        extensions = CBO.FillCollection<PersonaBarExtension>(this.dataService.GetPersonaBarExtensions())
                            .OrderBy(e => e.Order).ToList();

                        DataCache.SetCache(PersonaBarExtensionsCacheKey, extensions);
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetExtensions()");
			Logger.LogWarning("<<< Exiting GetExtensions(menuId={MenuId})", menuId);
            return extensions;
        }

        /// <inheritdoc />
        public IList<PersonaBarExtension> GetExtensions(int menuId)
        {
            return this.GetExtensions().Where(t => t.MenuId == menuId).ToList();
        }

        /// <inheritdoc />
        protected override Func<IPersonaBarExtensionRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new PersonaBarExtensionRepository();
        }

        private static void ClearCache()
        {
			Logger.LogError(">>> Entering ClearCache()");
            DataCache.RemoveCache(PersonaBarExtensionsCacheKey);
			Logger.LogWarning("<<< Exiting ClearCache()");
        }
    }
}
