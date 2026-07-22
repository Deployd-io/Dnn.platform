// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    using DotNetNuke.Services.Localization;

    public class WorkflowStateNameAlreadyExistsException : WorkflowException
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="WorkflowStateNameAlreadyExistsException"/> class.</summary>
        public WorkflowStateNameAlreadyExistsException()
            : base(Localization.GetString("WorkflowStateNameAlreadyExistsException", Localization.ExceptionsResourceFile))
        {
		Logger.LogDebug(">>> Entering WorkflowStateNameAlreadyExistsException()");
		Logger.LogDebug("<<< Exiting WorkflowStateNameAlreadyExistsException()");
        }
    }
}
