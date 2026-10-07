var logger = (typeof logger !== 'undefined' && logger) || console;
(function ($) {
    if (typeof dnn === "undefined" || dnn === null) { dnn = {}; };
    if (dnn.permissionGridManager) return;

    dnn.permissionGridManager = function (scopeId) {
		logger.error(`>>> Entering permissionGridManager(scopeId=${scopeId})`, { "event.action": "permissionGridManager", "logrite.scopeId": scopeId });
        var inputBox = $('#' + scopeId + '_txtUser');
        var userIdsField = $('#' + scopeId + '_hiddenUserIds');
		logger.error(`permissionGridManager(scopeId=${scopeId}): inputBox → ${inputBox}`, { "event.action": "permissionGridManager", "logrite.scopeId": scopeId });
		logger.error(`permissionGridManager(scopeId=${scopeId}): userIdsField → ${userIdsField}`, { "event.action": "permissionGridManager", "logrite.scopeId": scopeId });
        var init = function () {
            var serviceUrl = $.ServicesFramework(-1).getServiceRoot('InternalServices') + 'ItemListService/SearchUser';
			logger.error('>>> Entering init()', { "event.action": "init" });
            inputBox.tokenInput(serviceUrl, {
				theme: "facebook",
				resultsFormatter: function (item) {
					logger.error(`>>> Entering resultsFormatter(item=${item})`, { "event.action": "resultsFormatter", "logrite.item": item });
					logger.warn(`<<< Exiting resultsFormatter(item=${item})`, { "event.action": "resultsFormatter", "logrite.item": item });
				    return "<li class='user'><img src='" + item.iconfile + "' title='" + item.name + "' style='width:25px;height:25px;' /><span>" + item.name + "</span></li>";
				},
				minChars: 2,
				preventDuplicates: true,
				hintText: '',
				onAdd: function (item) {
					logger.error(`>>> Entering onAdd(item=${item})`, { "event.action": "onAdd", "logrite.item": item });
					if (userIdsField.val() == '') {
					    userIdsField.val(item.id);
					} else {
					    var array = userIdsField.val().split(','),
						index = $.inArray(item.id, array);
						logger.error(`onAdd(item=${item}): array → ${array}`, { "event.action": "onAdd", "logrite.item": item });
					    if (index == -1) {
					        userIdsField.val(userIdsField.val() + ',' + item.id);
							logger.warn(`<<< Exiting onAdd(item=${item})`, { "event.action": "onAdd", "logrite.item": item });
					    }
					}
				},
				onDelete: function (item) {
					logger.error(`>>> Entering onDelete(item=${item})`, { "event.action": "onDelete", "logrite.item": item });
					var array = userIdsField.val().split(','),
						id = item.id,
						index = $.inArray(id, array);
						logger.error(`onDelete(item=${item}): array → ${array}`, { "event.action": "onDelete", "logrite.item": item });
					if (index !== -1) {
					    array.splice(index, 1);
					    userIdsField.val(array.join(','));
						logger.warn(`<<< Exiting onDelete(item=${item})`, { "event.action": "onDelete", "logrite.item": item });
					}
				},
				onError: function (xhr, status) {
					logger.error(`>>> Entering onError(xhr=${xhr},status=${status})`, { "event.action": "onError", "logrite.xhr": xhr, "logrite.status": status });
					//displayMessage(composeMessageDialog, opts.autoSuggestErrorText + status);
					logger.warn(`<<< Exiting onError(xhr=${xhr},status=${status})`, { "event.action": "onError", "logrite.xhr": xhr, "logrite.status": status });
				}
            });

	        var inputBoxId = inputBox.attr('id');
			logger.error(`init(): inputBoxId → ${inputBoxId}`, { "event.action": "init" });
	        $('label[for="' + inputBoxId + '"]').attr('for', 'token-input-' + inputBoxId);

            var roleId = $('#' + scopeId + '_roleField');
			logger.error(`init(): roleId → ${roleId}`, { "event.action": "init" });
            var roleSelector = $('#' + scopeId + '_cboSelectRole');
			logger.error(`init(): roleSelector → ${roleSelector}`, { "event.action": "init" });
            roleSelector.change(function () {
                updateHiddenField();
            });
            updateHiddenField();

            function updateHiddenField() {
				logger.error('>>> Entering updateHiddenField()', { "event.action": "updateHiddenField" });
                var Id = roleSelector.val();
                roleId.val(Id);
				logger.error(`updateHiddenField(): Id → ${Id}`, { "event.action": "updateHiddenField" });
            }
			logger.warn('<<< Exiting init()', { "event.action": "init" });
        }

        $(document).ready(function() {
            init();
        });
        return this;
    };
})(jQuery);