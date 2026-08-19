import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import ExtensionHeader from "../common/ExtensionHeader";
import ExtensionDetailRow from "../common/ExtensionDetailRow";
import styles from "./style.module.less";
const logger = console;

class ExtensionList extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;

		logger.debug('<<< Exiting render()');
        return (
            <GridCell className={styles.extensionList} style={{ padding: "5px 20px" }}>
                <ExtensionHeader />
                {props.packages.map((_package, index) => {
                    return <ExtensionDetailRow
                        _package={_package}
                        isHost={props.isHost}
                        onEdit={props.onEdit.bind(this, index)}
                        onDelete={props.onDelete.bind(this, _package, index)}
                        key={index} />;
                })}
            </GridCell>
        );
    }
}

ExtensionList.propTypes = {
    label: PropTypes.string,
    packages: PropTypes.array,
    collapsed: PropTypes.bool,
    onChange: PropTypes.func,
    onEdit: PropTypes.func,
    isHost: PropTypes.bool
};


export default ExtensionList;
