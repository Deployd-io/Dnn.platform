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
		logger.debug(`>>> Entering constructor(props=${props})`);
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
		logger.debug(`constructor(props=${props}): this.submitted → ${this.submitted}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    UNSAFE_componentWillReceiveProps(newProps) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(newProps=${newProps})`);
        this.resetState(newProps);
		logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(newProps=${newProps})`);
    }

    clickOnPortal(index, type) {
		logger.debug(`>>> Entering clickOnPortal(index=${index},type=${type})`);
        if (type === "assignedPortals") {
            let p = JSON.parse(JSON.stringify(this.state.Portals));
            p[index].selected = !p[index].selected;
            this.setState({ Portals: p });
        } else {
            let p = JSON.parse(JSON.stringify(this.state.UnassignedSites));
			logger.debug(`clickOnPortal(index=${index},type=${type}): p → ${p}`);
            p[index].selected = !p[index].selected;
            this.setState({ UnassignedSites: p });
			logger.debug(`<<< Exiting clickOnPortal(index=${index},type=${type})`);
        }
    }

    moveItemsLeft() {
		logger.debug('>>> Entering moveItemsLeft()');
        let assignedPortals = JSON.parse(JSON.stringify(this.state.Portals));
        let unassignedPortals = JSON.parse(
            JSON.stringify(this.state.UnassignedSites)
        ).filter(site => site.PortalId !== this.props.group.MasterPortal.PortalId);
		logger.debug(`moveItemsLeft(): assignedPortals → ${assignedPortals}`);
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
			logger.debug('<<< Exiting moveItemsLeft()');
            this.setState({
                UnassignedSites: unassignedPortals.concat(itemsToMove),
                Portals: itemsToStay
            });
        }
    }

    moveItemsRight() {
		logger.debug('>>> Entering moveItemsRight()');
        let assignedPortals = JSON.parse(JSON.stringify(this.state.Portals));
        let unassignedPortals = JSON.parse(
            JSON.stringify(this.state.UnassignedSites)
        );
        let itemsToStay = [],
            itemsToMove = [];
			logger.debug(`moveItemsRight(): assignedPortals → ${assignedPortals}`);
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
			logger.debug('<<< Exiting moveItemsRight()');
            this.setState({
                UnassignedSites: itemsToStay,
                Portals: assignedPortals.concat(itemsToMove)
            });
        }
    }

    moveAll(direction) {
		logger.debug(`>>> Entering moveAll(direction=${direction})`);
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
				logger.debug(`<<< Exiting moveAll(direction=${direction})`);
                break;
        }
    }

    sortPortals(a, b) {
		logger.debug(`>>> Entering sortPortals(a=${a},b=${b})`);
		logger.debug(`<<< Exiting sortPortals(a=${a},b=${b})`);
        return a.PortalName < b.PortalName ? -1 : 1;
    }

    isNew() {
		logger.debug('>>> Entering isNew()');
		logger.debug('<<< Exiting isNew()');
        return this.props.group.PortalGroupId === -1;
    }

    save() {
		logger.debug('>>> Entering save()');
        this.submitted = true;
        if (this.isValid()) this.props.onSave(this.result());
		logger.debug('<<< Exiting save()');
    }

    cancel() {
		logger.debug('>>> Entering cancel()');
        this.resetState(this.props);
        this.props.onCancelEditing();
		logger.debug('<<< Exiting cancel()');
    }

    resetState(props) {
		logger.debug(`>>> Entering resetState(props=${props})`);
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
		logger.debug(`<<< Exiting resetState(props=${props})`);
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
		logger.debug('>>> Entering result()');
		logger.debug('<<< Exiting result()');
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
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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
