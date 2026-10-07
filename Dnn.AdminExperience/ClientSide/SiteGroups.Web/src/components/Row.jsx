import PropTypes from "prop-types";
import React from "react";
import Resx from "../localization";
import {
    GridCell,
    Collapsible as Collapse,
    IconButton
} from "@dnnsoftware/dnn-react-common";
import SiteGroupEditor from "./Editor";
import "./Row.less";
const logger = console;

export default class SiteGroupRow extends React.Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            PortalGroupName: props.group.PortalGroupName || "",
            AuthenticationDomain: props.group.AuthenticationDomain,
            Description: props.group.Description,
            Portals: JSON.parse(JSON.stringify(props.group.Portals)) || [],
            UnassignedSites: JSON.parse(
                JSON.stringify(props.unassignedSites || [])
            ).filter(
                site => site.PortalId !== this.props.group.MasterPortal.PortalId
            ),
            errors: {
                groupName: false
            }
        };
        this.submitted = false;
		logger.debug(`constructor(props=${props}): this.submitted → ${this.submitted}`, { "event.action": "constructor", "logrite.props": props });
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
        this.resetState(newProps);
		logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.newProps": newProps });
    }

    clickOnPortal(index, type) {
		logger.error(`>>> Entering clickOnPortal(index=${index},type=${type})`, { "event.action": "clickOnPortal", "logrite.index": index, "logrite.type": type });
        if (type === "assignedPortals") {
            let p = JSON.parse(JSON.stringify(this.state.Portals));
            p[index].selected = !p[index].selected;
            this.setState({ Portals: p });
        } else {
            let p = JSON.parse(JSON.stringify(this.state.UnassignedSites));
			logger.error(`clickOnPortal(index=${index},type=${type}): p → ${p}`, { "event.action": "clickOnPortal", "logrite.index": index, "logrite.type": type });
            p[index].selected = !p[index].selected;
            this.setState({ UnassignedSites: p });
			logger.warn(`<<< Exiting clickOnPortal(index=${index},type=${type})`, { "event.action": "clickOnPortal", "logrite.index": index, "logrite.type": type });
        }
    }

    moveItemsLeft() {
		logger.error('>>> Entering moveItemsLeft()', { "event.action": "moveItemsLeft" });
        let assignedPortals = JSON.parse(JSON.stringify(this.state.Portals));
        let unassignedPortals = JSON.parse(
            JSON.stringify(this.state.UnassignedSites)
        ).filter(site => site.PortalId !== this.props.group.MasterPortal.PortalId);
		logger.error(`moveItemsLeft(): assignedPortals → ${assignedPortals}`, { "event.action": "moveItemsLeft" });
        let itemsToStay = [],
            itemsToMove = [];
        let selectedCount = 0;
        assignedPortals.forEach(portal => {
            let { selected } = portal;
            delete portal.selected;
            if (selected) {
                selectedCount++;
                itemsToMove.push(portal);
            } else {
                itemsToStay.push(portal);
            }
        });
        if (selectedCount > 0) {
			logger.warn('<<< Exiting moveItemsLeft()', { "event.action": "moveItemsLeft" });
            this.setState({
                UnassignedSites: unassignedPortals.concat(itemsToMove),
                Portals: itemsToStay
            });
        }
    }

    moveItemsRight() {
		logger.error('>>> Entering moveItemsRight()', { "event.action": "moveItemsRight" });
        let assignedPortals = JSON.parse(JSON.stringify(this.state.Portals));
        let unassignedPortals = JSON.parse(
            JSON.stringify(this.state.UnassignedSites)
        );
        let itemsToStay = [],
            itemsToMove = [];
			logger.error(`moveItemsRight(): assignedPortals → ${assignedPortals}`, { "event.action": "moveItemsRight" });
        let selectedCount = 0;
        unassignedPortals.forEach(portal => {
            let { selected } = portal;
            delete portal.selected;
            if (selected) {
                selectedCount++;
                itemsToMove.push(portal);
            } else {
                itemsToStay.push(portal);
            }
        });
        if (selectedCount > 0) {
			logger.warn('<<< Exiting moveItemsRight()', { "event.action": "moveItemsRight" });
            this.setState({
                UnassignedSites: itemsToStay,
                Portals: assignedPortals.concat(itemsToMove)
            });
        }
    }

    moveAll(direction) {
		logger.error(`>>> Entering moveAll(direction=${direction})`, { "event.action": "moveAll", "logrite.direction": direction });
        let assignedPortals = JSON.parse(JSON.stringify(this.state.Portals));
        let unassignedPortals = JSON.parse(
            JSON.stringify(this.state.UnassignedSites)
        );
        switch (direction) {
            case "right":
                this.setState({
                    UnassignedSites: [],
                    Portals: assignedPortals.concat(unassignedPortals)
                });

                break;
            default:
                this.setState({
                    UnassignedSites: unassignedPortals.concat(assignedPortals),
                    Portals: []
                });
				logger.warn(`<<< Exiting moveAll(direction=${direction})`, { "event.action": "moveAll", "logrite.direction": direction });
                break;
        }
    }

    sortPortals(a, b) {
		logger.error(`>>> Entering sortPortals(a=${a},b=${b})`, { "event.action": "sortPortals", "logrite.a": a, "logrite.b": b });
		logger.warn(`<<< Exiting sortPortals(a=${a},b=${b})`, { "event.action": "sortPortals", "logrite.a": a, "logrite.b": b });
        return a.PortalName < b.PortalName ? -1 : 1;
    }

    isNew() {
		logger.error('>>> Entering isNew()', { "event.action": "isNew" });
		logger.warn('<<< Exiting isNew()', { "event.action": "isNew" });
        return this.props.group.PortalGroupId === -1;
    }

    save() {
		logger.error('>>> Entering save()', { "event.action": "save" });
        this.submitted = true;
        if (this.isValid()) this.props.onSave(this.result());
		logger.warn('<<< Exiting save()', { "event.action": "save" });
    }

    cancel() {
		logger.error('>>> Entering cancel()', { "event.action": "cancel" });
        this.resetState(this.props);
        this.props.onCancelEditing();
		logger.warn('<<< Exiting cancel()', { "event.action": "cancel" });
    }

    resetState(props) {
		logger.error(`>>> Entering resetState(props=${props})`, { "event.action": "resetState", "logrite.props": props });
        this.setState({
            PortalGroupName: props.group.PortalGroupName || "",
            AuthenticationDomain: props.group.AuthenticationDomain,
            Description: props.group.Description,
            Portals: JSON.parse(JSON.stringify(props.group.Portals)),
            UnassignedSites: JSON.parse(JSON.stringify(props.unassignedSites)),
            errors: {
                groupName: false,
                authenticationDomain: false
            }
        });
		logger.warn(`<<< Exiting resetState(props=${props})`, { "event.action": "resetState", "logrite.props": props });
    }

    isValid() {
        let valid = true;
        if (this.submitted) {
            let { PortalGroupName } = this.state;
            let { errors } = this.state;
            errors.groupName = false;
            if (PortalGroupName === "") {
                errors.groupName = true;
                valid = false;
            }
            this.setState({ errors });
        }
        return valid;
    }

    result() {
		logger.error('>>> Entering result()', { "event.action": "result" });
		logger.warn('<<< Exiting result()', { "event.action": "result" });
        return {
            PortalGroup: {
                PortalGroupId: this.props.group.PortalGroupId,
                AuthenticationDomain: this.state.AuthenticationDomain,
                PortalGroupName: this.state.PortalGroupName,
                MasterPortal: this.props.group.MasterPortal,
                Portals: this.state.Portals,
                Description: this.state.Description
            },
            UnassignedSites: this.state.UnassignedSites
        };
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"row " + this.props.isOpened}>
                <div className="rowHeader">
                    <GridCell columnSize={45}>
                        {this.props.group.PortalGroupName}
                    </GridCell>
                    <GridCell columnSize={45}>
                        {this.props.group.MasterPortal.PortalName}
                    </GridCell>
                    <GridCell columnSize={10}>
                        <IconButton
                            type="Edit"
                            className={"edit-icon " + !this.props.isOpened}
                            onClick={() =>
                                this.props.isOpened
                                    ? this.props.onCancelEditing()
                                    : this.props.onEditGroup(this.props.group)
                            }
                            title={Resx.get("Edit.Button")}
                        />
                    </GridCell>
                </div>
                <Collapse
                    isOpened={this.props.isOpened}
                >
                    {this.props.isOpened && (
                        <SiteGroupEditor
                            group={this.props.group}
                            portalGroupName={this.state.PortalGroupName}
                            authenticationDomain={this.state.AuthenticationDomain}
                            description={this.state.Description}
                            errors={this.state.errors}
                            unassignedSites={this.state.UnassignedSites}
                            portals={this.state.Portals}
                            isNew={this.isNew()}
                            onCancel={() => this.cancel()}
                            onDeleteGroup={group => this.props.onDeleteGroup(group)}
                            onSave={() => this.save()}
                            onAuthenticationDomainChanged={value => {
                                this.setState({ AuthenticationDomain: value });
                            }}
                            onDescriptionChanged={value => {
                                this.setState({ Description: value });
                            }}
                            onGroupNameChanged={value => {
                                this.setState({ PortalGroupName: value });
                                this.isValid();
                            }}
                            onClickOnPortal={(i, t) => this.clickOnPortal(i, t)}
                            onMoveItemsLeft={() => this.moveItemsLeft()}
                            onMoveItemsRight={() => this.moveItemsRight()}
                            onMoveAll={d => this.moveAll(d)}
                        />
                    )}
                </Collapse>
            </div>
        );
    }
}

SiteGroupRow.propTypes = {
    unassignedSites: PropTypes.array,
    group: PropTypes.object,
    onDeleteGroup: PropTypes.func,
    onSave: PropTypes.func,
    onEditGroup: PropTypes.func,
    onCancelEditing: PropTypes.func,
    isOpened: PropTypes.bool
};
