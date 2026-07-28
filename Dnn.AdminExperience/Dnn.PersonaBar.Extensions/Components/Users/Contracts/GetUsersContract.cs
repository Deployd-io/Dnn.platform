// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Contracts
{
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Users.Components.Dto;

    [DataContract]
    public class GetUsersContract
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public GetUsersContract()
        {
			Logger.LogDebug(">>> Entering GetUsersContract()");
            this.SortColumn = "Joined";
            this.SortAscending = false;
			Logger.LogDebug("<<< Exiting GetUsersContract()");
			Logger.LogDebug("GetUsersContract(): this.SortColumn → {Arg0}", this.SortColumn);
			Logger.LogDebug("GetUsersContract(): this.SortAscending → {Arg0}", this.SortAscending);
        }

        public int PortalId { get; set; }

        public string SearchText { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string SortColumn { get; set; }

        public bool SortAscending { get; set; }

        public UserFilters Filter { get; set; }
    }
}
