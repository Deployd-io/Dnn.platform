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
        public GetUsersContract()
        {
			eventLogger.LogDebug(">>> Entering GetUsersContract()");
            this.SortColumn = "Joined";
            this.SortAscending = false;
			eventLogger.LogDebug("GetUsersContract(): this.SortColumn → {Arg0}", this.SortColumn);
			eventLogger.LogDebug("<<< Exiting GetUsersContract()");
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
