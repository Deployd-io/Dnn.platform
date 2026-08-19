import React, { Component  } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import { getPasswordStrength } from "utils/PasswordStrength";
import {CommonUsersActions as UserActions } from "../../../actions";

import "./style.less";
const logger = console;

class Password extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.props.loadPasswordStrengthOptions();
		logger.debug('<<< Exiting componentDidMount()');
    }
   
    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <div className="passwordContainer">
                <SingleLineInputWithError label={Localization.get("Password") }
                    error={this.props.error.password}
                    onChange={this.props.onChangePassword }
                    tooltipMessage={Localization.get("Password.Help")}
                    errorMessage={Localization.get("Password.Required") }
                    style={this.props.style}
                    inputStyle={!this.props.requiresQuestionAndAnswer ? { marginBottom: 15 } : { marginBottom: 0 }}
                    type="password"
                    autoComplete="off"
                    value={this.props.UserDetails.password}  tabIndex={7}/>

                <div id="passwordStrengthBar" className={"passwordStrength " + getPasswordStrength(this.props.UserDetails.password, this.props.passwordStrengthOptions)}></div>
                <div id="passwordStrengthLabel" className={"passwordStrengthLabel " + getPasswordStrength(this.props.UserDetails.password, this.props.passwordStrengthOptions)}>
                    {getPasswordStrength(this.props.UserDetails.password, this.props.passwordStrengthOptions)}
                </div>
            </div>
        );
    }
}

Password.propTypes = {
    error:PropTypes.object,
    style: PropTypes.object.isRequired,
    UserDetails: PropTypes.object.isRequired,
    requiresQuestionAndAnswer : PropTypes.bool,
    onChangePassword : PropTypes.func.isRequired,
    passwordStrengthOptions : PropTypes.object,
    loadPasswordStrengthOptions : PropTypes.func
};

const mapStateToProps = (state) => {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        passwordStrengthOptions : state.users.passwordStrengthOptions
    };
};

const mapDispatchToProps = (dispatch) => {
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
    return {
        loadPasswordStrengthOptions : () =>{
            dispatch(UserActions.passwordStrength());
        }
    };
};

export default connect(mapStateToProps,mapDispatchToProps)(Password) ;