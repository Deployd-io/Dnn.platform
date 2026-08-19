// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowInvalidOperationException : WorkflowException
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowInvalidOperationException));
        /// <summary>Initializes a new instance of the <see cref="WorkflowInvalidOperationException"/> class.</summary>
        /// <param name="message">A message that describes the error.</param>
        public WorkflowInvalidOperationException(string message)
            : base(message)
        {
		Logger.LogDebug(">>> Entering WorkflowInvalidOperationException(message={Message})", message);
		Logger.LogDebug("<<< Exiting WorkflowInvalidOperationException(message={Message})", message);
		Logger.LogDebug(">>> Entering WorkflowInvalidOperationException()");
        }

        /// <summary>Initializes a new instance of the <see cref="WorkflowInvalidOperationException"/> class.</summary>
        public WorkflowInvalidOperationException()
        {
		Logger.LogDebug("<<< Exiting WorkflowInvalidOperationException()");
        }
    }
}
