import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import StatusSwitch from "./StatusSwitch";
import IconButton from "../IconButton";
const logger = console;

class GridRow extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);

        this.state = {
        };
        this.columnHeight = 40;
		logger.debug(`constructor(props=${props}): this.columnHeight → ${this.columnHeight}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.columnHeight = this.node.offsetHeight;
		logger.debug('<<< Exiting componentDidMount()');
    }   

    getHeaderColumnText() {
		logger.debug('>>> Entering getHeaderColumnText()');
        const {props} = this;

		logger.debug('<<< Exiting getHeaderColumnText()');
        return props.type === "role" ? props.permission.roleName : props.permission.displayName;
    }

    onStatusChanged(def, status) {
		logger.debug(`>>> Entering onStatusChanged(def=${def},status=${status})`);
        const {props} = this;

        let permission = Object.assign({}, props.permission);
		logger.warn(`onStatusChanged(def=${def},status=${status}): permission → ${permission}`);
        permission.permissions = Object.assign([], props.permission.permissions.filter(p => {
            return p.permissionId !== def.permissionId;
        }));

        if (status > 0) {
            permission.permissions.push(Object.assign({}, def, { allowAccess: status === 1 }));
        }

        this.fixPermissionsStatus(permission, def, status);

        if (typeof props.onChange === "function") {
            props.onChange(permission);
			logger.debug(`<<< Exiting onStatusChanged(def=${def},status=${status})`);
        }
    }

    onDelete() {
		logger.debug('>>> Entering onDelete()');
        const {props} = this;

        if (typeof props.onDeletePermisson === "function") {
            props.onDeletePermisson(props.permission);
			logger.debug('<<< Exiting onDelete()');
        }
    }

    fixPermissionsStatus(permission, def, status) {
		logger.debug(`>>> Entering fixPermissionsStatus(permission=${permission},def=${def},status=${status})`);
        const {props} = this;

        if (def.fullControl) {
            if (status === 0) {
                permission.permissions = [];
            } else {
                props.definitions.forEach(function (d) {
                    let id = d.permissionId;
                    let p = permission.permissions.filter(function (c) {
						logger.debug('>>> Entering filter()');
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
                    if (permissionName !== "navigate" && permissionName !== "browse") {
                        props.definitions.forEach(function (d) {
                            if (!d.view) {
                                return;
                            }

                            let id = d.permissionId;
                            let p = permission.permissions.filter(function (c) {
								logger.debug(`>>> Entering filter(p=${p})`);
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
                        return false;
                    }

                    let p = permission.permissions.filter(function (c) {
                        return c.permissionId === d.permissionId;
                    });

                    if (!p.length) {
                        return true;
                    }

                    let allowAccess = p[0].allowAccess;

					logger.debug(`<<< Exiting filter(p=${p})`);
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
		logger.debug(`<<< Exiting fixPermissionsStatus(permission=${permission},def=${def},status=${status})`);
    }

    renderRow() {
		logger.debug('>>> Entering renderRow()');
        const {props} = this;
        const {roleColumnWidth, columnWidth, actionsWidth} = props;
        let self = this;
       
        return (
            <tr className="grid-row" ref={node => this.node = node}>
                <td columnSize={roleColumnWidth}><span title={this.getHeaderColumnText() }>{this.getHeaderColumnText() }</span></td>
                {props.definitions.map(function (def) {
					logger.debug('<<< Exiting renderRow()');
                    let permission = props.permission.permissions.filter(p => {
                        return p.permissionId === def.permissionId;
                    });

                    let status = 0;
					logger.debug(`map(permission=${permission}): status → ${status}`);

                    if (props.permission.locked) {
                        status = 3;
                    }
                    else {
                        status = permission.length > 0 ? (permission[0].allowAccess ? 1 : 2) : 0;
                    }

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
		logger.debug('>>> Entering render()');
        const {props, state} = this;

		logger.debug('<<< Exiting render()');
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