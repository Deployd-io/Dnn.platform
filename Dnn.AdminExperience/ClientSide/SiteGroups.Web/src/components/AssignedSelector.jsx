import PropTypes from "prop-types";
import React from "react";
import { Scrollbars } from "react-custom-scrollbars";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import Resx from "localization";
import styles from "./AssignedSelector.module.less";
const logger = console;
class AssignedSelector extends React.Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    getPortalList(list, type) {
		logger.debug(`>>> Entering getPortalList(list=${list},type=${type})`);
		logger.debug(`<<< Exiting getPortalList(list=${list},type=${type})`);
        return list.map((portal, index) => {
            return (
                <li
                    className={portal.selected ? "selected" : ""}
                    key={portal.PortalId.toString()}
                    onClick={() => this.props.onClickOnPortal(index, type)}
                >
                    {portal.PortalName}
                </li>
            );
        });
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const assignedPortals = this.getPortalList(
            this.props.assignedPortals,
            "assignedPortals"
        );
        const unassignedPortals = this.getPortalList(
            this.props.unassignedPortals,
            "unassignedPortals"
        );
        let leftSelected = this.props.unassignedPortals.find(p => p.selected) !== null;
		logger.debug(`render(): leftSelected → ${leftSelected}`);
        let rightSelected = this.props.assignedPortals.find(p => p.selected) !== null;
		logger.debug('<<< Exiting render()');
        return (
            <GridCell className={styles.assignedSelector}>
                <GridCell columnSize={45} className="selector-box">
                    <h6>{Resx.get("EditModule_Unassigned.Label")}</h6>
                    <Scrollbars
                        style={{
                            width: "100%",
                            height: "100%",
                            border: "1px solid #c8c8c8"
                        }}
                    >
                        <ul>{unassignedPortals}</ul>
                    </Scrollbars>
                </GridCell>
                <GridCell columnSize={10} className="selector-controls">
                    <div
                        href=""
                        className={"move-item single-right" + (leftSelected ? " enabled" : "")}
                        onClick={() => this.props.moveItemsRight()}
                    ><SvgIcons.ArrowRightIcon /></div>
                    <div
                        href=""
                        className={"move-item single-left" + (rightSelected ? " enabled" : "")}
                        onClick={() => this.props.moveItemsLeft()}
                    ><SvgIcons.ArrowLeftIcon /></div>
                    <div
                        href=""
                        className={"move-item double-right" + (unassignedPortals.length > 0 ? " enabled" : "")}
                        onClick={() => this.props.moveAll("right")}
                    ><SvgIcons.DoubleArrowRightIcon /></div>
                    <div
                        href=""
                        className={"move-item double-left" + (assignedPortals.length > 0 ? " enabled" : "")}
                        onClick={() => this.props.moveAll("left")}
                    ><SvgIcons.DoubleArrowLeftIcon /></div>
                </GridCell>
                <GridCell columnSize={45} className="selector-box">
                    <h6>{Resx.get("EditModule_Assigned.Label")}</h6>
                    <Scrollbars
                        style={{
                            width: "100%",
                            height: "100%",
                            border: "1px solid #c8c8c8"
                        }}
                    >
                        <ul>{assignedPortals}</ul>
                    </Scrollbars>
                </GridCell>
            </GridCell>
        );
    }
}

AssignedSelector.propTypes = {
    assignedPortals: PropTypes.array,
    unassignedPortals: PropTypes.array,
    onClickOnPortal: PropTypes.func,
    moveItemsRight: PropTypes.func,
    moveItemsLeft: PropTypes.func,
    moveAll: PropTypes.func
};

export default AssignedSelector;
