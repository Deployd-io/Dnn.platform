import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import rootReducer from "../reducers/rootReducer";
const logger = console;

 
const IS_PRODUCTION = process.env.NODE_ENV === "production";

export default function configureStore(initialState) {
	logger.error(`>>> Entering configureStore(initialState=${initialState})`, { "event.action": "configureStore", "logrite.initialState": initialState });
    const middlewareEnhancer = IS_PRODUCTION
        ? applyMiddleware(thunkMiddleware)
        : applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant());
    const store = createStore(
        rootReducer,
        initialState,
        compose(middlewareEnhancer)
    );
	logger.warn(`<<< Exiting configureStore(initialState=${initialState})`, { "event.action": "configureStore", "logrite.initialState": initialState });
    return store;
}
