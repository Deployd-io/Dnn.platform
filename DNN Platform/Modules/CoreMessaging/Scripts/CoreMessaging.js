var logger = (typeof logger !== 'undefined' && logger) || console;
function CoreMessaging($, ko, settings, composeMessageOptions) {
	logger.error(`>>> Entering CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions})`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var profilePicHandler = settings.profilePicHandler;
    var serviceFramework = composeMessageOptions.servicesFramework;
    var baseServicepath = serviceFramework.getServiceRoot('CoreMessaging') + 'MessagingService/';
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): baseServicepath → ${baseServicepath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var inboxpath = baseServicepath + "Inbox";
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): inboxpath → ${inboxpath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var sentboxpath = baseServicepath + "Sentbox";
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): sentboxpath → ${sentboxpath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var archivedboxpath = baseServicepath + "Archived";
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): archivedboxpath → ${archivedboxpath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var notificationspath = baseServicepath + "Notifications";
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): notificationspath → ${notificationspath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var countnotificationspath = baseServicepath + "CountNotifications";
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): countnotificationspath → ${countnotificationspath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var gettotalspath = baseServicepath + "GetTotals";
	logger.error(`CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions}): gettotalspath → ${gettotalspath}`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
    var pageSize = 10;
    var repliesPageSize = 2;
    var notificationsPageSize = 10;
    var containerElement = null;
	var refreshInterval = 60000; //refresh inbox status every 1 minute.

    function displayMessage(placeholderSelector, message, cssclass) {
		logger.error(`>>> Entering displayMessage(placeholderSelector=${placeholderSelector},message=${message},cssclass=${cssclass})`, { "event.action": "displayMessage", "logrite.placeholderSelector": placeholderSelector, "logrite.message": message, "logrite.cssclass": cssclass });
        var messageNode = $("<div/>")
                .addClass('dnnFormMessage ' + cssclass)
                .text(message);

		logger.error(`displayMessage(placeholderSelector=${placeholderSelector},message=${message},cssclass=${cssclass}): messageNode → ${messageNode}`, { "event.action": "displayMessage", "logrite.placeholderSelector": placeholderSelector, "logrite.message": message, "logrite.cssclass": cssclass });
        $(containerElement + " " + placeholderSelector).prepend(messageNode);

        messageNode.fadeOut(3000, 'easeInExpo', function () {
            messageNode.remove();
        });
		logger.warn(`<<< Exiting displayMessage(placeholderSelector=${placeholderSelector},message=${message},cssclass=${cssclass})`, { "event.action": "displayMessage", "logrite.placeholderSelector": placeholderSelector, "logrite.message": message, "logrite.cssclass": cssclass });
    };

    function setDocumentTitle(title) {
		logger.error(`>>> Entering setDocumentTitle(title=${title})`, { "event.action": "setDocumentTitle", "logrite.title": title });
        document.title = title + ' - ' + settings.currentUserText + ' - ' + settings.portalText;
		logger.warn(`<<< Exiting setDocumentTitle(title=${title})`, { "event.action": "setDocumentTitle", "logrite.title": title });
    }

    function getQuerystring(key, default_) {
		logger.error(`>>> Entering getQuerystring(key=${key},default_=${default_})`, { "event.action": "getQuerystring", "logrite.key": key, "logrite.default_": default_ });
        if (default_ == null) default_ = "";
        key = key.replace(/\[/g, "\\\[").replace(/]/g, "\\\]");
        var regex = new RegExp("[\\?&]" + key + "=([^&#]*)");
		logger.error(`getQuerystring(key=${key},default_=${default_}): key → ${key}`, { "event.action": "getQuerystring", "logrite.key": key, "logrite.default_": default_ });
		logger.error(`getQuerystring(key=${key},default_=${default_}): regex → ${regex}`, { "event.action": "getQuerystring", "logrite.key": key, "logrite.default_": default_ });
        var qs = regex.exec(window.location.href);
        if (qs == null)
            return default_;
        else
            return qs[1];
    }

    (function (window) {
        // Check Location
        if (document.location.protocol === 'file:') {
            alert('The HTML5 History API (and thus History.js) do not work on files, please upload it to a server.');
        }

        // Establish Variables
        var History = window.History; // Note: We are using a capital H instead of a lower h

        // Bind to State Change
        History.Adapter.bind(window, 'statechange', function () { // Note: We are using statechange instead of popstate
            var state = History.getState(); // Note: We are using History.getState() instead of event.state

            if (state.data != null) {
                var data = state.data;

                var action = data.action;
                var view = data.view;
                var context = ko.contextFor(document.getElementById($(containerElement).attr("id")));

                if (view == "notifications") {
                    context.$root.loadNotificationsTabHandler();
                    $(containerElement + ' #smMainContent').dnnTabs({ selected: 1 });
                } else {
                    $(containerElement + ' #smMainContent').dnnTabs({ selected: 0 });
                    switch (action) {
                        case 'inbox':
                            context.$root.messages([]);
                            context.$root.showReplies(false);
                            context.$root.myinboxHandler();
                            break;
                        case 'sentbox':
                            context.$root.messages([]);
                            context.$root.showReplies(false);
                            context.$root.mysentboxHandler();
                            break;
                        case 'archivebox':
                            context.$root.messages([]);
                            context.$root.showReplies(false);
                            context.$root.myarchiveboxHandler();
                            break;
                        case 'thread':
                            context.$root.highlightThreads = false;
                            context.$root.conversationRead(true);
                            context.$root.messagethreads([]);
                            context.$root.sendThreadRequestHandler(data.conversationId, -1);
                            context.$root.showReplies(true);
                            break;
                        default:
                            //empty
                    }
                }
            }
        });
    })(window);

    function messageConversationView(data) {
		logger.error(`>>> Entering messageConversationView(data=${data})`, { "event.action": "messageConversationView", "logrite.data": data });
        var self = this;

        self.MessageID = data.MessageID;
		logger.debug(`messageConversationView(data=${data}): self.MessageID → ${self.MessageID}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.From → ${self.From}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.Subject → ${self.Subject}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.Body → ${self.Body}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.ConversationId → ${self.ConversationId}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.ReplyAllAllowed → ${self.ReplyAllAllowed}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.SenderUserID → ${self.SenderUserID}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.SenderDisplayName → ${self.SenderDisplayName}`, { "event.action": "messageConversationView", "logrite.data": data });
		logger.debug(`messageConversationView(data=${data}): self.RowNumber → ${self.RowNumber}`, { "event.action": "messageConversationView", "logrite.data": data });
        self.To = data.To;
		logger.debug(`messageConversationView(data=${data}): self.To → ${self.To}`, { "event.action": "messageConversationView", "logrite.data": data });
        self.From = data.From;
        self.Subject = data.Subject;
        self.Body = data.Body;
        self.ConversationId = data.ConversationId;
        self.ReplyAllAllowed = data.ReplyAllAllowed;
        self.SenderUserID = data.SenderUserID;
        self.SenderDisplayName = data.SenderDisplayName || data.From;
        self.RowNumber = data.RowNumber;
        self.AttachmentCount = ko.observable(data.AttachmentCount);
        self.NewThreadCount = ko.observable(data.NewThreadCount);
        self.ThreadCount = ko.observable(data.ThreadCount);
        self.MessageIDName = "messageSelect" + self.ConversationId;
		logger.debug(`messageConversationView(data=${data}): self.MessageIDName → ${self.MessageIDName}`, { "event.action": "messageConversationView", "logrite.data": data });
        self.SenderAvatar = profilePicHandler.replace("{0}", self.SenderUserID).replace("{1}", 64).replace("{2}", 64);
        self.SenderProfileUrl = data.SenderProfileUrl;
		logger.debug(`messageConversationView(data=${data}): self.SenderProfileUrl → ${self.SenderProfileUrl}`, { "event.action": "messageConversationView", "logrite.data": data });
        self.MessageAbstract = (self.Body.length < 50) ? self.Body : self.Body.substr(0, 50) + "...";
        self.messageSelected = ko.observable(false);
        self.CreatedOnDate = data.DisplayDate;
		logger.debug(`messageConversationView(data=${data}): self.CreatedOnDate → ${self.CreatedOnDate}`, { "event.action": "messageConversationView", "logrite.data": data });

        self.Read = ko.computed(function () {
            return self.NewThreadCount() === 0;
        });

        self.HasAttachments = ko.computed(function () {
		logger.warn(`<<< Exiting messageConversationView(data=${data})`, { "event.action": "messageConversationView", "logrite.data": data });
            return self.AttachmentCount() > 0;
        });
    }

    function messageThreadView(data) {
		logger.error(`>>> Entering messageThreadView(data=${data})`, { "event.action": "messageThreadView", "logrite.data": data });
        var self = this;

        self.MessageID = data.Conversation.MessageID;
		logger.debug(`messageThreadView(data=${data}): self.MessageID → ${self.MessageID}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.From → ${self.From}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.Subject → ${self.Subject}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.Body → ${self.Body}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.ConversationId → ${self.ConversationId}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.ReplyAllAllowed → ${self.ReplyAllAllowed}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.SenderUserID → ${self.SenderUserID}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.SenderDisplayName → ${self.SenderDisplayName}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.RowNumber → ${self.RowNumber}`, { "event.action": "messageThreadView", "logrite.data": data });
		logger.debug(`messageThreadView(data=${data}): self.AttachmentCount → ${self.AttachmentCount}`, { "event.action": "messageThreadView", "logrite.data": data });
        self.To = data.Conversation.To;
		logger.debug(`messageThreadView(data=${data}): self.To → ${self.To}`, { "event.action": "messageThreadView", "logrite.data": data });
        self.From = data.Conversation.From;
        self.Subject = data.Conversation.Subject;
        self.Body = data.Conversation.Body;
        self.ConversationId = data.Conversation.ConversationId;
        self.ReplyAllAllowed = data.Conversation.ReplyAllAllowed;
        self.SenderUserID = data.Conversation.SenderUserID;
        self.SenderDisplayName = data.Conversation.SenderDisplayName || data.Conversation.From;
        self.RowNumber = data.Conversation.RowNumber;
        self.AttachmentCount = data.Conversation.AttachmentCount;
        self.NewThreadCount = ko.observable(data.Conversation.NewThreadCount);
        self.Attachments = data.Attachments;
		logger.debug(`messageThreadView(data=${data}): self.Attachments → ${self.Attachments}`, { "event.action": "messageThreadView", "logrite.data": data });
        self.MessageIDName = "messageSelect" + self.ConversationId;
		logger.debug(`messageThreadView(data=${data}): self.MessageIDName → ${self.MessageIDName}`, { "event.action": "messageThreadView", "logrite.data": data });
        self.SenderAvatar = profilePicHandler.replace("{0}", self.SenderUserID).replace("{1}", 64).replace("{2}", 64);
        self.SenderProfileUrl = data.Conversation.SenderProfileUrl;
		logger.debug(`messageThreadView(data=${data}): self.SenderProfileUrl → ${self.SenderProfileUrl}`, { "event.action": "messageThreadView", "logrite.data": data });
        self.CreatedOnDate = data.Conversation.DisplayDate;
		logger.debug(`messageThreadView(data=${data}): self.CreatedOnDate → ${self.CreatedOnDate}`, { "event.action": "messageThreadView", "logrite.data": data });

        self.Read = ko.computed(function () {
		logger.warn(`<<< Exiting messageThreadView(data=${data})`, { "event.action": "messageThreadView", "logrite.data": data });
            return self.NewThreadCount() === 0;
        });
    }

    function notificationActionViewModel(data, notificationId) {
		logger.error(`>>> Entering notificationActionViewModel(data=${data},notificationId=${notificationId})`, { "event.action": "notificationActionViewModel", "logrite.data": data, "logrite.notificationId": notificationId });
        var self = this;

        self.NotificationId = notificationId;
		logger.debug(`notificationActionViewModel(data=${data},notificationId=${notificationId}): self.NotificationId → ${self.NotificationId}`, { "event.action": "notificationActionViewModel", "logrite.data": data, "logrite.notificationId": notificationId });
		logger.debug(`notificationActionViewModel(data=${data},notificationId=${notificationId}): self.Description → ${self.Description}`, { "event.action": "notificationActionViewModel", "logrite.data": data, "logrite.notificationId": notificationId });
		logger.debug(`notificationActionViewModel(data=${data},notificationId=${notificationId}): self.Confirm → ${self.Confirm}`, { "event.action": "notificationActionViewModel", "logrite.data": data, "logrite.notificationId": notificationId });
		logger.debug(`notificationActionViewModel(data=${data},notificationId=${notificationId}): self.APICall → ${self.APICall}`, { "event.action": "notificationActionViewModel", "logrite.data": data, "logrite.notificationId": notificationId });
        self.Name = data.Name;
		logger.debug(`notificationActionViewModel(data=${data},notificationId=${notificationId}): self.Name → ${self.Name}`, { "event.action": "notificationActionViewModel", "logrite.data": data, "logrite.notificationId": notificationId });
        self.Description = data.Description;
        self.Confirm = data.Confirm;
        self.APICall = data.APICall;
    }

    function notificationViewModel(data) {
		logger.error(`>>> Entering notificationViewModel(data=${data})`, { "event.action": "notificationViewModel", "logrite.data": data });
        var self = this;

        self.NotificationId = data.NotificationId;
		logger.debug(`notificationViewModel(data=${data}): self.NotificationId → ${self.NotificationId}`, { "event.action": "notificationViewModel", "logrite.data": data });
		logger.debug(`notificationViewModel(data=${data}): self.From → ${self.From}`, { "event.action": "notificationViewModel", "logrite.data": data });
		logger.debug(`notificationViewModel(data=${data}): self.Body → ${self.Body}`, { "event.action": "notificationViewModel", "logrite.data": data });
		logger.debug(`notificationViewModel(data=${data}): self.SenderAvatar → ${self.SenderAvatar}`, { "event.action": "notificationViewModel", "logrite.data": data });
		logger.debug(`notificationViewModel(data=${data}): self.SenderProfileUrl → ${self.SenderProfileUrl}`, { "event.action": "notificationViewModel", "logrite.data": data });
		logger.debug(`notificationViewModel(data=${data}): self.DisplayDate → ${self.DisplayDate}`, { "event.action": "notificationViewModel", "logrite.data": data });
		logger.debug(`notificationViewModel(data=${data}): self.SenderDisplayName → ${self.SenderDisplayName}`, { "event.action": "notificationViewModel", "logrite.data": data });
        self.Subject = data.Subject;
		logger.debug(`notificationViewModel(data=${data}): self.Subject → ${self.Subject}`, { "event.action": "notificationViewModel", "logrite.data": data });
        self.From = data.From;
        self.Body = data.Body;
        self.SenderAvatar = data.SenderAvatar;
        self.SenderProfileUrl = data.SenderProfileUrl;
        self.DisplayDate = data.DisplayDate;
        self.SenderDisplayName = data.SenderDisplayName;
        self.Actions = $.map(data.Actions, function (action) { return new notificationActionViewModel(action, data.NotificationId); });
		logger.warn(`<<< Exiting notificationViewModel(data=${data})`, { "event.action": "notificationViewModel", "logrite.data": data });
    }

    function coreMessagingViewModel() {
		logger.error('>>> Entering coreMessagingViewModel()', { "event.action": "coreMessagingViewModel" });
        var self = this;

		logger.error(`coreMessagingViewModel(): self → ${self}`, { "event.action": "coreMessagingViewModel" });
	    self.disablePrivateMessage = ko.observable(settings.disablePrivateMessage);
        self.messages = ko.observableArray([]);
        self.notifications = ko.observableArray([]);

        self.notificationFadeTime = ko.observable(500);

        // Number displayed in Notifications tab
        self.TotalNotifications = ko.observable(0);

        // Used in box pagination
        self.TotalConversations = ko.observable(0);

        // Number displayed in Messages tab
        self.TotalNewThreads = ko.observable(0);

        // Used in Thread View to know if there are more threads to display
        self.TotalThreads = ko.observable(0);

        // Used in Thread View to display archive/unarchive links
        self.TotalArchivedThreads = ko.observable(0);

        self.messagethreads = ko.observableArray([]);

        //use an observable value to show/hide the boxes
        self.showInbox = ko.observable(true);
        self.showSentbox = ko.observable(false);
        self.showArchivebox = ko.observable(false);

        //use an observable value to show/hide the replies list
        self.showReplies = ko.observable(false);
        
        //use an observable value to decide if a reply is possible
        self.replyHasRecipients = ko.observable(true);

        self.threadSubject = ko.observable('');
        self.threadTo = ko.observable('');
        self.loadingData = ko.observable(false);
        self.isReplySelected = ko.observable(false);
        self.highlightThreads = false;
		logger.debug(`coreMessagingViewModel(): self.highlightThreads → ${self.highlightThreads}`, { "event.action": "coreMessagingViewModel" });
        self.conversationRead = ko.observable();

        self.selectMenuOn = ko.observable(false);
        self.actionsMenuOn = ko.observable(false);

        self.hasElementsSelected = ko.computed(function () {
            var selectedElement = ko.utils.arrayFirst(self.messages(), function (message) {
                return message.messageSelected() === true;
            });

            return selectedElement != undefined;
        });

        self.toogleSelectMenu = function () {
			logger.error('>>> Entering toogleSelectMenu()', { "event.action": "toogleSelectMenu" });
            self.selectMenuOn(!self.selectMenuOn());
			logger.warn('<<< Exiting toogleSelectMenu()', { "event.action": "toogleSelectMenu" });
        };

        self.toogleActionsMenu = function () {
			logger.error('>>> Entering toogleActionsMenu()', { "event.action": "toogleActionsMenu" });
            if (!self.hasElementsSelected()) {
                self.actionsMenuOn(false);
            } else {
                self.actionsMenuOn(!self.actionsMenuOn());
				logger.warn('<<< Exiting toogleActionsMenu()', { "event.action": "toogleActionsMenu" });
            }
        };

        $('body').on('click.coremessaging', function (event) {
            if (!$(event.target).closest('#SelectMenu').length) {
                self.selectMenuOn(false);
            };

            if (!$(event.target).closest('#ActionsMenu').length) {
                self.actionsMenuOn(false);
            };
        });

        self.getPageNumbers = ko.computed(function () {
            if (self.TotalConversations() === 0) return '0-0';
            return '1-' + self.messages().length;
        });

        self.getNotificationPageNumbers = ko.computed(function () {
            if (self.TotalNotifications() === 0) return '0-0';
            return '1-' + self.notifications().length;
        });

        self.backToMessages = function () {
			logger.error('>>> Entering backToMessages()', { "event.action": "backToMessages" });
            self.messages([]);
            self.showReplies(false);
            self.reloadBoxes();
			logger.warn('<<< Exiting backToMessages()', { "event.action": "backToMessages" });
        };

        self.loadMessagesTab = function () {
			logger.error('>>> Entering loadMessagesTab()', { "event.action": "loadMessagesTab" });
            self.messages([]);
            self.showReplies(false);
            self.myinbox();
			logger.warn('<<< Exiting loadMessagesTab()', { "event.action": "loadMessagesTab" });
        };

        self.reloadBoxes = function () {
			logger.error('>>> Entering reloadBoxes()', { "event.action": "reloadBoxes" });
            self.loadingData(true);
            if (self.showInbox()) {
                self.myinbox();
            } else if (self.showSentbox()) {
                self.mysentbox();
            } else if (self.showArchivebox()) {
                self.myarchivebox();
				logger.warn('<<< Exiting reloadBoxes()', { "event.action": "reloadBoxes" });
            }
        };

        self.fetch_unix_timestamp = function () { return (new Date().getTime().toString().substring(0, 10)); };
		logger.error('>>> Entering fetch_unix_timestamp()', { "event.action": "fetch_unix_timestamp" });

		logger.warn('<<< Exiting fetch_unix_timestamp()', { "event.action": "fetch_unix_timestamp" });
        self.sendThreadRequest = function (message) {
			logger.error(`>>> Entering sendThreadRequest(message=${message})`, { "event.action": "sendThreadRequest", "logrite.message": message });
            History.pushState({ view: 'messages', action: 'thread', conversationId: message.ConversationId }, "", "?view=messages&action=thread&t=" + self.fetch_unix_timestamp());
            setDocumentTitle(settings.messageTitleText);
			logger.warn(`<<< Exiting sendThreadRequest(message=${message})`, { "event.action": "sendThreadRequest", "logrite.message": message });
        };

        self.checkReplyHasRecipients = function (conversationId) {
			logger.error(`>>> Entering checkReplyHasRecipients(conversationId=${conversationId})`, { "event.action": "checkReplyHasRecipients", "logrite.conversationId": conversationId });
            var returnValue = 0; // If the call fails, just return 0.

            $.ajax({
                type: "GET",
                beforeSend: serviceFramework.setModuleHeaders,
                url: baseServicepath + "CheckReplyHasRecipients",
                async: false,
                cache: false,
                data: { conversationId: conversationId }
            }).done(function (data) {
                if ($.type(data) === "number") {
                    returnValue = data;
                }
            });

            if (returnValue == 0) {
                self.replyHasRecipients(false);
                displayMessage("#dnnCoreMessaging", settings.replyHasNoRecipientsText, "dnnFormWarning");
            } else {
                self.replyHasRecipients(true);
				logger.warn(`<<< Exiting checkReplyHasRecipients(conversationId=${conversationId})`, { "event.action": "checkReplyHasRecipients", "logrite.conversationId": conversationId });
            }
        };
        
        self.sendThreadRequestHandler = function (conversationId, afterMessageId) {
            self.checkReplyHasRecipients(conversationId);
            $.ajax({
                type: "GET",
                url: baseServicepath + "Thread",
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: conversationId, afterMessageId: afterMessageId, numberOfRecords: repliesPageSize },
                cache: false
            }).done(function (messagethreadsView) {
                if (typeof messagethreadsView !== "undefined" &&
                    messagethreadsView != null &&
                    typeof messagethreadsView.Conversations !== "undefined") {
                    var mappedMessages = $.map(messagethreadsView.Conversations, function (item) {
                        return new messageThreadView(item);
                    });

                    if (mappedMessages.length > 0) {
                        for (var i = 0; i < mappedMessages.length; i++) {
                            self.messagethreads.unshift(mappedMessages[i]);
                        }
                    }
                    self.threadSubject(self.messagethreads()[0].Subject);
                    self.threadTo(self.messagethreads()[0].To);
                    self.highlightThreads = true;

                    //update totals
                    if (typeof messagethreadsView.TotalThreads !== "undefined" && $.type(messagethreadsView.TotalThreads) === "number") {
                        self.TotalThreads(messagethreadsView.TotalThreads);
                    }

                    if (typeof messagethreadsView.TotalNewThreads !== "undefined" && $.type(messagethreadsView.TotalNewThreads) === "number") {
                        self.TotalNewThreads(messagethreadsView.TotalNewThreads);
                    }

                    if (typeof messagethreadsView.TotalArchivedThreads !== "undefined" && $.type(messagethreadsView.TotalArchivedThreads) === "number") {
                        self.TotalArchivedThreads(messagethreadsView.TotalArchivedThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn(`<<< Exiting sendThreadRequestHandler(conversationId=${conversationId},afterMessageId=${afterMessageId})`, { "event.action": "sendThreadRequestHandler", "logrite.conversationId": conversationId, "logrite.afterMessageId": afterMessageId });
        };

        self.getReplies = function (message) {
			logger.error(`>>> Entering getReplies(message=${message})`, { "event.action": "getReplies", "logrite.message": message });
            self.highlightThreads = false;
            if (message.Read() !== true) {
                self.markAsRead(message);
            }
            self.conversationRead(true);
            self.messagethreads([]);
            self.sendThreadRequest(message);
            self.showReplies(true);
			logger.warn(`<<< Exiting getReplies(message=${message})`, { "event.action": "getReplies", "logrite.message": message });
        };

        self.getRepliesAndReply = function (message) {
			logger.error(`>>> Entering getRepliesAndReply(message=${message})`, { "event.action": "getRepliesAndReply", "logrite.message": message });
            self.getReplies(message);
            self.setReplySelected();
			logger.warn(`<<< Exiting getRepliesAndReply(message=${message})`, { "event.action": "getRepliesAndReply", "logrite.message": message });
        };

        self.showPreviousReplies = function () {
			logger.error('>>> Entering showPreviousReplies()', { "event.action": "showPreviousReplies" });
            if (self.TotalThreads() > self.messagethreads().length) {
                self.sendThreadRequestHandler(self.messagethreads()[0].ConversationId, self.messagethreads()[0].MessageID);
				logger.warn('<<< Exiting showPreviousReplies()', { "event.action": "showPreviousReplies" });
            }
        };

        self.showPreviousRepliesVisible = ko.computed(function () {
            return self.messagethreads().length < self.TotalThreads();
        });

        self.updateThreadCount = function (messageconversationView) {
			logger.error(`>>> Entering updateThreadCount(messageconversationView=${messageconversationView})`, { "event.action": "updateThreadCount", "logrite.messageconversationView": messageconversationView });
            var previousNewThreads = self.TotalNewThreads();

			logger.error(`updateThreadCount(messageconversationView=${messageconversationView}): previousNewThreads → ${previousNewThreads}`, { "event.action": "updateThreadCount", "logrite.messageconversationView": messageconversationView });
            if (messageconversationView.Read() === true) {
                messageconversationView.NewThreadCount(0);
                self.TotalNewThreads(previousNewThreads + messageconversationView.NewThreadCount());
            } else {
                self.TotalNewThreads(previousNewThreads - messageconversationView.NewThreadCount());
                messageconversationView.NewThreadCount(1);
				logger.warn(`<<< Exiting updateThreadCount(messageconversationView=${messageconversationView})`, { "event.action": "updateThreadCount", "logrite.messageconversationView": messageconversationView });
            }
        };

        self.changeState = function (messageconversationView, url) {
            $.ajax({
                type: "POST",
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: messageconversationView.ConversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    if (messageconversationView.ThreadCount != undefined) {
                        self.updateThreadCount(messageconversationView);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn(`<<< Exiting changeState(messageconversationView=${messageconversationView},url=${url})`, { "event.action": "changeState", "logrite.messageconversationView": messageconversationView, "logrite.url": url });
        };

        self.markAsRead = function (messageconversationView) {
			logger.error(`>>> Entering markAsRead(messageconversationView=${messageconversationView})`, { "event.action": "markAsRead", "logrite.messageconversationView": messageconversationView });
            messageconversationView.NewThreadCount(0);
            self.changeState(messageconversationView, baseServicepath + 'MarkRead');
			logger.warn(`<<< Exiting markAsRead(messageconversationView=${messageconversationView})`, { "event.action": "markAsRead", "logrite.messageconversationView": messageconversationView });
        };

        self.markAsUnread = function (messageconversationView) {
			logger.error(`>>> Entering markAsUnread(messageconversationView=${messageconversationView})`, { "event.action": "markAsUnread", "logrite.messageconversationView": messageconversationView });
            messageconversationView.NewThreadCount(1);
            self.changeState(messageconversationView, baseServicepath + 'MarkUnRead');
			logger.warn(`<<< Exiting markAsUnread(messageconversationView=${messageconversationView})`, { "event.action": "markAsUnread", "logrite.messageconversationView": messageconversationView });
        };

        self.toggleState = function (messageconversationView) {
			logger.error(`>>> Entering toggleState(messageconversationView=${messageconversationView})`, { "event.action": "toggleState", "logrite.messageconversationView": messageconversationView });
            if (messageconversationView.Read() === true) {
                self.markAsUnread(messageconversationView);
            } else {
                self.markAsRead(messageconversationView);
				logger.warn(`<<< Exiting toggleState(messageconversationView=${messageconversationView})`, { "event.action": "toggleState", "logrite.messageconversationView": messageconversationView });
            }
        };

        self.toggleConversationState = function () {
			logger.error('>>> Entering toggleConversationState()', { "event.action": "toggleConversationState" });
            var messageconversationView = self.messagethreads()[0];
            self.toggleState(messageconversationView);
			logger.error(`toggleConversationState(): messageconversationView → ${messageconversationView}`, { "event.action": "toggleConversationState" });

            self.conversationRead(!self.conversationRead());

            if (self.conversationRead() === true) {
                $('.successMsg').text(settings.conversationSetAsReadText);
            } else {
                $('.successMsg').text(settings.conversationSetAsUnreadText);
            }
            $('.successMsg').show().fadeOut(3000, 'easeInExpo');
			logger.warn('<<< Exiting toggleConversationState()', { "event.action": "toggleConversationState" });
        };

        self.loadMore = function (data, event) {
			logger.error(`>>> Entering loadMore(data=${data},event=${event})`, { "event.action": "loadMore", "logrite.data": data, "logrite.event": event });
            $(event.target).html('<img src="images/dnnanim.gif" style="vertical-align:middle" /> ' + settings.loadingText);

            var afterMessageId = self.messages().length == 0 ? -1 : self.messages()[self.messages().length - 1].MessageID;
			logger.error(`loadMore(data=${data},event=${event}): afterMessageId → ${afterMessageId}`, { "event.action": "loadMore", "logrite.data": data, "logrite.event": event });
            var url = inboxpath;
            if (self.showSentbox()) url = sentboxpath;
            else if (self.showArchivebox()) url = archivedboxpath;

            $.ajax({
                type: "GET",
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterMessageId: afterMessageId, numberOfRecords: pageSize },
                cache: false
            }).done(function (messageboxView) {
                if (typeof messageboxView !== "undefined" &&
                    messageboxView != null &&
                    typeof messageboxView.Conversations !== "undefined") {
                    var mappedMessages = $.map(messageboxView.Conversations, function (item) { return new messageConversationView(item); });
                    for (var i = 0; i < mappedMessages.length; i++) {
                        self.messages.push(mappedMessages[i]);
                    }

                    if (typeof messageboxView.TotalConversations !== "undefined" && $.type(messageboxView.TotalConversations) === "number") {
                        self.TotalConversations(messageboxView.TotalConversations);
                    }
                    if (typeof messageboxView.TotalNewThreads !== "undefined" && $.type(messageboxView.TotalNewThreads) === "number") {
                        self.TotalNewThreads(messageboxView.TotalNewThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            }).always(function () {
                $(event.target).html(settings.loadMoreText);
            });
			logger.warn(`<<< Exiting loadMore(data=${data},event=${event})`, { "event.action": "loadMore", "logrite.data": data, "logrite.event": event });
        };

        self.moveToArchive = function (message) {
			logger.error(`>>> Entering moveToArchive(message=${message})`, { "event.action": "moveToArchive", "logrite.message": message });
            $.ajax({
                type: "POST",
                url: baseServicepath + 'MarkArchived',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: message.ConversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    if (message.ThreadCount != undefined) {
                        self.TotalNewThreads(self.TotalNewThreads() - message.NewThreadCount());
                    }

                    self.messages.remove(message);
                    self.TotalConversations(self.TotalConversations() - 1);
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn(`<<< Exiting moveToArchive(message=${message})`, { "event.action": "moveToArchive", "logrite.message": message });
        };

        self.unarchive = function (message) {
			logger.error(`>>> Entering unarchive(message=${message})`, { "event.action": "unarchive", "logrite.message": message });
            $.ajax({
                type: "POST",
                url: baseServicepath + 'MarkUnArchived',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: message.ConversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    if (message.ThreadCount != undefined) {
                        self.TotalNewThreads(self.TotalNewThreads() - message.NewThreadCount());
                    }

                    self.messages.remove(message);
                    self.TotalConversations(self.TotalConversations() - 1);
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn(`<<< Exiting unarchive(message=${message})`, { "event.action": "unarchive", "logrite.message": message });
        };

        self.delete = function (message) {
			logger.error(`>>> Entering delete(message=${message})`, { "event.action": "delete", "logrite.message": message });
            var opts = {
                callbackTrue: function() {
					logger.error('>>> Entering callbackTrue()', { "event.action": "callbackTrue" });
                    $.ajax({
                        type: "POST",
                        url: baseServicepath + 'DeleteUserFromConversation',
                        beforeSend: serviceFramework.setModuleHeaders,
                        data: { conversationId: message.ConversationId }
                    }).done(function(data) {
                        if (data.Result === "success") {
                            if (message.ThreadCount != undefined) {
                                self.TotalNewThreads(self.TotalNewThreads() - message.NewThreadCount());
                            }

                            self.messages.remove(message);
                            self.TotalConversations(self.TotalConversations() - 1);
                        } else {
                            displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                        }
                    }).fail(function(xhr, status) {
                        displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
                    });
					logger.warn('<<< Exiting callbackTrue()', { "event.action": "callbackTrue" });
                },
                text: settings.text,
                yesText: settings.yesText,
                noText: settings.noText,
                title: settings.title
            };

            $.dnnConfirm(opts);
			logger.warn(`<<< Exiting delete(message=${message})`, { "event.action": "delete", "logrite.message": message });
        };

        self.moveSelectedToArchive = function () {
			logger.error('>>> Entering moveSelectedToArchive()', { "event.action": "moveSelectedToArchive" });
            $.each(self.messages(), function () {
                if (this.messageSelected()) self.moveToArchive(this);
            });
			logger.warn('<<< Exiting moveSelectedToArchive()', { "event.action": "moveSelectedToArchive" });
        };

        self.moveSelectedToRead = function () {
			logger.error('>>> Entering moveSelectedToRead()', { "event.action": "moveSelectedToRead" });
            $.each(self.messages(), function () {
                if (this.messageSelected()) self.markAsRead(this);
            });
			logger.warn('<<< Exiting moveSelectedToRead()', { "event.action": "moveSelectedToRead" });
        };

        self.moveSelectedToUnread = function () {
			logger.error('>>> Entering moveSelectedToUnread()', { "event.action": "moveSelectedToUnread" });
            $.each(self.messages(), function () {
                if (this.messageSelected()) self.markAsUnread(this);
            });
			logger.warn('<<< Exiting moveSelectedToUnread()', { "event.action": "moveSelectedToUnread" });
        };

        self.toggleArchiveConversation = function () {
			logger.error('>>> Entering toggleArchiveConversation()', { "event.action": "toggleArchiveConversation" });
            var conversationId = self.messagethreads()[0].ConversationId;
            var url;
			logger.error(`toggleArchiveConversation(): conversationId → ${conversationId}`, { "event.action": "toggleArchiveConversation" });

            if (self.TotalArchivedThreads() === 0) {
                url = baseServicepath + 'MarkArchived';
                $('.successMsg').text(settings.conversationArchivedText);
            } else {
                url = baseServicepath + 'MarkUnArchived';
                $('.successMsg').text(settings.conversationUnarchivedText);
				logger.error(`toggleArchiveConversation(): url → ${url}`, { "event.action": "toggleArchiveConversation" });
            }

            $.ajax({
                type: "POST",
                url: url,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: conversationId }
            }).done(function (data) {
                if (data.Result === "success") {
                    $('.successMsg').show().fadeOut(3000, 'easeInExpo');

                    if (self.TotalArchivedThreads() === 0) {
                        self.TotalArchivedThreads(self.TotalThreads);
                    } else {
                        self.TotalArchivedThreads(0);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn('<<< Exiting toggleArchiveConversation()', { "event.action": "toggleArchiveConversation" });
        };

        self.changeBoxes = function (inbox, sentbox, archivebox) {
			logger.error(`>>> Entering changeBoxes(inbox=${inbox},sentbox=${sentbox},archivebox=${archivebox})`, { "event.action": "changeBoxes", "logrite.inbox": inbox, "logrite.sentbox": sentbox, "logrite.archivebox": archivebox });
            self.showInbox(inbox);
            self.showSentbox(sentbox);
            self.showArchivebox(archivebox);
			logger.warn(`<<< Exiting changeBoxes(inbox=${inbox},sentbox=${sentbox},archivebox=${archivebox})`, { "event.action": "changeBoxes", "logrite.inbox": inbox, "logrite.sentbox": sentbox, "logrite.archivebox": archivebox });
        };

        self.getTotalNotifications = function () {
			logger.error('>>> Entering getTotalNotifications()', { "event.action": "getTotalNotifications" });
            var returnValue = 0; // If the call fails, just return 0.

            $.ajax({
                type: "GET",
                beforeSend: serviceFramework.setModuleHeaders,
                url: countnotificationspath,
                async: false,
                cache: false
            }).done(function (data) {
                if ($.type(data) === "number") {
                    returnValue = data;
                }
            });

			logger.warn('<<< Exiting getTotalNotifications()', { "event.action": "getTotalNotifications" });
            return returnValue;
        };

        self.loadNotificationsTab = function () {
			logger.error('>>> Entering loadNotificationsTab()', { "event.action": "loadNotificationsTab" });
            self.notificationFadeTime(0);
            History.pushState({ view: 'notifications', action: 'notifications' }, "", "?view=notifications&action=notifications&t=" + self.fetch_unix_timestamp());
            setDocumentTitle(settings.notificationTitleText);
			logger.warn('<<< Exiting loadNotificationsTab()', { "event.action": "loadNotificationsTab" });
        };

        self.loadNotificationsTabHandler = function () {
			logger.error('>>> Entering loadNotificationsTabHandler()', { "event.action": "loadNotificationsTabHandler" });
            self.loadingData(true);
            
            $.ajax({
                type: "GET",
                url: notificationspath,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterNotificationId: -1, numberOfRecords: notificationsPageSize },
                cache: false
            }).done(function (notificationsViewModel) {
                if (typeof notificationsViewModel !== "undefined" &&
                    notificationsViewModel != null &&
                    typeof notificationsViewModel.Notifications !== "undefined") {
                    var mappedNotifications = $.map(notificationsViewModel.Notifications, function (notification) { return new notificationViewModel(notification); });
                    self.notifications(mappedNotifications);

                    if (typeof notificationsViewModel.TotalNotifications !== "undefined" && $.type(notificationsViewModel.TotalNotifications) === "number") {
                        self.TotalNotifications(notificationsViewModel.TotalNotifications);
                    }
                } else {
                    displayMessage("#dnnCoreNotification", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreNotification", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            }).always(function () {
                self.loadingData(false);
            });
			logger.warn('<<< Exiting loadNotificationsTabHandler()', { "event.action": "loadNotificationsTabHandler" });
        };

        self.loadMoreNotifications = function () {
			logger.error('>>> Entering loadMoreNotifications()', { "event.action": "loadMoreNotifications" });
            var afterNotificationId = self.notifications().length == 0 ? -1 : self.notifications()[self.notifications().length - 1].NotificationId;

            $.ajax({
                type: "GET",
                url: notificationspath,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterNotificationId: afterNotificationId, numberOfRecords: notificationsPageSize },
                cache: false
            }).done(function (notificationsViewModel) {
                if (typeof notificationsViewModel !== "undefined" &&
                    notificationsViewModel != null &&
                    typeof notificationsViewModel.Notifications !== "undefined") {
                    var mappedNotifications = $.map(notificationsViewModel.Notifications, function (notification) { return new notificationViewModel(notification); });
					logger.error(`loadMoreNotifications(): afterNotificationId → ${afterNotificationId}`, { "event.action": "loadMoreNotifications" });
                    for (var i = 0; i < mappedNotifications.length; i++) {
                        self.notifications.push(mappedNotifications[i]);
                    }

                    if (typeof notificationsViewModel.TotalNotifications !== "undefined" && $.type(notificationsViewModel.TotalNotifications) === "number") {
                        self.TotalNotifications(notificationsViewModel.TotalNotifications);
                    }
                } else {
                    displayMessage("#dnnCoreNotification", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function () {
                displayMessage("#dnnCoreNotification", settings.serverErrorWithDescriptionText, "dnnFormWarning");
            });
			logger.warn('<<< Exiting loadMoreNotifications()', { "event.action": "loadMoreNotifications" });
        };

        self.loadMoreVisible = ko.computed(function () {
            return self.messages().length < self.TotalConversations();
        });

        self.loadMoreNotificationsVisible = ko.computed(function () {
            return self.notifications().length < self.TotalNotifications();
        });

        self.isLastNotificationAction = function (notification, action) {
			logger.error(`>>> Entering isLastNotificationAction(notification=${notification},action=${action})`, { "event.action": "isLastNotificationAction", "logrite.notification": notification, "logrite.action": action });
			logger.warn(`<<< Exiting isLastNotificationAction(notification=${notification},action=${action})`, { "event.action": "isLastNotificationAction", "logrite.notification": notification, "logrite.action": action });
            return action === notification.Actions[notification.Actions.length - 1];
        };

        self.performNotificationAction = function (action) {
			logger.error(`>>> Entering performNotificationAction(action=${action})`, { "event.action": "performNotificationAction", "logrite.action": action });
            if (action.Confirm.length > 0) {
                var confirmDialog = $("<div class='dnnDialog'></div>").html(action.Confirm).dialog({
                    autoOpen: false,
                    resizable: false,
                    modal: true,
                    title: settings.notificationConfirmTitleText,
                    classes: {
                        'ui-dialog': 'dnnFormPopup dnnClear'
                    },
                    open: function () {
						logger.error('>>> Entering open()', { "event.action": "open" });
                        $('.ui-dialog-buttonpane').find('button:contains("' + settings.notificationConfirmNoText + '")').addClass('dnnConfirmCancel');
						logger.warn('<<< Exiting open()', { "event.action": "open" });
                    },
                    buttons: [
                        {
                            text: settings.notificationConfirmYesText,
                            click: function () {
								logger.error('>>> Entering click()', { "event.action": "click" });
                                $(this).dialog("close");
                                self.apiCallRequest(action);
								logger.warn('<<< Exiting click()', { "event.action": "click" });
                            }
                        },
                        {
                            text: settings.notificationConfirmNoText,
                            click: function () { $(this).dialog("close"); }
                        }
                    ]
                });

				logger.error(`performNotificationAction(action=${action}): confirmDialog → ${confirmDialog}`, { "event.action": "performNotificationAction", "logrite.action": action });
                if (confirmDialog.is(':visible')) {
                    confirmDialog.dialog("close");
					logger.warn(`<<< Exiting performNotificationAction(action=${action})`, { "event.action": "performNotificationAction", "logrite.action": action });
                    return true;
                }

                confirmDialog.dialog('open');
            } else {
                self.apiCallRequest(action);
            }
        };

        self.dismissAllNotifications = function () {
			logger.error('>>> Entering dismissAllNotifications()', { "event.action": "dismissAllNotifications" });
            var opts = {
                callbackTrue: function () {
                    $.ajax({
                        type: "POST",
                        url: baseServicepath + 'DismissAllNotifications',
                        beforeSend: serviceFramework.setModuleHeaders
                    }).done(function (data) {
                        if (data.Result === "success") {
                            displayMessage("#dnnCoreNotification", settings.actionPerformedText, "dnnFormSuccess");
                            location.href = window.location.href;
                        } else {
                            displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                        }
                    }).fail(function (xhr, status) {
                        displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
                    });
                },
                text: String.format(settings.dismissAllConfirmText, self.TotalNotifications()),
                yesText: settings.yesText,
                noText: settings.noText,
                title: settings.title,
                buttonYesClass: 'dnnSecondaryAction',
                buttonNoClass: 'dnnPrimaryAction',
            };

            $.dnnConfirm(opts);
			logger.warn('<<< Exiting dismissAllNotifications()', { "event.action": "dismissAllNotifications" });
        };

        self.apiCallRequest = function (action) {
			logger.error(`>>> Entering apiCallRequest(action=${action})`, { "event.action": "apiCallRequest", "logrite.action": action });
            $.ajax({
                type: "POST",
                url: dnn.getVar("sf_siteRoot", "/") + action.APICall,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { NotificationId: action.NotificationId }
            }).done(function (data) {
                if (data.Result === "success") {

                    var notificationToRemove = ko.utils.arrayFirst(self.notifications(), function (notification) {
                        return notification.NotificationId === action.NotificationId;
                    });

                    self.notifications.remove(notificationToRemove);

                    //remove notification from the userControl bar

                    var notifications = $("#dnn_dnnUser_notificationLink").children("span");

                    if (notifications) {
                        var totalNotificationsTxt = notifications.text();

                        if (totalNotificationsTxt != "") {
                            var totalNotifications = parseInt(totalNotificationsTxt);
                            totalNotifications--;

                            if (totalNotifications > 0) {
                                notifications.text(totalNotifications);
                            }
                            else {
                                notifications.text("");
                            }
                        }
                    }


                    self.TotalNotifications(self.TotalNotifications() - 1);
                    displayMessage("#dnnCoreNotification", settings.actionPerformedText, "dnnFormSuccess");
                    
                    if (data.Link) {
                        location.href = data.Link;
                    }
                }
                else {
                    if (typeof data.Message !== "undefined" && data.Message != null && data.Message !== '') {
                        displayMessage("#dnnCoreNotification", data.Message, "dnnFormWarning");
                    } else {
                        displayMessage("#dnnCoreNotification", settings.actionNotPerformedText, "dnnFormWarning");
                    }
                }
            }).fail(function (data) {
                var response = $.parseJSON(data.responseText);
                displayMessage("#dnnCoreNotification", response.Message, "dnnFormWarning");
            });
			logger.warn(`<<< Exiting apiCallRequest(action=${action})`, { "event.action": "apiCallRequest", "logrite.action": action });
        };

        self.hideNotification = function (elem) {
			logger.error(`>>> Entering hideNotification(elem=${elem})`, { "event.action": "hideNotification", "logrite.elem": elem });
            if (elem.nodeType === 1) {
                $(elem).fadeOut(self.notificationFadeTime(), function () { $(elem).remove(); });
                self.notificationFadeTime(500);
				logger.warn(`<<< Exiting hideNotification(elem=${elem})`, { "event.action": "hideNotification", "logrite.elem": elem });
            }
        };

        self.loadBox = function (boxPath) {
			logger.error(`>>> Entering loadBox(boxPath=${boxPath})`, { "event.action": "loadBox", "logrite.boxPath": boxPath });
            self.loadingData(true);

            $.ajax({
                type: "GET",
                url: boxPath,
                beforeSend: serviceFramework.setModuleHeaders,
                data: { afterMessageId: -1, numberOfRecords: pageSize },
                cache: false
            }).done(function (messageboxView) {
                if (typeof messageboxView !== "undefined" &&
                    messageboxView != null &&
                    typeof messageboxView.Conversations !== "undefined") {
                    var mappedMessages = $.map(messageboxView.Conversations, function (item) { return new messageConversationView(item); });
                    self.messages(mappedMessages);

                    if (typeof messageboxView.TotalConversations !== "undefined" && $.type(messageboxView.TotalConversations) === "number") {
                        self.TotalConversations(messageboxView.TotalConversations);
                    }
                    if (typeof messageboxView.TotalNewThreads !== "undefined" && $.type(messageboxView.TotalNewThreads) === "number") {
                        self.TotalNewThreads(messageboxView.TotalNewThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescription + status, "dnnFormWarning");
            }).always(function () {
                self.loadingData(false);
            });
			logger.warn(`<<< Exiting loadBox(boxPath=${boxPath})`, { "event.action": "loadBox", "logrite.boxPath": boxPath });
        };

        self.myinbox = function () {
			logger.error('>>> Entering myinbox()', { "event.action": "myinbox" });
            History.pushState({ view: 'messages', action: 'inbox' }, "", "?view=messages&action=inbox&t=" + self.fetch_unix_timestamp());
            setDocumentTitle(settings.messageTitleText);
			logger.warn('<<< Exiting myinbox()', { "event.action": "myinbox" });
        };

        self.myinboxHandler = function () {
			logger.error('>>> Entering myinboxHandler()', { "event.action": "myinboxHandler" });
            self.changeBoxes(true, false, false);
            self.loadingData(true);
            self.loadBox(inboxpath);
			logger.warn('<<< Exiting myinboxHandler()', { "event.action": "myinboxHandler" });
        };

        self.mysentbox = function () {
			logger.error('>>> Entering mysentbox()', { "event.action": "mysentbox" });
            History.pushState({ view: 'messages', action: 'sentbox' }, "", "?view=messages&action=sentbox&t=" + self.fetch_unix_timestamp());
            setDocumentTitle(settings.sentTitleText);
			logger.warn('<<< Exiting mysentbox()', { "event.action": "mysentbox" });
        };

        self.mysentboxHandler = function () {
			logger.error('>>> Entering mysentboxHandler()', { "event.action": "mysentboxHandler" });
            self.changeBoxes(false, true, false);
            self.loadingData(true);
            self.loadBox(sentboxpath);
			logger.warn('<<< Exiting mysentboxHandler()', { "event.action": "mysentboxHandler" });
        };

        self.myarchivebox = function () {
			logger.error('>>> Entering myarchivebox()', { "event.action": "myarchivebox" });
            History.pushState({ view: 'messages', action: 'archivebox' }, "", "?view=messages&action=archivebox&t=" + self.fetch_unix_timestamp());
            setDocumentTitle(settings.archiveTitleText);
			logger.warn('<<< Exiting myarchivebox()', { "event.action": "myarchivebox" });
        };

        self.myarchiveboxHandler = function () {
			logger.error('>>> Entering myarchiveboxHandler()', { "event.action": "myarchiveboxHandler" });
            self.changeBoxes(false, false, true);
            self.loadingData(true);
            self.loadBox(archivedboxpath);
			logger.warn('<<< Exiting myarchiveboxHandler()', { "event.action": "myarchiveboxHandler" });
        };

        self.reply = function () {
			logger.error('>>> Entering reply()', { "event.action": "reply" });
            var body = $(containerElement + " #replyMessage").val();
            if (body.length == 0) return;
			logger.error(`reply(): body → ${body}`, { "event.action": "reply" });
            var conversationId = self.messagethreads()[0].ConversationId;
            $.ajax({
                type: "POST",
                url: baseServicepath + "Reply",
                beforeSend: serviceFramework.setModuleHeaders,
                data: { conversationId: conversationId, body: encodeURIComponent(body), fileIds: [] }
            }).done(function (data) {
                if (typeof data !== "undefined" &&
                    data != null &&
                    typeof data.Conversation !== "undefined") {
                    displayMessage("#dnnCoreMessaging", settings.messageSentText, "dnnFormSuccess");
					logger.error(`reply(): conversationId → ${conversationId}`, { "event.action": "reply" });

                    $(containerElement + " #replyMessage").val('');
                    self.messagethreads.push(new messageThreadView(data));

                    if (typeof data.TotalThreads !== "undefined" && $.type(data.TotalThreads) === "number") {
                        self.TotalThreads(data.TotalThreads);
                    }
                    if (typeof data.TotalNewThreads !== "undefined" && $.type(data.TotalNewThreads) === "number") {
                        self.TotalNewThreads(data.TotalNewThreads);
                    }
                    if (typeof data.TotalArchivedThreads !== "undefined" && $.type(data.TotalArchivedThreads) === "number") {
                        self.TotalArchivedThreads(data.TotalArchivedThreads);
                    }
                } else {
                    displayMessage("#dnnCoreMessaging", settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage("#dnnCoreMessaging", settings.serverErrorWithDescription + JSON.parse(xhr.responseText).ExceptionMessage, "dnnFormWarning");
            });
			logger.warn('<<< Exiting reply()', { "event.action": "reply" });
        };

        self.setReplySelected = function () {
			logger.error('>>> Entering setReplySelected()', { "event.action": "setReplySelected" });
            self.isReplySelected(true);
			logger.warn('<<< Exiting setReplySelected()', { "event.action": "setReplySelected" });
        };

        self.threadViewAfterAdd = function (node) {
			logger.error(`>>> Entering threadViewAfterAdd(node=${node})`, { "event.action": "threadViewAfterAdd", "logrite.node": node });
            if (node.childNodes.length > 0 && self.highlightThreads === true) {
                $(node).effect("highlight", {}, 3000);
				logger.warn(`<<< Exiting threadViewAfterAdd(node=${node})`, { "event.action": "threadViewAfterAdd", "logrite.node": node });
            }
        };

        self.selectAll = function () {
			logger.error('>>> Entering selectAll()', { "event.action": "selectAll" });
            $.each(self.messages(), function () {
                this.messageSelected(true);
            });
			logger.warn('<<< Exiting selectAll()', { "event.action": "selectAll" });
        };

        self.selectNone = function () {
			logger.error('>>> Entering selectNone()', { "event.action": "selectNone" });
            $.each(self.messages(), function () {
                this.messageSelected(false);
            });
			logger.warn('<<< Exiting selectNone()', { "event.action": "selectNone" });
        };

        self.selectRead = function () {
			logger.error('>>> Entering selectRead()', { "event.action": "selectRead" });
            $.each(self.messages(), function () {
                this.messageSelected(this.Read() === true);
            });
			logger.warn('<<< Exiting selectRead()', { "event.action": "selectRead" });
        };

        self.selectUnread = function () {
			logger.error('>>> Entering selectUnread()', { "event.action": "selectUnread" });
            $.each(self.messages(), function () {
                this.messageSelected(this.Read() === false);
            });
			logger.warn('<<< Exiting selectUnread()', { "event.action": "selectUnread" });
        };

        self.getTotals = function (refresh) {
			logger.error(`>>> Entering getTotals(refresh=${refresh})`, { "event.action": "getTotals", "logrite.refresh": refresh });
            $.ajax({
                type: "GET",
                beforeSend: serviceFramework.setModuleHeaders,
                url: gettotalspath,
                async: false,
                cache: false
            }).done(function (totalsViewModel) {
                if (typeof totalsViewModel !== "undefined" && totalsViewModel != null) {
                    var state = window.History.getState();
                    if (typeof totalsViewModel.TotalUnreadMessages !== "undefined" && $.type(totalsViewModel.TotalUnreadMessages) === "number") {
                        var oldTotalNewThreads = self.TotalNewThreads();
                        self.TotalNewThreads(totalsViewModel.TotalUnreadMessages);
                        if (refresh && oldTotalNewThreads !== totalsViewModel.TotalUnreadMessages) {
                            if (state.data == null || !state.data.view || (state.data.view == "messages" && state.data.action == "inbox")) {
                                self.loadBox(inboxpath);
                            }
                        }
                    }

                    if (typeof totalsViewModel.TotalNotifications !== "undefined" && $.type(totalsViewModel.TotalNotifications) === "number") {
                        var oldNotifications = self.TotalNotifications();
                        self.TotalNotifications(totalsViewModel.TotalNotifications);
                        if (refresh && oldNotifications !== totalsViewModel.TotalNotifications) {
                            if (state.data == null || !state.data.view || (state.data.view == "notifications" && state.data.action == "notifications")) {
                                self.loadNotificationsTabHandler();
								logger.warn(`<<< Exiting getTotals(refresh=${refresh})`, { "event.action": "getTotals", "logrite.refresh": refresh });
                            }
                        }
                    }
                }
            });
        };
		logger.warn('<<< Exiting coreMessagingViewModel()', { "event.action": "coreMessagingViewModel" });
    }

    this.init = function (element) {
		logger.error(`>>> Entering init(element=${element})`, { "event.action": "init", "logrite.element": element });
        containerElement = element;

        var viewModel = new coreMessagingViewModel();
		logger.error(`init(element=${element}): viewModel → ${viewModel}`, { "event.action": "init", "logrite.element": element });
        ko.applyBindings(viewModel, document.getElementById($(element).attr("id")));
        $(element).dnnComposeMessage(composeMessageOptions);

        var stateview = getQuerystring('view');
		logger.error(`init(element=${element}): stateview → ${stateview}`, { "event.action": "init", "logrite.element": element });
        //cover case where advanced rewriter used
        if (stateview == "") {
            if (document.URL.indexOf("view/notifications") >= 0) {
                stateview = "notifications";
            }
        }
        
        if (stateview === "notifications") {
            // load initial state of notifications
            $(element + ' #smMainContent').dnnTabs({ selected: 1 });
            viewModel.loadNotificationsTabHandler();
        } else {
            //load initial state of inbox
            $(element + ' #smMainContent').dnnTabs({ selected: 0 });
            viewModel.myinboxHandler();
        }

        setInterval(function () {
            //for issue DNN-5753
            viewModel.getTotals(true);
        }, refreshInterval);

        viewModel.getTotals();
		logger.warn(`<<< Exiting init(element=${element})`, { "event.action": "init", "logrite.element": element });
    };
	logger.warn(`<<< Exiting CoreMessaging(ko=${ko},settings=${settings},composeMessageOptions=${composeMessageOptions})`, { "event.action": "CoreMessaging", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageOptions": composeMessageOptions });
};