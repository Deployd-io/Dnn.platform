import ActionTypes from "../constants/actionTypes/addPagesActionTypes";
const logger = console;

function getEmptyBulkPage() {
	logger.error('>>> Entering getEmptyBulkPage()', { "event.action": "getEmptyBulkPage" });
	logger.warn('<<< Exiting getEmptyBulkPage()', { "event.action": "getEmptyBulkPage" });
    return {
        bulkPages: "",
        parentId: null,
        keywords: "",
        tags: "",
        includeInMenu: true,
        startDate: null,
        endDate: null
    };
}

function changeField(state, field, value) {
	logger.error(`>>> Entering changeField(state=${state},field=${field},value=${value})`, { "event.action": "changeField", "logrite.state": state, "logrite.field": field, "logrite.value": value });
    const newBulkPage = {
        ...state.bulkPage
    };  
    newBulkPage[field] = value;
    
	logger.warn(`<<< Exiting changeField(state=${state},field=${field},value=${value})`, { "event.action": "changeField", "logrite.state": state, "logrite.field": field, "logrite.value": value });
    return newBulkPage;
}

export default function addPagesReducer(state = { 
    bulkPage: getEmptyBulkPage()
}, action) {

    switch (action.type) {
        case ActionTypes.LOAD_ADD_MULTIPLE_PAGES:
            return { ...state,                
                bulkPage: getEmptyBulkPage(),
                dirtyBulkPage:false
            };
        
        case ActionTypes.CHANGE_MULTIPLE_PAGE_VALUE:
            return { ...state,
                bulkPage: changeField(state, action.field, action.value),
                dirtyBulkPage:true           
            };

        default:
            return state;
    }
}