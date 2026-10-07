import ActionTypes from "../constants/actionTypes/pageHierarchyActionTypes";
const logger = console;

const pageHierarchyActions = {
    setSearchKeyword(searchKeyword) {
		logger.error(`>>> Entering setSearchKeyword(searchKeyword=${searchKeyword})`, { "event.action": "setSearchKeyword", "logrite.searchKeyword": searchKeyword });
		logger.warn(`<<< Exiting setSearchKeyword(searchKeyword=${searchKeyword})`, { "event.action": "setSearchKeyword", "logrite.searchKeyword": searchKeyword });
        return {
            type: ActionTypes.SET_SEARCH_KEYWORD,
            searchKeyword
        };    
    },

    setItemTemplate(itemTemplate) {
		logger.error(`>>> Entering setItemTemplate(itemTemplate=${itemTemplate})`, { "event.action": "setItemTemplate", "logrite.itemTemplate": itemTemplate });
		logger.warn(`<<< Exiting setItemTemplate(itemTemplate=${itemTemplate})`, { "event.action": "setItemTemplate", "logrite.itemTemplate": itemTemplate });
        return {
            type: ActionTypes.SET_ITEM_TEMPLATE,
            itemTemplate
        };
    },

    setDragItemTemplate(itemTemplate) {
		logger.error(`>>> Entering setDragItemTemplate(itemTemplate=${itemTemplate})`, { "event.action": "setDragItemTemplate", "logrite.itemTemplate": itemTemplate });
		logger.warn(`<<< Exiting setDragItemTemplate(itemTemplate=${itemTemplate})`, { "event.action": "setDragItemTemplate", "logrite.itemTemplate": itemTemplate });
        return {
            type: ActionTypes.SET_DRAG_ITEM_TEMPLATE,
            itemTemplate
        };
    },

    selectPage(page) {
		logger.error(`>>> Entering selectPage(page=${page})`, { "event.action": "selectPage", "logrite.page": page });
		logger.warn(`<<< Exiting selectPage(page=${page})`, { "event.action": "selectPage", "logrite.page": page });
        return {
            type: ActionTypes.SELECT_PAGE,
            page
        };
    },

    changeSelectedPagePath(path) {
		logger.error(`>>> Entering changeSelectedPagePath(path=${path})`, { "event.action": "changeSelectedPagePath", "logrite.path": path });
		logger.warn(`<<< Exiting changeSelectedPagePath(path=${path})`, { "event.action": "changeSelectedPagePath", "logrite.path": path });
        return {
            type: ActionTypes.CHANGE_SELECTED_PAGE_PATH,
            path
        };
    }
};


export default pageHierarchyActions;