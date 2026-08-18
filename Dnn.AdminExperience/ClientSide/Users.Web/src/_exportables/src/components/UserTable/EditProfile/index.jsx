import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import { CommonUsersActions } from "../../../actions";
import "./style.less";
const logger = console;

class EditProfile extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            userDetails: props.userDetails
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let {props} = this;
        if (props.userDetails === undefined || props.userDetails.userId !== props.userId) {
            this.getUserDetails(props);
			logger.debug('<<< Exiting componentDidMount()');
        }
    }
    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        if (this.props.userDetails === undefined && this.props.userDetails.userId !== this.props.userId) {
            this.getUserDetails(this.props);
			logger.debug('<<< Exiting componentDidUpdate()');
        }
    }
    getUserDetails(props) {
		logger.debug(`>>> Entering getUserDetails(props=${props})`);
        props.dispatch(CommonUsersActions.getUserDetails({ userId: props.userId }, (data) => {
            let userDetails = Object.assign({}, data);
            this.setState({
                userDetails
            });
			logger.debug(`<<< Exiting getUserDetails(props=${props})`);
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        userDetails: state.users.userDetails
    };
}

export default connect(mapStateToProps)(EditProfile);