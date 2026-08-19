// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Executers.Dto
{
    using System;
    using System.Data;

    using DotNetNuke.Entities.Modules;
	using Microsoft.Extensions.Logging;

    public class CopyModuleItem : IHydratable
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public int Id { get; set; }

        public string Title { get; set; }

        public int KeyID
        {
            get { return this.Id; }
            set { this.Id = value; }
        }

        public void Fill(IDataReader dr)
        {
			_logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.Id = Convert.ToInt32(dr["ModuleId"]);
            this.Title = Convert.ToString(dr["ModuleTitle"]);
			_logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
