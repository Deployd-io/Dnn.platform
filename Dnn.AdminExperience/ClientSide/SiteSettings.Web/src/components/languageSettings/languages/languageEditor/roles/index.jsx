import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import resx from "../../../../../resources";
import {
    languages as LanguagesActions
} from "actions";
import RoleRow from "./RoleRow";
const logger = console;
import { Label, GridCell, Dropdown } from "@dnnsoftware/dnn-react-common";

const tableFields = [
    { name: "RoleName", width: 60 },
    { name: "Select", width: 40 }
];

class RolesPanel extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');

        this.state = {
            groupId: -1
        };
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const {props, state} = this;

        if (!props.roleGroups) {
            props.dispatch(LanguagesActions.getRoleGroups(props.portalId));
        }
        props.dispatch(LanguagesActions.getRoles(props.portalId, state.groupId, props.cultureCode));
		logger.debug('<<< Exiting componentDidMount()');
    }

    onRoleGroupChanged(group) {
		logger.debug(`>>> Entering onRoleGroupChanged(group=${group})`);
        const {props} = this;
        let groupId = group.value;
        this.setState({
            groupId: groupId
        });
        props.dispatch(LanguagesActions.getRoles(props.portalId, groupId, props.cultureCode));
		logger.debug(`<<< Exiting onRoleGroupChanged(group=${group})`);
    }

    renderHeader() {
		logger.debug('>>> Entering renderHeader()');
        let tableHeaders = tableFields.map((field, i) => {
            return <GridCell columnSize={field.width} style={{ fontWeight: "bolder" }} key={i}>
                {
                    field.name !== "" ?
                        <span>{resx.get(field.name + ".Header")}</span>
                        : <span>&nbsp; </span>
                }
            </GridCell>;
        });
		logger.debug('<<< Exiting renderHeader()');
        return <div id="header-row" className="header-row">{tableHeaders}</div>;
    }

    renderedRolesList() {
		logger.debug('>>> Entering renderedRolesList()');
        const {props} = this;
        let i = 0;
        if (props.rolesList) {
			logger.debug('<<< Exiting renderedRolesList()');
            return props.rolesList.map((role, index) => {
                let id = "row-" + i++;
                return (
                    <RoleRow
                        roleName={role.RoleName}
                        roleId={role.RoleID}
                        selected={role.Selected}
                        onSelectChange={props.onSelectChange}
                        index={index}
                        key={"role-" + index}
                        id={id}>
                    </RoleRow>
                );
            });
        }
    }

    getRoleGroupOptions() {
		logger.debug('>>> Entering getRoleGroupOptions()');
        const {props} = this;
        let options = [];
        if (props.roleGroups !== undefined) {
            options = props.roleGroups.map((item) => {
                return {
                    label: item.RoleGroupName, value: item.RoleGroupID
                };
            });
            if (options.length > 0) {
                options.unshift({ label: "<" + resx.get("GlobalRoles") + ">", value: -1 });
                options.unshift({ label: "<" + resx.get("AllRoles") + ">", value: -2 });
            }
        }
		logger.debug('<<< Exiting getRoleGroupOptions()');
        return options;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
		logger.debug('<<< Exiting render()');
        return (
            <div className="language-roles-list-container">
                <div className="translator-sectionTitle">
                    <Label
                        tooltipMessage={resx.get("translatorsLabel.Help")}
                        label={resx.get("Translators")}
                    />
                </div>
                {props.roleGroups && props.roleGroups.length > 0 &&
                    <div className="group-filter">
                        <Dropdown
                            value={state.groupId}
                            style={{ width: "150px" }}
                            options={this.getRoleGroupOptions()}
                            withBorder={false}
                            onSelect={this.onRoleGroupChanged.bind(this)}
                        />
                    </div>
                }
                <div className="container">
                    {this.renderHeader()}
                    {this.renderedRolesList()}
                </div>
            </div>
        );
    }
}

RolesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    rolesList: PropTypes.array,
    roleGroups: PropTypes.array,
    cultureCode: PropTypes.string,
    onSelectChange: PropTypes.func,
    portalId: PropTypes.number
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        roleGroups: state.languages.roleGroups,
        rolesList: state.languages.rolesList
    };
}

export default connect(mapStateToProps)(RolesPanel);