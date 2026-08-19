import { createStore, applyMiddleware, compose } from "redux";
import thunk from "redux-thunk";
import rootReducer from "../reducers/rootReducer";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
const logger = console;

export default function configureStore(initialState) {
logger.debug(`>>> Entering configureStore(initialState=${initialState})`);

    const middleware = process.env.NODE_ENV !== "production" ?
        [reduxImmutableStateInvariant(), thunk] :
        [thunk];

    const composeEnhancers = window.__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || compose;

    const store = createStore(
        rootReducer,
        initialState,
        composeEnhancers(applyMiddleware(...middleware))
    );
	logger.debug(`<<< Exiting configureStore(initialState=${initialState})`);
    return store;
}
