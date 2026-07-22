// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An API controller for managing content moving through its workflow.</summary>
    /// <param name="contentController">The content controller.</param>
    /// <param name="workflowEngine">The workflow engine.</param>
    /// <param name="tabController">The tab controller.</param>
    [DnnAuthorize]
    public partial class ContentWorkflowServiceController(IContentController contentController, IWorkflowEngine workflowEngine, ITabController tabController)
        : DnnApiController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IContentController contentController = contentController ?? ContentController.Instance;
		Logger.LogDebug(">>> Entering ContentWorkflowServiceController(contentController={ContentController},workflowEngine={WorkflowEngine},tabController={TabController})", contentController, workflowEngine, tabController);
        private readonly IWorkflowEngine workflowEngine = workflowEngine ?? WorkflowEngine.Instance;
        private readonly ITabController tabController = tabController ?? TabController.Instance;

        /// <summary>Initializes a new instance of the <see cref="ContentWorkflowServiceController"/> class.</summary>
		Logger.LogDebug("<<< Exiting ContentWorkflowServiceController(contentController={ContentController},workflowEngine={WorkflowEngine},tabController={TabController})", contentController, workflowEngine, tabController);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public ContentWorkflowServiceController()
            : this(null, null, null)
        {
		Logger.LogError("Exception in ContentWorkflowServiceController(contentController={ContentController},workflowEngine={WorkflowEngine},tabController={TabController}): {Arg3}", contentController, workflowEngine, tabController, ex.Message);
        }

        /// <summary>Rejects a workflow.</summary>
        /// <param name="postData">The workflow notification to reject.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reject(NotificationDTO postData)
        {
			Logger.LogDebug(">>> Entering Reject(postData={PostData})", postData);
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                if (notification != null)
				Logger.LogDebug("Reject(postData={PostData}): notification → {Notification}", postData, notification);
                {
                    if (string.IsNullOrEmpty(notification.Context))
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                    }

                    string[] parameters = notification.Context.Split(':');

                    var stateTransaction = new StateTransaction
                    {
                        ContentItemId = int.Parse(parameters[0], CultureInfo.InvariantCulture),
                        CurrentStateId = int.Parse(parameters[2], CultureInfo.InvariantCulture),
                        Message = new StateTransactionMessage(),
                        UserId = this.UserInfo.UserID,
                    };
                    this.workflowEngine.DiscardState(stateTransaction);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in ContentWorkflowServiceController(contentController={ContentController},workflowEngine={WorkflowEngine},tabController={TabController}): {Arg3}", contentController, workflowEngine, tabController, ex.Message);
                Exceptions.LogException(exc);
				Logger.LogError("Exception in Reject(postData={PostData}): {Arg1}", postData, ex.Message);
            }

			Logger.LogDebug("<<< Exiting Reject(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        /// <summary>Approves a workflow.</summary>
        /// <param name="postData">The workflow notification to approve.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Approve(NotificationDTO postData)
        {
			Logger.LogDebug(">>> Entering Approve(postData={PostData})", postData);
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                if (notification != null)
				Logger.LogDebug("Approve(postData={PostData}): notification → {Notification}", postData, notification);
                {
                    if (string.IsNullOrEmpty(notification.Context))
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                    }

                    string[] parameters = notification.Context.Split(':');

                    var stateTransaction = new StateTransaction
                    {
                        ContentItemId = int.Parse(parameters[0], CultureInfo.InvariantCulture),
                        CurrentStateId = int.Parse(parameters[2], CultureInfo.InvariantCulture),
                        Message = new StateTransactionMessage(),
                        UserId = this.UserInfo.UserID,
                    };
                    this.workflowEngine.CompleteState(stateTransaction);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
				Logger.LogError("Exception in Approve(postData={PostData}): {Arg1}", postData, ex.Message);
            }

			Logger.LogDebug("<<< Exiting Approve(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        /// <summary>Complete a workflow state for the current page.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CompleteState()
        {
			Logger.LogDebug(">>> Entering CompleteState()");
            try
            {
                this.workflowEngine.CompleteState(this.BuildStateTransaction());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
				Logger.LogError("Exception in CompleteState(): {Arg0}", ex.Message);
            }

			Logger.LogDebug("<<< Exiting CompleteState()");
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        /// <summary>Discards a workflow state for the current page.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DiscardState()
        {
			Logger.LogDebug(">>> Entering DiscardState()");
            try
            {
                this.workflowEngine.DiscardState(this.BuildStateTransaction());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
				Logger.LogError("Exception in DiscardState(): {Arg0}", ex.Message);
            }

			Logger.LogDebug("<<< Exiting DiscardState()");
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        /// <summary>Complete a workflow for the current page.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CompleteWorkflow()
        {
			Logger.LogDebug(">>> Entering CompleteWorkflow()");
            try
            {
                this.workflowEngine.CompleteWorkflow(this.BuildStateTransaction());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
				Logger.LogError("Exception in CompleteWorkflow(): {Arg0}", ex.Message);
            }

			Logger.LogDebug("<<< Exiting CompleteWorkflow()");
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        /// <summary>Discards a workflow for the current page.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DiscardWorkflow()
        {
			Logger.LogDebug(">>> Entering DiscardWorkflow()");
            try
            {
                this.workflowEngine.DiscardWorkflow(this.BuildStateTransaction());
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
				Logger.LogError("Exception in DiscardWorkflow(): {Arg0}", ex.Message);
            }

			Logger.LogDebug("<<< Exiting DiscardWorkflow()");
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        private StateTransaction BuildStateTransaction()
        {
			Logger.LogDebug(">>> Entering BuildStateTransaction()");
            var portalId = this.PortalSettings.PortalId;
            var tabId = this.Request.FindTabId();
            var currentPage = this.tabController.GetTab(tabId, portalId);
			Logger.LogDebug("BuildStateTransaction(): tabId → {TabId}", tabId);
			Logger.LogDebug("BuildStateTransaction(): currentPage → {CurrentPage}", currentPage);
            var contentItemId = currentPage.ContentItemId;
            var contentItem = this.contentController.GetContentItem(contentItemId);
            var stateTransaction = new StateTransaction
			Logger.LogDebug("BuildStateTransaction(): contentItem → {ContentItem}", contentItem);
            {
                ContentItemId = contentItem.ContentItemId,
                CurrentStateId = contentItem.StateID,
                Message = new StateTransactionMessage(),
                UserId = this.UserInfo.UserID,
            };
			Logger.LogDebug("<<< Exiting BuildStateTransaction()");
            return stateTransaction;
        }
    }
}
