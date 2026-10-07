var logger = (typeof logger !== 'undefined' && logger) || console;
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

(function ($) {
    window.Subscription = function Subscription (ko, settings, bindingElement) {
		logger.error(`>>> Entering Subscription(ko=${ko},settings=${settings},bindingElement=${bindingElement})`, { "event.action": "Subscription", "logrite.ko": ko, "logrite.settings": settings, "logrite.bindingElement": bindingElement });
        var that = this;

		logger.debug(`Subscription(ko=${ko},settings=${settings},bindingElement=${bindingElement}): that.messageFrequency → ${that.messageFrequency}`, { "event.action": "Subscription", "logrite.ko": ko, "logrite.settings": settings, "logrite.bindingElement": bindingElement });
		logger.debug(`Subscription(ko=${ko},settings=${settings},bindingElement=${bindingElement}): that.notificationFrequency → ${that.notificationFrequency}`, { "event.action": "Subscription", "logrite.ko": ko, "logrite.settings": settings, "logrite.bindingElement": bindingElement });
        $.extend(this, Subscription.defaultSettings, settings);
        $.extend(this, settings);
	    
		this.requestService = function(path, type, data, success, failure) {
			logger.error(`>>> Entering requestService(path=${path},type=${type},data=${data},success=${success},failure=${failure})`, { "event.action": "requestService", "logrite.path": path, "logrite.type": type, "logrite.data": data, "logrite.success": success, "logrite.failure": failure });
		    $.ajax({
			    type: type,
			    url: that.servicesFramework.getServiceRoot('CoreMessaging') + path,
			    beforeSend: that.servicesFramework.setModuleHeaders,
			    data: data,
			    cache: false
		    }).done(function(response) {
			    success.call(that, response);
		    }).fail(function(xhr, status) {
			    failure.call(that, xhr);
		    });
			logger.warn(`<<< Exiting requestService(path=${path},type=${type},data=${data},success=${success},failure=${failure})`, { "event.action": "requestService", "logrite.path": path, "logrite.type": type, "logrite.data": data, "logrite.success": success, "logrite.failure": failure });
	    };
	    
        this.servicesFramework = $.ServicesFramework(settings.moduleId);
        this.localizationController = new LocalizationController(settings, that);
        this.subscriptionsViewModel = new SubscriptionsViewModel(ko, settings, that);

        this.localizer = function () {
			logger.error('>>> Entering localizer()', { "event.action": "localizer" });
            return that.localizationController;
        };

        this.getString = function (key) {
			logger.error(`>>> Entering getString(key=${key})`, { "event.action": "getString", "logrite.key": key });
            var localizer = that.localizer();
            if (localizer != null) {
                return localizer.getString(key);
            }

			logger.warn(`<<< Exiting getString(key=${key})`, { "event.action": "getString", "logrite.key": key });
            return key;
        };
        
        this.frequencyOptions = [
            { value: '0', text: that.getString('Instant') },
            { value: '1', text: that.getString('Daily') },
            { value: '2', text: that.getString('Hourly') },
            { value: '3', text: that.getString('Weekly') },
            { value: '-1', text: that.getString('Never') }
        ];
        
		logger.debug(`Subscription(ko=${ko},settings=${settings},bindingElement=${bindingElement}): this.frequencyOptions → ${this.frequencyOptions}`, { "event.action": "Subscription", "logrite.ko": ko, "logrite.settings": settings, "logrite.bindingElement": bindingElement });
        this.notifyFrequency = ko.observable(settings.notifyFrequency);
        this.msgFrequency = ko.observable(settings.msgFrequency);
        
        this.save = function() {
			logger.error('>>> Entering save()', { "event.action": "save" });
            var startSearch = function () {
				logger.error('>>> Entering startSearch()', { "event.action": "startSearch" });
                $('#fsFrequency', settings.moduleScope).addClass('searching');
				logger.warn('<<< Exiting startSearch()', { "event.action": "startSearch" });
            };

            var finishSearch = function () {
				logger.error('>>> Entering finishSearch()', { "event.action": "finishSearch" });
                $('#fsFrequency', settings.moduleScope).removeClass('searching');
				logger.warn('<<< Exiting finishSearch()', { "event.action": "finishSearch" });
            };

            var success = function (model) {
				logger.error(`>>> Entering success(model=${model})`, { "event.action": "success" });
            	
                that.messageFrequency = model.messagesEmailFrequency;
                that.notificationFrequency = model.notificationsEmailFrequency;

                $('#divUpdated').show();
                $("#divUpdated").delay(1800).hide(400);
                finishSearch();
				logger.warn(`<<< Exiting success(model=${model})`, { "event.action": "success" });
            };

            var failure = function (xhr, status) {
				logger.error(`>>> Entering failure(xhr=${xhr},status=${status})`, { "event.action": "failure", "logrite.xhr": xhr, "logrite.status": status });
                var localizer = that.localizationController;

                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').replace("{0}", status || that.getString('UnknownError'))
                });

                finishSearch();
				logger.warn(`<<< Exiting failure(xhr=${xhr},status=${status})`, { "event.action": "failure", "logrite.xhr": xhr, "logrite.status": status });
            };

            startSearch();

            var params = {                
                NotifyFreq: $('#ddlNotify').val(),
                MsgFreq: $('#ddlMsg').val()
            };

            that.requestService('Subscriptions/UpdateSystemSubscription', 'post', params, success, failure);
			logger.warn('<<< Exiting save()', { "event.action": "save" });
        };
        
        this['delete'] = function (model) {
            var localizer = that.localizationController;
            
            var success = function () {
				logger.error('>>> Entering success()', { "event.action": "success" });
                $('#divUnsubscribed').show();                
                that.subscriptionsViewModel.search();
                $("#divUnsubscribed").delay(1800).hide(400);
            };

            var failure = function (xhr, status) {
                $.dnnAlert({
                    title: localizer.getString('SubscribeFailureTitle'),
                    text: localizer.getString('SubscribeFailure').replace("{0}", status || that.getString('UnknownError'))
                });
            };
            
            $("<div class='dnnDialog'></div>").html(localizer.getString('UnsubscribeConfirm'))
                .dialog({
                    modal: true,
                    autoOpen: true,
                    classes: {
                        'ui-dialog': 'dnnFormPopup',
                    },
                    width: 400,
                    height: 200,
                    resizable: false,
                    title: localizer.getString('UnsubscribeConfirmTitle'),
                    buttons:
                        [
                            {
                                id: "ok_button",
                                text: localizer.getString('Yes'),
                                "class": "dnnPrimaryAction",
                                click: function () {
								logger.error('>>> Entering click()', { "event.action": "click" });

                                    var params = {                
                                        SubscriptionId: model.subscriptionId
                                    };

                                    that.requestService('Subscriptions/DeleteContentSubscription', 'post', params, success, failure);
                                    $(this).dialog("close");
									logger.warn('<<< Exiting click()', { "event.action": "click" });
                                }
                            },
                            {
                                id: "no_button",
                                text: localizer.getString('No'),
                                click: function () {
                                    $(this).dialog("close");
                                },
                                "class": "dnnSecondaryAction"
                            }
                        ]
                });
            };
        
        ko.applyBindings(this, document.getElementById(bindingElement));
		logger.warn(`<<< Exiting Subscription(ko=${ko},settings=${settings},bindingElement=${bindingElement})`, { "event.action": "Subscription", "logrite.ko": ko, "logrite.settings": settings, "logrite.bindingElement": bindingElement });
    };

    Subscription.defaultSettings = {
        moduleId: -1,
        portalId: 0,
        pageSize: 25,
        notifyFrequency: 2,
        msgFrequency: 0        
    };
})(window.jQuery);