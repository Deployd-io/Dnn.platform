// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.ComponentModel.Helpers
{
    public class Service2Impl : IService2
    {
        private readonly IService _service;

        public Service2Impl(IService service)
        {
			mockEventLogger.LogDebug(">>> Entering Service2Impl(service={Service})", service);
            this._service = service;
			mockEventLogger.LogDebug("Service2Impl(service={Service}): this._service → {Arg1}", service, this._service);
			mockEventLogger.LogDebug("Service2Impl(service={Service}): _service → {_service}", service, _service);
			mockEventLogger.LogDebug("<<< Exiting Service2Impl(service={Service})", service);
        }

        public IService Service
        {
            get { return this._service; }
        }
    }
}
