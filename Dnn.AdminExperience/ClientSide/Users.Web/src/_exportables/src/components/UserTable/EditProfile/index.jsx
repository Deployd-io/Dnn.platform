import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { CommonUsersActions } from "../../../actions";
import "./style.less";
const logger = console;

class EditProfile extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            userDetails: props.userDetails
        };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props);
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }
    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        if (this.props.userDetails === undefined && this.props.userDetails.userId !== this.props.userId) {
            this.getUserDetails(this.props);
			logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
        }
    }
    getUserDetails(props) {
		logger.error(`>>> Entering getUserDetails(props=${props})`, { "event.action": "getUserDetails", "logrite.props": props });
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            });
			logger.warn(`<<< Exiting getUserDetails(props=${props})`, { "event.action": "getUserDetails", "logrite.props": props });
        }));
    }
    render() {
        return <iframe 
            className="edit-profile" seamless
            src={this.state.userDetails !== undefined && this.state.userDetails.editProfileUrl !== undefined ? 
                this.state.userDetails.editProfileUrl : ""}
        />;
    }
}

EditProfile.propTypes = {
    userDetails: PropTypes.object,
    userId:PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(EditProfile);