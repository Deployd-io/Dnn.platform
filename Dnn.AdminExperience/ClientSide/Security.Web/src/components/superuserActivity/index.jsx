import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

class SuperuserActivityPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;
        if (props.activities) {
            return;
        }
        props.dispatch(SecurityActions.getSuperuserActivities());
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    renderHeader() {
		logger.error('>>> Entering renderHeader()', { "event.action": "renderHeader" });
        const tableFields = [
            { "name": resx.get("Username"), "id": "Username" },
            { "name": resx.get("CreatedDate"), "id": "CreatedDate" },
            { "name": resx.get("LastLogin"), "id": "LastLogin" },
            { "name": resx.get("LastActivityDate"), "id": "LastActivityDate" }
        ];
        let tableHeaders = tableFields.map((field, i) => {
            let className = "activityHeader activityHeader-" + field.id;
            return <div className={className} key={i}>
                <span>{field.name}</span>
            </div>;
        });

		logger.error(`renderHeader(): tableHeaders → ${tableHeaders}`, { "event.action": "renderHeader" });
		logger.warn('<<< Exiting renderHeader()', { "event.action": "renderHeader" });
        return <div className="activityHeader-wrapper">{tableHeaders}</div>;
    }

    renderedList() {
		logger.error('>>> Entering renderedList()', { "event.action": "renderedList" });
        const {props} = this;
		logger.warn('<<< Exiting renderedList()', { "event.action": "renderedList" });
        return props.activities.map((term, i) => {
            return (
                <div className="activityItem" key={i}>
                    <div className="label-username">
                        <div className="label-wrapper">
                            <span>{term.Username}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-createddate">
                        <div className="label-wrapper">
                            <span>{term.CreatedDate}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-lastlogindate">
                        <div className="label-wrapper">
                            <span>{term.LastLoginDate}&nbsp; </span>
                        </div>
                    </div>
                    <div className="label-lastactivitydate">
                        <div className="label-wrapper">
                            <span>{term.LastActivityDate}&nbsp; </span>
                        </div>
                    </div>
                </div>
            );
        });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        if (props.activities) {
            return (
                <div className={styles.activities}>
                    <div className="activities-topbar">
                        {resx.get("SuperUserActivityExplaination")}
                    </div>
                    <div className="activityItems">
                        {this.renderHeader()}
                        {this.renderedList()}
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SuperuserActivityPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    activities: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        activities: state.security.activities
    };
}

export default connect(mapStateToProps)(SuperuserActivityPanelBody);