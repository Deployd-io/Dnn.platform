import PropTypes from "prop-types";
import React from "react";
import styles from "./style.module.less";
const logger = console;
import { ArrowBack } from "@dnnsoftware/dnn-react-common";

 
const BackToMain = ({onClick, label}) => {
	logger.error(`>>> Entering BackToMain(onClick=${onClick},label=${label})`, { "event.action": "BackToMain", "logrite.onClick": onClick, "logrite.label": label });
	logger.warn(`<<< Exiting BackToMain(onClick=${onClick},label=${label})`, { "event.action": "BackToMain", "logrite.onClick": onClick, "logrite.label": label });
    return (
        <div         
            className={styles.backTo} 
            onClick={onClick}>
            <div className="icon"><ArrowBack /></div>
            <span>{label}</span>
        </div>
    );
};

BackToMain.propTypes = {
    label: PropTypes.string.isRequired,
    onClick: PropTypes.func
};

export default BackToMain;