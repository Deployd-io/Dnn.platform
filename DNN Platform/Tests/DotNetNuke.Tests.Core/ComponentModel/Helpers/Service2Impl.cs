// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.ComponentModel.Helpers
{
    public class Service2Impl : IService2
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IService _service;

        public Service2Impl(IService service)
        {
			Logger.LogDebug(">>> Entering Service2Impl(service={Service})", service);
            this._service = service;
			Logger.LogDebug("<<< Exiting Service2Impl(service={Service})", service);
			Logger.LogDebug("Service2Impl(service={Service}): _service → {_service}", service, _service);
			Logger.LogDebug("Service2Impl(service={Service}): this._service → {Arg1}", service, this._service);
        }

        public IService Service
        {
            get { return this._service; }
        }
    }
}
