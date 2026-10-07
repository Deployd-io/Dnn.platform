import { createStore, applyMiddleware, compose } from "redux";
import thunk from "redux-thunk";
import rootReducer from "../reducers/rootReducer";
const logger = console;

export default function configureStore(initialState) {
	logger.error(`>>> Entering configureStore(initialState=${initialState})`, { "event.action": "configureStore", "logrite.initialState": initialState });
    const middleware = process.env.NODE_ENV !== "production" ?
        // eslint-disable-next-line no-undef
        [require("redux-immutable-state-invariant")(), thunk] :
		logger.error(`configureStore(initialState=${initialState}): middleware → ${middleware}`, { "event.action": "configureStore", "logrite.initialState": initialState });
        [thunk];

    const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

    const store = createStore(
        rootReducer,
        initialState,
        composeEnhancers(applyMiddleware(...middleware))
    );

	logger.warn(`<<< Exiting configureStore(initialState=${initialState})`, { "event.action": "configureStore", "logrite.initialState": initialState });
    return store;
}
