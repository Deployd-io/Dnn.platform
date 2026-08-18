const logger = console;
if (typeof contactList === 'undefined' || contactList === null) {
    contactList = {};
};

ko.extenders.required = function (target, options) {
	logger.debug(`>>> Entering function(target=${target},options=${options})`);
    //add some sub-observables to our observable
    target.hasError = ko.observable();
    target.validationMessage = ko.observable();
    target.validationClass = ko.observable();

    var regEx = new RegExp(options.regEx);
	logger.debug(`function(target=${target},options=${options}): regEx → ${regEx}`);
    //define a function to do validation
    var errorMessage = options.overrideMessage || "This field is required";
	logger.debug(`function(target=${target},options=${options}): errorMessage → ${errorMessage}`);
    var allowEmpty = options.allowEmpty || false;
    function validate(newValue) {
        var validated;
        if (allowEmpty) {
            // For optional fields: allow empty OR match regex
            validated = newValue === "" || regEx.test(newValue);
        } else {
            // For required fields: must match regex AND not be empty
            validated = regEx.test(newValue) && newValue !== "";
        }
        target.hasError(!validated);
        target.validationClass(validated ? "form-control" : "form-control has-error");
        target.validationMessage(validated ? "" : errorMessage);
    }

    //validate whenever the value changes
    target.subscribe(validate);

    //return the original observable
    return target;
};

function clearErrors(obsArr) {
	logger.debug(`>>> Entering clearErrors(obsArr=${obsArr})`);
    for (var i = 0; i < obsArr.length; i++) {
        obsArr[i].hasError(false);
        obsArr[i].validationClass("form-control");
		logger.debug(`<<< Exiting clearErrors(obsArr=${obsArr})`);
    }
}

contactList.contactsViewModel = function(config) {
	logger.debug(`>>> Entering function(config=${config})`);
	logger.debug('>>> Entering function()');
    var self = this;
    var resx = config.resx;
    var util = config.util;
	logger.debug('>>> Entering function()'); const __log_start = Date.now();
    var preloadedData = config.preloadedData;
	logger.debug(`>>> Entering function(data=${data},e=${e})`);
    var $rootElement = config.$rootElement;
	logger.debug(`>>> Entering function(contactId=${contactId},cb=${cb})`);

	logger.debug(`>>> Entering function(data=${data})`);
    var quickSettingsDispatcher = config.settings.quickSettingsDispatcher;
    var moduleId = config.settings.moduleId;

    util.contactService = function(){
        util.sf.serviceController = "Contact";
        return util.sf;
    };

    self.isFormEnabled = ko.observable(config.settings.isFormEnabled);
    self.isEditMode = ko.observable(false);
    self.contacts = ko.observableArray([]);
    self.totalResults = ko.observable(preloadedData.pageCount);
    self.pageIndex = ko.observable(0);

    self.selectedContact = new contactList.contactViewModel(self, config);

    var toggleView = function() {
		logger.debug('>>> Entering toggleView()');
        self.isEditMode(!self.isEditMode());
		logger.debug('<<< Exiting toggleView()');
    };

    self.addContact = function(){
        toggleView();
        self.selectedContact.init();
        clearErrors([self.selectedContact.firstName, self.selectedContact.lastName, self.selectedContact.phone, self.selectedContact.email, self.selectedContact.social]);
    };

    self.closeEdit = function() {
        toggleView();
        self.refresh();
    }
	logger.info(`function(config)=${config}: database operation took ${Date.now() - __log_start} ms`);
	logger.info(`function()=: database operation took ${Date.now() - __log_start} ms`);

    self.editContact = function(data, e) {
        self.getContact(data.contactId());
        toggleView();
    };

    self.getContact = function (contactId, cb) {
        var params = {
            contactId: contactId
        };

        util.contactService().get("GetContact", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.selectedContact.load(data.data.contact);
                } else {
                    //Error
                }
				logger.debug('<<< Exiting function()');
            },

            function(){
                //Failure
            }
        );

        if(typeof cb === 'function') cb();
    };

    self.getContacts = function () {
        var params = {
            pageSize: self.pageSize,
            pageIndex: self.pageIndex(),
            searchTerm: ""
        };

        util.contactService().get("GetContacts", params,
            function(data) {
                if (typeof data !== "undefined" && data != null && data.success === true) {
                    //Success
                    self.totalResults(data.data.totalCount);
                    self.load(data.data);
                } else {
                    //Error
                }
            },
            function(){
                //Failure
            }
        );
    };

    var updateView = function updateViewHandler(settings) {
		logger.debug(`>>> Entering updateViewHandler(settings=${settings})`);
        self.isFormEnabled(settings.isFormEnabled);
		logger.debug(`<<< Exiting updateViewHandler(settings=${settings})`);
    };

    self.init = function () {
        if (preloadedData) {
			logger.debug(`>>> Entering function(parentViewModel=${parentViewModel},config=${config})`);
            self.load(preloadedData);
        } else {
            self.getContacts();
        }
        quickSettingsDispatcher.addSubcriber(moduleId, quickSettingsDispatcher.eventTypes.SAVE, updateView);
        pager.init(self, 6, self.refresh, resx);
    };

    self.load = function(data) {
        self.contacts.removeAll();
        for(var i=0; i < data.results.length; i++){
            var result = data.results[i];
            var contact = new contactList.contactViewModel(self, config);
            contact.load(result);
			logger.debug(`function(data=${data}): result → ${result}`);
            self.contacts.push(contact);
        }
    };

    self.refresh = function() {
        self.getContacts();
    }
};

