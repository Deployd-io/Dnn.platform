import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import StatusSwitch from "./StatusSwitch";
import IconButton from "../IconButton";
const logger = console;

class GridRow extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });

        this.state = {
        };
        this.columnHeight = 40;
		logger.debug(`constructor(props=${props}): this.columnHeight → ${this.columnHeight}`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.columnHeight = this.node.offsetHeight;
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }   

    getHeaderColumnText() {
		logger.error('>>> Entering getHeaderColumnText()', { "event.action": "getHeaderColumnText" });
        const {props} = this;

		logger.warn('<<< Exiting getHeaderColumnText()', { "event.action": "getHeaderColumnText" });
        return props.type === "role" ? props.permission.roleName : props.permission.displayName;
    }

    onStatusChanged(def, status) {
		logger.error(`>>> Entering onStatusChanged(def=${def},status=${status})`, { "event.action": "onStatusChanged", "logrite.def": def, "logrite.status": status });
        const {props} = this;

        let permission = Object.assign({}, props.permission);
		logger.error(`onStatusChanged(def=${def},status=${status}): permission → ${permission}`, { "event.action": "onStatusChanged", "logrite.def": def, "logrite.status": status });
        permission.permissions = Object.assign([], props.permission.permissions.filter(p => {
            return p.permissionId !== def.permissionId;
        }));

        if (status > 0) {
            permission.permissions.push(Object.assign({}, def, { allowAccess: status === 1 }));
        }

        this.fixPermissionsStatus(permission, def, status);

        if (typeof props.onChange === "function") {
            props.onChange(permission);
			logger.warn(`<<< Exiting onStatusChanged(def=${def},status=${status})`, { "event.action": "onStatusChanged", "logrite.def": def, "logrite.status": status });
        }
    }

    onDelete() {
		logger.error('>>> Entering onDelete()', { "event.action": "onDelete" });
        const {props} = this;

        if (typeof props.onDeletePermisson === "function") {
            props.onDeletePermisson(props.permission);
			logger.warn('<<< Exiting onDelete()', { "event.action": "onDelete" });
        }
    }

    fixPermissionsStatus(permission, def, status) {
		logger.error(`>>> Entering fixPermissionsStatus(permission=${permission},def=${def},status=${status})`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
        const {props} = this;

        if (def.fullControl) {
            if (status === 0) {
                permission.permissions = [];
            } else {
                props.definitions.forEach(function (d) {
                    let id = d.permissionId;
                    let p = permission.permissions.filter(function (c) {
                        return c.permissionId === id;
                    });

                    if (p.length) {
                        p[0].allowAccess = status === 1;
                    } else {
                        permission.permissions.push(Object.assign({}, d, { allowAccess: status === 1 }));
                    }
                });
            }

        } else {
            //Check if View Permission is not allow, then also set other permission
            if (def.view) {
                if (status !== 1) {
                    if (status === 0) {
                        permission.permissions = [];
                    } else {
                        props.definitions.forEach(function (d) {
                            let id = d.permissionId;
                            let p = permission.permissions.filter(function (c) {
                                return c.permissionId === id;
                            });

                            if (p.length) {
                                p[0].allowAccess = status === 1;
                            } else {
                                permission.permissions.push(Object.assign({}, d, { allowAccess: status === 1 }));
                            }
                        });
                    }
                }
            } else {
                //if other permissions are set to true must have View
                if (status === 1) {
                    let permissionName = def.permissionName.toLowerCase();
					logger.error(`fixPermissionsStatus(permission=${permission},def=${def},status=${status}): permissionName → ${permissionName}`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
                    if (permissionName !== "navigate" && permissionName !== "browse") {
                        props.definitions.forEach(function (d) {
                            if (!d.view) {
                                return;
                            }

                            let id = d.permissionId;
							logger.error(`fixPermissionsStatus(permission=${permission},def=${def},status=${status}): id → ${id}`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
                            let p = permission.permissions.filter(function (c) {
                                return c.permissionId === id;
                            });

                            if (p.length) {
                                p[0].allowAccess = true;
                            } else {
                                permission.permissions.push(Object.assign({}, d, { allowAccess: true }));
                            }
                        });
                    }
                }
            }

            if (status > 0) {
                let samePermissionsSet = props.definitions.filter(function (d) {
                    if (d.fullControl) {
						logger.error(`fixPermissionsStatus(permission=${permission},def=${def},status=${status}): samePermissionsSet → ${samePermissionsSet}`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
                        return false;
                    }

                    let p = permission.permissions.filter(function (c) {
                        return c.permissionId === d.permissionId;
                    });

                    if (!p.length) {
						logger.error(`fixPermissionsStatus(permission=${permission},def=${def},status=${status}): p → ${p}`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
                        return true;
                    }

                    let allowAccess = p[0].allowAccess;

				logger.debug(`fixPermissionsStatus(permission=${permission},def=${def},status=${status}): permission.permissions → ${permission.permissions}`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
                    return (!allowAccess && status === 1) || (allowAccess && status === 2);
                }).length === 0;

                if (samePermissionsSet) {
                    let d = props.definitions.filter(function (d) {
                        return d.fullControl;
                    });

                    if (d.length) {
                        let id = d[0].permissionId;
                        let p = permission.permissions.filter(function (c) {
                            return c.permissionId === id;
                        });

                        if (p.length) {
                            p[0].allowAccess = status === 1;
                        } else {
                            permission.permissions.push(Object.assign({}, d[0], { allowAccess: status === 1 }));
                        }
                    }
                }
            }
        }
		logger.warn(`<<< Exiting fixPermissionsStatus(permission=${permission},def=${def},status=${status})`, { "event.action": "fixPermissionsStatus", "logrite.permission": permission, "logrite.def": def, "logrite.status": status });
    }

    renderRow() {
		logger.error('>>> Entering renderRow()', { "event.action": "renderRow" });
        const {props} = this;
        const {roleColumnWidth, columnWidth, actionsWidth} = props;
        let self = this;
       
        return (
            <tr className="grid-row" ref={node => this.node = node}>
                <td columnSize={roleColumnWidth}><span title={this.getHeaderColumnText() }>{this.getHeaderColumnText() }</span></td>
                {props.definitions.map(function (def) {
                    let permission = props.permission.permissions.filter(p => {
                        return p.permissionId === def.permissionId;
                    });

                    let status = 0;

                    if (props.permission.locked) {
                        status = 3;
                    }
                    else {
                        status = permission.length > 0 ? (permission[0].allowAccess ? 1 : 2) : 0;
                    }

					logger.warn('<<< Exiting renderRow()', { "event.action": "renderRow" });
                    return (
                        
                        <td style={{"height":self.columnHeight+"px"}} key={def.permissionId} >
                            <StatusSwitch permission={permission} status={status} onChange={self.onStatusChanged.bind(self, def) } />
                        </td>
                    );
                }) }
                <td style={{"height":self.columnHeight+"px"}} className="col-actions">
                    {!props.permission.default && <IconButton type="trash" onClick={this.onDelete.bind(this) } />}
                </td>
            </tr>
        );
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            this.renderRow()
        );
    }
}

GridRow.propTypes = {
    localization: PropTypes.object,
    definitions: PropTypes.object.isRequired,
    permission: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"]),
    roleColumnWidth: PropTypes.number.isRequired,
    columnWidth: PropTypes.number.isRequired,
    actionsWidth: PropTypes.number.isRequired
};

GridRow.DefaultProps = {
};

export default GridRow;