import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import { CommonUsersActions } from "../../../actions";
import utilities from "utils";
import { Button, SingleLineInputWithError, GridSystem, GridCell } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

const inputStyle = { width: "100%" };
const blankChangePassword = {
    userId: 0,
    password: ""
};

class ChangePassword extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            changePassword: Object.assign({}, blankChangePassword),
            errors: {
                password: false,
                confirmPassword: false,
                passwordsMatch: false
            },
            confirmPassword: ""
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    componentWillMount() {
		logger.debug('>>> Entering componentWillMount()');
        let {changePassword} = this.state;
        changePassword.userId = this.props.userId;
		logger.debug(`componentWillMount(): changePassword.userId → ${changePassword.userId}`);
        changePassword.password = "";
		logger.debug('<<< Exiting componentWillMount()');
        this.setState({
            changePassword
        });
    }
    componentWillReceiveProps(newProps) {
		logger.debug(`>>> Entering componentWillReceiveProps(newProps=${newProps})`);
        this.clear(() => {
            let {changePassword} = this.state;
			logger.debug(`<<< Exiting componentWillReceiveProps(newProps=${newProps})`);
            changePassword.userId = newProps.userId;
            this.setState({
                changePassword
            });
        });
    }

    onChange(key, item) {
        if (key === "confirmPassword") {
            this.setState({
                confirmPassword: item.target.value
            }, () => {
                this.validateForm();
            });
        } else {
            let {changePassword} = this.state;
            changePassword[key] = item.target.value;
            this.setState({ changePassword }, () => {
                this.validateForm();
            });
        }
    }
    save() {
		logger.debug('>>> Entering save()');
        if (this.validateForm()) {
            this.props.dispatch(CommonUsersActions.changePassword(this.state.changePassword, () => {
                this.cancel();
                utilities.notify(Localization.get("ChangeSuccessful"), 3000);
				logger.debug('<<< Exiting save()');
            }));
        }
    }
    validateForm() {
        let valid = true;
        let {errors} = this.state;
        errors.password = false;
        errors.confirmPassword = false;
        errors.passwordsMatch = false;
        let {changePassword} = this.state;
        let {confirmPassword} = this.state;
        if (changePassword.password === "") {
            errors.password = true;
            valid = false;
        }
        if (changePassword.confirmPassword === "") {
            errors.confirmPassword = true;
            valid = false;
        }
        else if (confirmPassword !== changePassword.password) {
            errors.passwordsMatch = true;
            valid = false;
        }
        this.setState({ errors });

        return valid;
    }
    clear(callback) {
		logger.debug(`>>> Entering clear(callback=${callback})`);
        this.setState({
            changePassword: Object.assign({}, blankChangePassword),
            confirmPassword: "",
            errors: {
                password: false,
                confirmPassword: false,
                passwordsMatch: false
            }
        }, () => {
            if (typeof callback === "function") {
                callback();
				logger.debug(`<<< Exiting clear(callback=${callback})`);
            }
        });
    }
    cancel() {
		logger.debug('>>> Entering cancel()');
        this.clear();
        if (typeof this.props.onCancel === "function")
			logger.debug('<<< Exiting cancel()');
            this.props.onCancel();
    }
    render() {
		logger.debug('>>> Entering render()');
        let {state} = this;
		logger.debug('<<< Exiting render()');
        return this.props.visible && <div className="dnn-user-change-password">
            <GridCell className="do-not-close">
                <GridCell>
                    <div className="title">
                        {Localization.get("ChangePassword")}
                    </div>
                    <SingleLineInputWithError label={Localization.get("NewPassword") }
                        error={state.errors.password}
                        onChange={this.onChange.bind(this, "password") }
                        tooltipMessage={Localization.get("NewPassword.Help") }
                        errorMessage={Localization.get("NewPassword.Required") }
                        style={inputStyle}
                        type="password"
                        autoComplete="off"
                        inputStyle={{ marginBottom: 15 }}
                        value={state.changePassword.password}/>
                    <SingleLineInputWithError label={Localization.get("NewConfirm") }
                        error={state.errors.confirmPassword || state.errors.passwordsMatch}
                        onChange={this.onChange.bind(this, "confirmPassword") }
                        tooltipMessage={Localization.get("NewConfirm.Help") }
                        errorMessage={state.errors.confirmPassword ? Localization.get("NewConfirm.Required") : Localization.get("NewConfirmMismatch.ErrorMessage") }
                        style={inputStyle}
                        type="password"
                        inputStyle={{ marginBottom: 15 }}
                        autoComplete="off"
                        value={state.confirmPassword}/>
                </GridCell>
                <GridSystem>
                    <Button className="right do-not-close" id="cancelbtn"  type="neutral" onClick={this.cancel.bind(this) }>{Localization.get("btnCancel") }</Button>
                    <Button id="confirmbtn do-not-close" type="primary" onClick={this.save.bind(this) }>{Localization.get("btnApply") }</Button>
                </GridSystem>
            </GridCell>
        </div >;
    }
}
ChangePassword.propTypes = {
    dispatch: PropTypes.func.isRequired,
    userId: PropTypes.number.isRequired,
    visible: PropTypes.bool,
    onCancel: PropTypes.func
};
ChangePassword.defaultProps = {
    visible: true
};
function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(ChangePassword);