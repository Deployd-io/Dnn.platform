import React, { Component } from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
const logger = console;

class ProviderEditor extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

     
    render() {        
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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