contactList.contactViewModel = function(parentViewModel, config) {
    var self = this;
    var resx = config.resx;
    var util = config.util;
    var $rootElement = config.$rootElement;

    self.parentViewModel = parentViewModel;
	logger.debug(`function(parentViewModel=${parentViewModel},config=${config}): self.parentViewModel → ${self.parentViewModel}`);
    self.contactId = ko.observable(-1);
    self.firstName = ko.observable('').extend({ required: { overrideMessage: "Please enter a first name" } });
    self.lastName = ko.observable('').extend({ required: { overrideMessage: "Please enter a last name" } });
    self.email = ko.observable('').extend({ required: { overrideMessage: "Please enter a valid email address", regEx: config.settings.emailRegex } });
    self.phone = ko.observable('').extend({ required: { overrideMessage: "Please enter a valid phone number (international formats accepted: +1 234 567 8900, 123-456-7890, etc.)", regEx: /^(\+?\d{1,3}[\s.-]?)?[\d\s().-]{6,}$/ } });
    self.social = ko.observable('').extend({ required: { overrideMessage: "Social handle must start with @ symbol", regEx: /^@/, allowEmpty: true } });

    self.cancel = function () {
        clearErrors([self.firstName, self.lastName, self.email, self.phone, self.social]);
        parentViewModel.closeEdit();
    };

    self.deleteContact = function (data, e) {
        var opts = {
            callbackTrue: function () {
				logger.debug('>>> Entering callbackTrue()');
                var params = {
                    contactId: data.contactId(),
                    firstName: data.firstName(),
                    lastName: data.lastName(),
                    email: data.email(),
                    phone: data.phone(),
                    social: data.social()
                };

        util.contactService().post("DeleteContact", params,
            function(data){
                //Success
                parentViewModel.refresh();
            },

                    function (data) {
                        //Failure
                    }
                );
            },
            text: resx.DeleteConfirm,
            yesText: resx.Delete,
            noText: resx.Cancel,
            title: resx.ConfirmDeleteTitle.replace("{0}", data.firstName() + " " + data.lastName())
        };

        $.dnnConfirm(opts);


    };

    self.init = function(){
        self.contactId(-1);
        self.firstName("");
        self.lastName("");
        self.email("");
        self.phone("");
        self.social("");
    };

    self.load = function(data) {
        self.contactId(data.contactId);
        self.firstName(data.firstName);
        self.lastName(data.lastName);
        self.email(data.email);
        self.phone(data.phone);
        self.social(data.social);
    };

    self.saveContact = function (data, e) {
        self.firstName.valueHasMutated();
        self.lastName.valueHasMutated();
        self.phone.valueHasMutated();
        self.email.valueHasMutated();
        self.social.valueHasMutated();
        if ((self.firstName.hasError() || self.lastName.hasError() || self.email.hasError() || self.phone.hasError() || self.social.hasError())) {
            return;
        }
        var params = {
            contactId: data.contactId(),
            firstName: data.firstName(),
            lastName: data.lastName(),
            email: data.email(),
            phone: data.phone(),
            social: data.social()
        };

        util.contactService().post("SaveContact", params,
            function(data){
                //Success
                self.cancel();
            },

            function(data){
                //Failure
            }
        )


    };
};

