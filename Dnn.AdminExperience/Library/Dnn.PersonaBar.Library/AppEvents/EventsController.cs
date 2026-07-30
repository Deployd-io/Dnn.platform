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
			Logger.LogDebug(">>> Entering ApplicationStartEvent()");
            lock (LockThis)
            {
                if (isInitialized)
                {
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
			Logger.LogDebug("<<< Exiting ApplicationStartEvent()");
        }

        /// <inheritdoc />
        public void ApplicationEndEvent()
        {
			Logger.LogDebug(">>> Entering ApplicationEndEvent()");
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
			Logger.LogDebug("<<< Exiting ApplicationEndEvent()");
        }

        /// <inheritdoc />
        protected override Func<IEventsController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new EventsController();
        }

        private static IEnumerable<T> GetEventsImplements<T>()
            where T : class
        {
			Logger.LogDebug(">>> Entering GetEventsImplements()");
            var types = GetAllEventTypes<T>();

			Logger.LogDebug("GetEventsImplements(): types → {Types}", types);
            foreach (var type in types)
            {
                T appEventHandler;
                try
                {
                    appEventHandler = Activator.CreateInstance(type) as T;
					Logger.LogDebug("GetEventsImplements(): appEventHandler → {AppEventHandler}", appEventHandler);
                }
                catch (Exception e)
                {
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
			Logger.LogDebug("<<< Exiting GetEventsImplements()");
        }

        private static IEnumerable<Type> GetAllEventTypes<T>()
            where T : class
        {
			Logger.LogDebug(">>> Entering GetAllEventTypes()");
            var typeLocator = new TypeLocator();
            return typeLocator.GetAllMatchingTypes(
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true, } &&
                     typeof(T).IsAssignableFrom(t) &&
                     (IgnoreVersionMatchCheck(t) || VersionMatched(t)));
			Logger.LogDebug("<<< Exiting GetAllEventTypes()");
			Logger.LogDebug("GetAllEventTypes(): typeLocator → {TypeLocator}", typeLocator);
        }

        private static bool IgnoreVersionMatchCheck(Type type)
        {
			Logger.LogDebug(">>> Entering IgnoreVersionMatchCheck(type={Type})", type);
			Logger.LogDebug("<<< Exiting IgnoreVersionMatchCheck(type={Type})", type);
            return type.GetCustomAttributes(true).Any(a => a is IgnoreVersionMatchCheckAttribute);
        }

        private static bool VersionMatched(Type t)
        {
			Logger.LogDebug(">>> Entering VersionMatched(t={T})", t);
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

			Logger.LogDebug("<<< Exiting VersionMatched(t={T})", t);
            return matched;
        }
    }
}
