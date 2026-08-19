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
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const {props} = this;
        if (props.activities) {
            return;
        }
        props.dispatch(SecurityActions.getSuperuserActivities());
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    renderHeader() {
		logger.debug('>>> Entering renderHeader()');
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

		logger.debug(`renderHeader(): tableHeaders → ${tableHeaders}`);
		logger.debug('<<< Exiting renderHeader()');
        return <div className="activityHeader-wrapper">{tableHeaders}</div>;
    }

    renderedList() {
		logger.debug('>>> Entering renderedList()');
        const {props} = this;
		logger.debug('<<< Exiting renderedList()');
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
		logger.debug('>>> Entering render()');
        const {props} = this;
        if (props.activities) {
			logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        tabIndex: state.pagination.tabIndex,
        activities: state.security.activities
    };
}

export default connect(mapStateToProps)(SuperuserActivityPanelBody);