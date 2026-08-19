import { combineReducers } from "redux";
import pagination from "./paginationReducer";
import visiblePanel from "./visiblePanelReducer";
import siteInfo from "./siteInfoReducer";
import siteBehavior from "./siteBehaviorReducer";
import languages from "./languagesReducer";
import search from "./searchReducer";
import languageEditor from "./languageEditorReducer";
const logger = console;

function getExtraReducers() {
	logger.debug('>>> Entering getExtraReducers()');
    let extraReducers = {};
    if (window.dnn.SiteSettings && window.dnn.SiteSettings.SiteBehaviorExtras) {
        window.dnn.SiteSettings.SiteBehaviorExtras.forEach((extra) => {
            extraReducers[extra.ReducerKey] = extra.Reducer;
        });
    }    
    if (window.dnn.SiteSettings && window.dnn.SiteSettings.SearchExtras) {
        window.dnn.SiteSettings.SearchExtras.forEach((extra) => {
            extraReducers[extra.ReducerKey] = extra.Reducer;
        });
    }
	logger.debug('<<< Exiting getExtraReducers()');
    return extraReducers;
}

const rootReducer = combineReducers(Object.assign(getExtraReducers(), {
    pagination,
    visiblePanel,
    siteInfo,
    siteBehavior,
    languages,
    search,
    languageEditor
}));

export default rootReducer;
