// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    using DotNetNuke.Services.Localization;

    public class WorkflowNameAlreadyExistsException : WorkflowException
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowNameAlreadyExistsException));
        /// <summary>Initializes a new instance of the <see cref="WorkflowNameAlreadyExistsException"/> class.</summary>
        public WorkflowNameAlreadyExistsException()
            : base(Localization.GetString("WorkflowNameAlreadyExistsException", Localization.ExceptionsResourceFile))
        {
		Logger.LogDebug(">>> Entering WorkflowNameAlreadyExistsException()");
		Logger.LogDebug("<<< Exiting WorkflowNameAlreadyExistsException()");
        }
    }
}
