var logger = (typeof logger !== 'undefined' && logger) || console;
function MemberDirectory($, ko, settings, composeMessageSettings) {
	logger.error(`>>> Entering MemberDirectory(ko=${ko},settings=${settings},composeMessageSettings=${composeMessageSettings})`, { "event.action": "MemberDirectory", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageSettings": composeMessageSettings });
    var opts = $.extend({}, MemberDirectory.defaultSettings, settings);
    var serviceFramework = settings.servicesFramework;
	logger.error(`MemberDirectory(ko=${ko},settings=${settings},composeMessageSettings=${composeMessageSettings}): opts → ${opts}`, { "event.action": "MemberDirectory", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageSettings": composeMessageSettings });
    var baseServicepath = serviceFramework.getServiceRoot('MemberDirectory') + 'MemberDirectory/';
	logger.error(`MemberDirectory(ko=${ko},settings=${settings},composeMessageSettings=${composeMessageSettings}): baseServicepath → ${baseServicepath}`, { "event.action": "MemberDirectory", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageSettings": composeMessageSettings });
    var pageIndex = 0;
    var pageSize = opts.pageSize;
    var userId = opts.userId;
	logger.error(`MemberDirectory(ko=${ko},settings=${settings},composeMessageSettings=${composeMessageSettings}): userId → ${userId}`, { "event.action": "MemberDirectory", "logrite.ko": ko, "logrite.settings": settings, "logrite.composeMessageSettings": composeMessageSettings });
    var groupId = opts.groupId;
    var viewerId = opts.viewerId;
    var profileUrl = opts.profileUrl;
    var profileUrlUserToken = opts.profileUrlUserToken;
    var profilePicHandler = opts.profilePicHandler;
    var containerElement = null;

    function displayMessage(message, cssclass) {
		logger.error(`>>> Entering displayMessage(message=${message},cssclass=${cssclass})`, { "event.action": "displayMessage", "logrite.message": message, "logrite.cssclass": cssclass });
        var messageNode = $("<div/>").addClass('dnnFormMessage ' + cssclass).text(message);
        $(containerElement).prepend(messageNode);
		logger.error(`displayMessage(message=${message},cssclass=${cssclass}): messageNode → ${messageNode}`, { "event.action": "displayMessage", "logrite.message": message, "logrite.cssclass": cssclass });
        messageNode.fadeOut(3000, 'easeInExpo', function () { messageNode.remove(); });
		logger.warn(`<<< Exiting displayMessage(message=${message},cssclass=${cssclass})`, { "event.action": "displayMessage", "logrite.message": message, "logrite.cssclass": cssclass });
    };

    function Member(item) {
		logger.error(`>>> Entering Member(item=${item})`, { "event.action": "Member", "logrite.item": item });
        var self = this;
        self.AddFriendText = opts.addFriendText;
		logger.debug(`Member(item=${item}): self.AddFriendText → ${self.AddFriendText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.FriendPendingText → ${self.FriendPendingText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.RemoveFriendText → ${self.RemoveFriendText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.FollowText → ${self.FollowText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.UnFollowText → ${self.UnFollowText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.SendMessageText → ${self.SendMessageText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.UserNameText → ${self.UserNameText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.EmailText → ${self.EmailText}`, { "event.action": "Member", "logrite.item": item });
		logger.debug(`Member(item=${item}): self.CityText → ${self.CityText}`, { "event.action": "Member", "logrite.item": item });
        self.AcceptFriendText = opts.acceptFriendText;
		logger.debug(`Member(item=${item}): self.AcceptFriendText → ${self.AcceptFriendText}`, { "event.action": "Member", "logrite.item": item });
        self.FriendPendingText = opts.friendPendingText;
        self.RemoveFriendText = opts.removeFriendText;
        self.FollowText = opts.followText;
        self.UnFollowText = opts.unFollowText;
        self.SendMessageText = opts.sendMessageText;
        self.UserNameText = opts.userNameText;
        self.EmailText = opts.emailText;
        self.CityText = opts.cityText;

        self.UserId = ko.observable(item.MemberId);
        self.LastName = ko.observable(item.LastName);
        self.FirstName = ko.observable(item.FirstName);
        self.UserName = ko.observable(item.UserName);
        self.DisplayName = ko.observable(item.DisplayName);
        self.Email = ko.observable(item.Email);

        self.IsUser = ko.observable(item.MemberId == viewerId);

        self.IsAuthenticated = (viewerId > 0);

        //Friend Observables
        self.FriendStatus = ko.observable(item.FriendStatus);
        self.FriendId = ko.observable(item.FriendId);

        self.IsFriend = ko.computed(function () {
            return self.FriendStatus() == 2;
        }, this);

        self.IsPending = ko.computed(function () {
            return self.FriendStatus() == 1 && self.FriendId() != viewerId;
        }, this);

        self.HasPendingRequest = ko.computed(function () {
            return self.FriendStatus() == 1 && self.FriendId() == viewerId;
        }, this);

        //Following Observables
        self.FollowingStatus = ko.observable(item.FollowingStatus);

        self.IsFollowing = ko.computed(function () {
            return self.FollowingStatus() == 2;
        }, this);

        //Follower Observables
        self.FollowerStatus = ko.observable(item.FollowerStatus);

        self.IsFollower = ko.computed(function () {
            return self.FollowerStatus() == 2;
        }, this);

        //Computed Profile Observables
        self.City = ko.observable(item.City);
        self.Title = ko.observable(item.Title);
        self.Country = ko.observable(item.Country);
        self.Phone = ko.observable(item.Phone);
        self.Website = ko.observable(item.Website);
        self.PhotoURL = ko.observable(item.PhotoURL);
        self.ProfileProperties = ko.observable(item.ProfileProperties);
        self.ProfileUrl  = ko.observable(item.ProfileUrl);
        self.Location = ko.computed(function () {
            var city = self.City();
            var country = self.Country();
            var location = (city != null) ? city : '';
            if (location != '' && country != null && country != '') {
                location += ', ';
            }
            if (country != null) {
                location += country;
            }

            return location;
        });
        
        self.getProfilePicture = function (w, h) {
			logger.error(`>>> Entering getProfilePicture(w=${w},h=${h})`, { "event.action": "getProfilePicture", "logrite.w": w, "logrite.h": h });
			logger.warn(`<<< Exiting getProfilePicture(w=${w},h=${h})`, { "event.action": "getProfilePicture", "logrite.w": w, "logrite.h": h });
            return profilePicHandler.replace("{0}", self.UserId()).replace("{1}", h).replace("{2}", w);
        };

        //Actions
        self.acceptFriend = function () {
			logger.error('>>> Entering acceptFriend()', { "event.action": "acceptFriend" });
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'AcceptFriend',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { friendId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FriendStatus(2);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn('<<< Exiting acceptFriend()', { "event.action": "acceptFriend" });
        };

        self.addFriend = function () {
			logger.error('>>> Entering addFriend()', { "event.action": "addFriend" });
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'AddFriend',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { friendId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FriendStatus(1);
                    self.FriendId(self.UserId);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn('<<< Exiting addFriend()', { "event.action": "addFriend" });
        };

        self.follow = function () {
			logger.error('>>> Entering follow()', { "event.action": "follow" });
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'Follow',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { followId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FollowingStatus(2);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn('<<< Exiting follow()', { "event.action": "follow" });
        };

        self.removeFriend = function () {
			logger.error('>>> Entering removeFriend()', { "event.action": "removeFriend" });
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'RemoveFriend',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { friendId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FriendStatus(0);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn('<<< Exiting removeFriend()', { "event.action": "removeFriend" });
        };

        self.unFollow = function () {
			logger.error('>>> Entering unFollow()', { "event.action": "unFollow" });
            $.ajax({
                type: "POST",
                cache: false,
                url: baseServicepath + 'UnFollow',
                beforeSend: serviceFramework.setModuleHeaders,
                data: { followId: self.UserId }
            }).done(function (data) {
                if (data.Result === "success") {
                    self.FollowingStatus(0);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn('<<< Exiting unFollow()', { "event.action": "unFollow" });
        };
		logger.warn(`<<< Exiting Member(item=${item})`, { "event.action": "Member", "logrite.item": item });
    };

    function MemberDirectoryViewModel(initialData) {
		logger.error(`>>> Entering MemberDirectoryViewModel(initialData=${initialData})`, { "event.action": "MemberDirectoryViewModel", "logrite.initialData": initialData });
        // Data
        var self = this;

        var initialMembers = $.map(initialData, function (item) { return new Member(item); });
		logger.error(`MemberDirectoryViewModel(initialData=${initialData}): initialMembers → ${initialMembers}`, { "event.action": "MemberDirectoryViewModel", "logrite.initialData": initialData });

        self.Visible = ko.observable(true);
        self.Members = ko.observableArray(initialMembers);
        self.CanLoadMore = ko.observable(initialMembers.length == pageSize);
        self.SearchTerm = ko.observable('');
		self.disablePrivateMessage = ko.observable(settings.disablePrivateMessage);

        self.ResetEnabled = ko.observable(false);

        self.HasMembers = ko.computed(function () {
            return self.Members().length > 0;
        }, this);

        self.AdvancedSearchTerm1 = ko.observable('');
        self.AdvancedSearchTerm2 = ko.observable('');
        self.AdvancedSearchTerm3 = ko.observable('');
        self.AdvancedSearchTerm4 = ko.observable('');

        self.LastSearch = ko.observable('Advanced');

        self.loadingData = ko.observable(false);

        //Action Methods
        self.advancedSearch = function () {
			logger.error('>>> Entering advancedSearch()', { "event.action": "advancedSearch" });
            pageIndex = 0;
            self.SearchTerm('');

            self.xhrAdvancedSearch();
            self.LastSearch('Advanced');
            self.ResetEnabled(true);
			logger.warn('<<< Exiting advancedSearch()', { "event.action": "advancedSearch" });
        };

        self.basicSearch = function () {
			logger.error('>>> Entering basicSearch()', { "event.action": "basicSearch" });
            pageIndex = 0;
            self.AdvancedSearchTerm1('');
            self.AdvancedSearchTerm2('');
            self.AdvancedSearchTerm3('');
            self.AdvancedSearchTerm4('');

            self.xhrBasicSearch();

            self.LastSearch('Basic');
            self.ResetEnabled(true);
			logger.warn('<<< Exiting basicSearch()', { "event.action": "basicSearch" });
        };

        self.getMember = function (item) {
			logger.error(`>>> Entering getMember(item=${item})`, { "event.action": "getMember", "logrite.item": item });
            self.SearchTerm(item.value);
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "GetMember",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    userId: item.userId
                }
            }).done(function (members) {
                if (typeof members !== "undefined" && members != null) {
                    var mappedMembers = $.map(members, function (member) { return new Member(member); });
                    self.Members(mappedMembers);
                    self.CanLoadMore(false);
                } else {
                    displayMessage(settings.serverErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
            });
			logger.warn(`<<< Exiting getMember(item=${item})`, { "event.action": "getMember", "logrite.item": item });
        };

        self.getSuggestions = function (term, response) {
			logger.error(`>>> Entering getSuggestions(term=${term},response=${response})`, { "event.action": "getSuggestions", "logrite.term": term, "logrite.response": response });
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "GetSuggestions",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    groupId: groupId,
                    displayName: term
                }
            }).done(function (data) {
                response(data);
            }).fail(function () {
                displayMessage(settings.searchErrorText, "dnnFormWarning");
                response({}); // From jQuery UI docs: You must always call the response callback even if you encounter an error
            });
			logger.warn(`<<< Exiting getSuggestions(term=${term},response=${response})`, { "event.action": "getSuggestions", "logrite.term": term, "logrite.response": response });
        };

        self.handleAfterRender = function (elements, data) {
			logger.error(`>>> Entering handleAfterRender(elements=${elements},data=${data})`, { "event.action": "handleAfterRender", "logrite.elements": elements, "logrite.data": data });
            for (var i in elements) {
                var element = elements[i];
                if (element.nodeType == 1) {
                    var config = {
                        over: function () {
							logger.error('>>> Entering over()', { "event.action": "over" });
                            $(this).find("div[id$='popUpPanel']").fadeIn("slow");
							logger.warn('<<< Exiting over()', { "event.action": "over" });
                        },
                        timeout: 500,
                        out: function () {
							logger.error('>>> Entering out()', { "event.action": "out" });
                            $(this).find("div[id$='popUpPanel']").fadeOut("fast");
							logger.warn('<<< Exiting out()', { "event.action": "out" });
                        }
                    };
                    $(element).hoverIntent(config);
					logger.error(`handleAfterRender(elements=${elements},data=${data}): config → ${config}`, { "event.action": "handleAfterRender", "logrite.elements": elements, "logrite.data": data });
                }
            }
        };

        self.isEven = function (item) {
			logger.error(`>>> Entering isEven(item=${item})`, { "event.action": "isEven", "logrite.item": item });
			logger.warn(`<<< Exiting isEven(item=${item})`, { "event.action": "isEven", "logrite.item": item });
            return (self.Members.indexOf(item) % 2 == 0);
        };

        self.loadMore = function () {
			logger.error('>>> Entering loadMore()', { "event.action": "loadMore" });
            pageIndex++;
            if (self.LastSearch() === 'Advanced') {
                self.xhrAdvancedSearch();
            }
            else {
                self.xhrBasicSearch();
				logger.warn('<<< Exiting loadMore()', { "event.action": "loadMore" });
            }
        };

        self.resetSearch = function () {
			logger.error('>>> Entering resetSearch()', { "event.action": "resetSearch" });
            self.SearchTerm('');
            self.AdvancedSearchTerm1('');
            self.AdvancedSearchTerm2('');
            self.AdvancedSearchTerm3('');
            self.AdvancedSearchTerm4('');

            self.xhrAdvancedSearch();
            self.LastSearch('Advanced');
            self.ResetEnabled(false);
			logger.warn('<<< Exiting resetSearch()', { "event.action": "resetSearch" });
        };

        self.xhrAdvancedSearch = function () {
			logger.error('>>> Entering xhrAdvancedSearch()', { "event.action": "xhrAdvancedSearch" });
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "AdvancedSearch",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    userId: userId,
                    groupId: groupId,
                    pageIndex: pageIndex,
                    pageSize: pageSize,
                    searchTerm1: self.AdvancedSearchTerm1(),
                    searchTerm2: self.AdvancedSearchTerm2(),
                    searchTerm3: self.AdvancedSearchTerm3(),
                    searchTerm4: self.AdvancedSearchTerm4()
                }
            }).done(function (members) {
                if (typeof members !== "undefined" && members != null) {
                    var mappedMembers = $.map(members, function (item) { return new Member(item); });
                    if (pageIndex === 0) {
                        self.Members(mappedMembers);
                    } else {
                        for (var i = 0; i < mappedMembers.length; i++) {
                            self.Members.push(mappedMembers[i]);
                        }
                    }
                    self.CanLoadMore(mappedMembers.length == pageSize);
                } else {
                    displayMessage(settings.searchErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.searchErrorText, "dnnFormWarning");
            });
			logger.warn('<<< Exiting xhrAdvancedSearch()', { "event.action": "xhrAdvancedSearch" });
        };

		logger.warn(`<<< Exiting MemberDirectoryViewModel(initialData=${initialData})`, { "event.action": "MemberDirectoryViewModel", "logrite.initialData": initialData });
        self.xhrBasicSearch = function () {
			logger.error('>>> Entering xhrBasicSearch()', { "event.action": "xhrBasicSearch" });
            $.ajax({
                type: "GET",
                cache: false,
                url: baseServicepath + "BasicSearch",
                beforeSend: serviceFramework.setModuleHeaders,
                data: {
                    groupId: groupId,
                    searchTerm: self.SearchTerm(),
                    pageIndex: pageIndex,
                    pageSize: pageSize
                }
            }).done(function (members) {
                if (typeof members !== "undefined" && members != null) {
                    var mappedMembers = $.map(members, function (item) { return new Member(item); });
                    if (pageIndex === 0) {
                        self.Members(mappedMembers);
                    } else {
                        for (var i = 0; i < mappedMembers.length; i++) {
                            self.Members.push(mappedMembers[i]);
                        }
                    }
                    self.CanLoadMore(mappedMembers.length == pageSize);
                } else {
                    displayMessage(settings.searchErrorText, "dnnFormWarning");
                }
            }).fail(function (xhr, status) {
                displayMessage(settings.searchErrorText, "dnnFormWarning");
            });
			logger.warn('<<< Exiting xhrBasicSearch()', { "event.action": "xhrBasicSearch" });
        };
    };

    this.init = function (element) {
		logger.error(`>>> Entering init(element=${element})`, { "event.action": "init", "logrite.element": element });
        containerElement = element;

        //load initial state of inbox
        $.ajax({
            type: "GET",
            cache: false,
            url: baseServicepath + "AdvancedSearch",
            beforeSend: serviceFramework.setModuleHeaders,
            data: {
                userId: userId,
                groupId: groupId,
                pageIndex: pageIndex,
                pageSize: pageSize,
                searchTerm1: '',
                searchTerm2: '',
                searchTerm3: '',
                searchTerm4: ''
            }
        }).done(function (members) {
            if (typeof members !== "undefined" && members != null) {
                var viewModel = new MemberDirectoryViewModel(members);
                ko.applyBindings(viewModel, document.getElementById($(element).attr("id")));

                //Basic Search
                $('input#mdBasicSearch').autocomplete({
                    source: function (request, response) {
						logger.error(`>>> Entering source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
                        viewModel.getSuggestions(request.term, response);
						logger.warn(`<<< Exiting source(request=${request},response=${response})`, { "event.action": "source", "logrite.request": request, "logrite.response": response });
                        return;
                    },
                    minLength: 1,
                    select: function (event, ui) {
						logger.error(`>>> Entering select(event=${event},ui=${ui})`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
                        viewModel.getMember(ui.item);
						logger.warn(`<<< Exiting select(event=${event},ui=${ui})`, { "event.action": "select", "logrite.event": event, "logrite.ui": ui });
                    }
                });

                //Advanced Search
                $('a#mdAdvancedSearch').click(function (event) {
                    event.preventDefault();
                    $('div#mdAdvancedSearchForm').slideDown();
                    $(this).addClass("active");
                    $(".mdSearch").addClass("active");
                });
                
                var timer;
                var cursorIsOnAdvancedSearchForm;
                $('a#mdAdvancedSearch').mouseleave(function () {
                    timer = setTimeout(function () {
                        if ($('div#mdAdvancedSearchForm').is(':visible') && !cursorIsOnAdvancedSearchForm) {
                            $('div#mdAdvancedSearchForm').hide();
                            $(this).removeClass("active");
                            $(".mdSearch").removeClass("active");
                        }
                    }, 150);

                });
                $('div#mdAdvancedSearchForm').mouseenter(function () {
                    cursorIsOnAdvancedSearchForm = true;
                });
                $('div#mdAdvancedSearchForm').mouseleave(function () {
                    clearTimeout(timer);
                    cursorIsOnAdvancedSearchForm = false;
                    $(this).hide();
					logger.debug(`init(element=${element}): cursorIsOnAdvancedSearchForm → ${cursorIsOnAdvancedSearchForm}`, { "event.action": "init", "logrite.element": element });
                    $('a#mdAdvancedSearch').removeClass("active");
                    $(".mdSearch").removeClass("active");
                });

            	//Compose Message
	            if (!settings.disablePrivateMessage) {
		            var options = $.extend({}, {
			            openTriggerSelector: containerElement + " .ComposeMessage",
			            onPrePopulate: function(target) {
							logger.error(`>>> Entering onPrePopulate(target=${target})`, { "event.action": "onPrePopulate", "logrite.target": target });
				            var context = ko.contextFor(target);
				            var prePopulatedRecipients = [{ id: "user-" + context.$data.UserId(), name: context.$data.DisplayName() }];
							logger.error(`onPrePopulate(target=${target}): context → ${context}`, { "event.action": "onPrePopulate", "logrite.target": target });
							logger.warn(`<<< Exiting onPrePopulate(target=${target})`, { "event.action": "onPrePopulate", "logrite.target": target });
				            return prePopulatedRecipients;
			            },
			            servicesFramework: serviceFramework
		            }, composeMessageSettings);
		            $.fn.dnnComposeMessage(options);
	            }
            } else {
                displayMessage(settings.serverErrorText, "dnnFormWarning");
            }
        }).fail(function (xhr, status) {
            displayMessage(settings.serverErrorWithDescriptionText + status, "dnnFormWarning");
        });
		logger.warn(`<<< Exiting init(element=${element})`, { "event.action": "init", "logrite.element": element });
    };
}

MemberDirectory.defaultSettings = {
    userId: -1,
    groupId: -1,
    viewerId: -1,
    profileUrl: "",
    profileUrlUserToken: "PROFILEUSER",
    addFriendText: "AddFriend",
    acceptFriendText: "AcceptFriend",
    friendPendingText: "FriendPendingText",
    removeFriendText: "RemoveFriendText",
    followText: "FollowText",
    unFollowText: "UnFollowText",
    sendMessageText: "SendMessageText",
    userNameText: "UserNameText",
    emailText: "EmailText",
    cityText: "CityText",
    serverErrorText: "ServerErrorText",
    serverErrorWithDescriptionText: "ServerErrorWithDescriptionText"
};
