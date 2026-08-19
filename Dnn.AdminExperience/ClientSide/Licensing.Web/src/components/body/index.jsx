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
		logger.debug('>>> Entering constructor()');
        this.handleSelect = this.handleSelect.bind(this);
		logger.debug('<<< Exiting constructor()');
    }

    handleSelect(index) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        props.dispatch(LicensingActions.getProduct());
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    /*eslint no-mixed-spaces-and-tabs: "error"*/
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.index,
        productName: state.licensing.productName
    };
}

export default connect(mapStateToProps)(Body);