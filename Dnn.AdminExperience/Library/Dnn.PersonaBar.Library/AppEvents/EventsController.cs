// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.AppEvents
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Dnn.PersonaBar.Library.AppEvents.Attributes;
    using Dnn.PersonaBar.Library.Common;
    using DotNetNuke.Collections;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;

    public class EventsController : ServiceLocator<IEventsController, EventsController>, IEventsController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventsController));

        private static readonly object LockThis = new object();
        private static bool isInitialized;

        /// <inheritdoc />
        public void ApplicationStartEvent()
        {
			Logger.LogError(">>> Entering ApplicationStartEvent()");
            lock (LockThis)
            {
                if (isInitialized)
                {
					Logger.LogError("Throwing InvalidOperationException from ApplicationStartEvent()");
                    throw new InvalidOperationException("ApplicationStartEvent cannot be called more than once");
                }

                isInitialized = true;
				Logger.LogDebug("ApplicationStartEvent(): isInitialized → {IsInitialized}", isInitialized);
            }

            GetEventsImplements<IAppEvents>().ForEach(instance =>
            {
                try
                {
                    instance.ApplicationBegin();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "{0}.ApplicationStart threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName,
                        e.Message,
                        e.StackTrace);
                }
            });
			Logger.LogWarning("<<< Exiting ApplicationStartEvent()");
        }

        /// <inheritdoc />
        public void ApplicationEndEvent()
        {
			Logger.LogError(">>> Entering ApplicationEndEvent()");
            GetEventsImplements<IAppEvents>().ForEach(instance =>
            {
                try
                {
                    instance.ApplicationEnd();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "{0}.ApplicationEnd threw an exception.  {1}\r\n{2}",
                        instance.GetType().FullName,
                        e.Message,
                        e.StackTrace);
                }
            });
			Logger.LogWarning("<<< Exiting ApplicationEndEvent()");
        }

        /// <inheritdoc />
        protected override Func<IEventsController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new EventsController();
        }

        private static IEnumerable<T> GetEventsImplements<T>()
            where T : class
        {
			Logger.LogError(">>> Entering GetEventsImplements()");
            var types = GetAllEventTypes<T>();

			Logger.LogError("GetEventsImplements(): types → {Types}", types);
            foreach (var type in types)
            {
                T appEventHandler;
                try
                {
                    appEventHandler = Activator.CreateInstance(type) as T;
					Logger.LogError("GetEventsImplements(): appEventHandler → {AppEventHandler}", appEventHandler);
                }
                catch (Exception e)
                {
					Logger.LogDebug("GetEventsImplements(): appEventHandler → {AppEventHandler}", appEventHandler);
                    Logger.ErrorFormat(
                        CultureInfo.InvariantCulture,
                        "Unable to create {0} while calling Application start implementors.  {1}",
                        type.FullName,
                        e.Message);
                    appEventHandler = null;
                }

                if (appEventHandler != null)
                {
                    yield return appEventHandler;
                }
            }
			Logger.LogWarning("<<< Exiting GetEventsImplements()");
        }

        private static IEnumerable<Type> GetAllEventTypes<T>()
            where T : class
        {
			Logger.LogError(">>> Entering GetAllEventTypes()");
            var typeLocator = new TypeLocator();
			Logger.LogWarning("<<< Exiting GetAllEventTypes()");
					Logger.LogError("GetAllEventTypes(): typeLocator → {TypeLocator}", typeLocator);
            return typeLocator.GetAllMatchingTypes(
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true, } &&
                     typeof(T).IsAssignableFrom(t) &&
                     (IgnoreVersionMatchCheck(t) || VersionMatched(t)));
        }

        private static bool IgnoreVersionMatchCheck(Type type)
        {
			Logger.LogError(">>> Entering IgnoreVersionMatchCheck(type={Type})", type);
			Logger.LogWarning("<<< Exiting IgnoreVersionMatchCheck(type={Type})", type);
            return type.GetCustomAttributes(true).Any(a => a is IgnoreVersionMatchCheckAttribute);
        }

        private static bool VersionMatched(Type t)
        {
			Logger.LogError(">>> Entering VersionMatched(t={T})", t);
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var typeVersion = t.Assembly.GetName().Version;

            var matched = currentVersion.Major == typeVersion.Major &&
                   currentVersion.Minor == typeVersion.Minor &&
                   currentVersion.Build == typeVersion.Build;

            if (!matched)
            {
                Logger.InfoFormat(
                    CultureInfo.InvariantCulture,
                    "Type \"{0}\"'s version ({1}) doesn't match current version({2}) so ignored",
                    t.FullName,
                    typeVersion,
                    currentVersion);
            }

			Logger.LogWarning("<<< Exiting VersionMatched(t={T})", t);
            return matched;
        }
    }
}
