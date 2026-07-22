// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Diagnostics;

    using DotNetNuke.Web.Api;

    // various ServiceRouteMappers that will be reflected upon by the tests
    public class ReflectedServiceRouteMappers
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public class EmbeddedServiceRouteMapper : IServiceRouteMapper
        {
            public void RegisterRoutes(IMapRoute mapRouteManager)
            {
				Logger.LogDebug(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
                throw new NotImplementedException();
            }
        }
    }

    public class ExceptionOnCreateInstanceServiceRouteMapper : IServiceRouteMapper
    {
        public ExceptionOnCreateInstanceServiceRouteMapper(int i)
        {
			Logger.LogDebug(">>> Entering ExceptionOnCreateInstanceServiceRouteMapper(i={I})", i);
            // no default constructor prevents Activator.CreateInstance from working
            Debug.WriteLine(i);
			Logger.LogDebug("<<< Exiting ExceptionOnCreateInstanceServiceRouteMapper(i={I})", i);
        }

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }

    public class ExceptionOnRegisterServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }

    public class FakeServiceRouteMapper : IServiceRouteMapper
    {
        public static int RegistrationCalls { get; set; }

        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            RegistrationCalls++;
        }
    }

    public abstract class AbstractServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }

    internal class InternalServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            throw new NotImplementedException();
        }
    }
}
