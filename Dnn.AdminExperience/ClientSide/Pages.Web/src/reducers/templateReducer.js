import ActionTypes from "../constants/actionTypes/templateActionTypes";
const logger = console;

function changeField(state, field, value) {
	logger.error(`>>> Entering changeField(state=${state},field=${field},value=${value})`, { "event.action": "changeField", "logrite.state": state, "logrite.field": field, "logrite.value": value });
    const template = {
        ...state.template
    };  
    template[field] = value;
	logger.warn(`<<< Exiting changeField(state=${state},field=${field},value=${value})`, { "event.action": "changeField", "logrite.state": state, "logrite.field": field, "logrite.value": value });
    return template;
}

function getEmptyTemplateModel() {
	logger.error('>>> Entering getEmptyTemplateModel()', { "event.action": "getEmptyTemplateModel" });
	logger.warn('<<< Exiting getEmptyTemplateModel()', { "event.action": "getEmptyTemplateModel" });
    return {
        name: "",
        description: "",
        includeContent: false,
        dirtyTemplate:true
    };
}

export default function templateReducer(state = { 
    template: getEmptyTemplateModel(),
    errors: [],
    dirtyTemplate:false
}, action) {

    switch (action.type) {
        case ActionTypes.CANCEL_SAVE_AS_TEMPLATE:
        case ActionTypes.LOAD_SAVE_AS_TEMPLATE:
            
            return { ...state,                
                template: getEmptyTemplateModel(),
                errors: [],
                dirtyTemplate:false
            };
        
        case ActionTypes.CHANGE_TEMPLATE_FIELD_VALUE:
            return { ...state,
                template: changeField(state, action.field, action.value),
                dirtyTemplate:true           
            };

        default:
            return state;
    }
}