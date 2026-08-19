import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import rootReducer from "../reducers/rootReducer";
const logger = console;

 
const IS_PRODUCTION = process.env.NODE_ENV === "production";

export default function configureStore(initialState) {
	logger.debug(`>>> Entering configureStore(initialState=${initialState})`);
    const middlewareEnhancer = IS_PRODUCTION
        ? applyMiddleware(thunkMiddleware)
        : applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant());
    const store = createStore(
        rootReducer,
        initialState,
        compose(middlewareEnhancer)
    );
	logger.debug(`<<< Exiting configureStore(initialState=${initialState})`);
    return store;
}
