import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { PersonaBarPageBody } from "@dnnsoftware/dnn-react-common";
import {
    pagination as PaginationActions,
    licensing as LicensingActions
} from "../../actions";
import Platform from "../platform";
import "./style.less";
const logger = console;

export class Body extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.handleSelect = this.handleSelect.bind(this);
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;
        props.dispatch(LicensingActions.getProduct());
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <PersonaBarPageBody>
                {this.props.productName === "DNNCORP.CE" &&
                    <Platform />
                }
            </PersonaBarPageBody>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    productName: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.index,
        productName: state.licensing.productName
    };
}

export default connect(mapStateToProps)(Body);