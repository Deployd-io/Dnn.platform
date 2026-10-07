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
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.props.loadPasswordStrengthOptions();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }
   
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        passwordStrengthOptions : state.users.passwordStrengthOptions
    };
};

const mapDispatchToProps = (dispatch) => {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        loadPasswordStrengthOptions : () =>{
            dispatch(UserActions.passwordStrength());
        }
    };
};

export default connect(mapStateToProps,mapDispatchToProps)(Password) ;