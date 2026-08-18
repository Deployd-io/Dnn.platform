import { createStore, applyMiddleware, compose } from "redux";
import thunkMiddleware from "redux-thunk";
import reduxImmutableStateInvariant from "redux-immutable-state-invariant";
import reducers from "reducers/promptReducers";
import DevTools from "containers/DevTools";
const logger = console;
import { IS_DEV} from "globals/promptInit";

export default function configureStore(initialState) {
	logger.debug(`>>> Entering configureStore(initialState=${initialState})`);
    let enhancer;

    if (IS_DEV) {
        enhancer = compose(applyMiddleware(thunkMiddleware, reduxImmutableStateInvariant()), DevTools.instrument());
    } else {
        enhancer = applyMiddleware(thunkMiddleware);
    }

	logger.debug(`configureStore(initialState=${initialState}): enhancer → ${enhancer}`);
	logger.debug(`<<< Exiting configureStore(initialState=${initialState})`);
    return createStore(reducers, initialState, enhancer);
}
