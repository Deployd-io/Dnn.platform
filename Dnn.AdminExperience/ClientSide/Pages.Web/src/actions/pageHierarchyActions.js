import ActionTypes from "../constants/actionTypes/pageHierarchyActionTypes";
const logger = console;

const pageHierarchyActions = {
    setSearchKeyword(searchKeyword) {
		logger.debug(`>>> Entering setSearchKeyword(searchKeyword=${searchKeyword})`);
		logger.debug(`<<< Exiting setSearchKeyword(searchKeyword=${searchKeyword})`);
        return {
            type: ActionTypes.SET_SEARCH_KEYWORD,
            searchKeyword
        };    
    },

    setItemTemplate(itemTemplate) {
		logger.debug(`>>> Entering setItemTemplate(itemTemplate=${itemTemplate})`);
		logger.debug(`<<< Exiting setItemTemplate(itemTemplate=${itemTemplate})`);
        return {
            type: ActionTypes.SET_ITEM_TEMPLATE,
            itemTemplate
        };
    },

    setDragItemTemplate(itemTemplate) {
		logger.debug(`>>> Entering setDragItemTemplate(itemTemplate=${itemTemplate})`);
		logger.debug(`<<< Exiting setDragItemTemplate(itemTemplate=${itemTemplate})`);
        return {
            type: ActionTypes.SET_DRAG_ITEM_TEMPLATE,
            itemTemplate
        };
    },

    selectPage(page) {
		logger.debug(`>>> Entering selectPage(page=${page})`);
		logger.debug(`<<< Exiting selectPage(page=${page})`);
        return {
            type: ActionTypes.SELECT_PAGE,
            page
        };
    },

    changeSelectedPagePath(path) {
		logger.debug(`>>> Entering changeSelectedPagePath(path=${path})`);
		logger.debug(`<<< Exiting changeSelectedPagePath(path=${path})`);
        return {
            type: ActionTypes.CHANGE_SELECTED_PAGE_PATH,
            path
        };
    }
};


export default pageHierarchyActions;