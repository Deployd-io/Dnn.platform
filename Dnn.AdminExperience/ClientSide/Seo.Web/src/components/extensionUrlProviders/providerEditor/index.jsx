import React, { Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
const logger = console;

class ProviderEditor extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

     
    render() {        
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return <div className={styles.providerSettingEditor}>
            <iframe className="edit-provider" seamless src={this.props.settingUrl} />
        </div>;
    }
}

ProviderEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    settingUrl: PropTypes.string,
    Collapse: PropTypes.func
};

export default (ProviderEditor);