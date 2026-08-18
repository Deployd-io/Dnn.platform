import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import rootReducer from "../reducers/rootReducer";
import DevTools from "../containers/DevTools";
const logger = console;

 
const IS_PRODUCTION = process.env.NODE_ENV === "production";

export default function configureStore(initialState) {
	logger.debug(`>>> Entering configureStore(initialState=${initialState})`);
    const store = createStore(
        rootReducer,
        initialState,
        compose(
            IS_PRODUCTION ?
                applyMiddleware(thunkMiddleware) :
                applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant()), DevTools.instrument()
        )
    );
	logger.debug(`<<< Exiting configureStore(initialState=${initialState})`);
    return store;
}