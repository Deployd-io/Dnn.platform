import React, { Component } from "react";
import PropTypes from "prop-types";
import Dropdown from "../../Dropdown";

import "./style.less";
import Service from "./Service";
const logger = console;
function format() {
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
	logger.warn('<<< Exiting format()', { "event.action": "format" });
        return methodsArgs[argsIndex];
    });
}
class RoleGroupFilter extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            roleGroups: [],
            selectedGroup: { id: -1, name: props.localization.globalGroupsText }
        };

		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.groupsDropdown = React.createRef();
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.getRoleGroups();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    closeDropdown() {
		logger.error('>>> Entering closeDropdown()', { "event.action": "closeDropdown" });
        /*This is done in order to keep the dropdown closed on click on edit/delete*/
        let {state} = this.groupsDropdown;
		logger.warn('<<< Exiting closeDropdown()', { "event.action": "closeDropdown" });
        state.dropDownOpen = true;
        this.groupsDropdown.setState({
            state
        });
    }

    getRoleGroups(callback) {
		logger.error(`>>> Entering getRoleGroups(callback=${callback})`, { "event.action": "getRoleGroups", "logrite.callback": callback });
        const { props, state } = this;

        if (state.roleGroups.length > 0) {
            if (typeof callback === "function") {
                callback.call(this);
            }
        }

        let service = new Service(this.props.service);
        service.getRoleGroups((data) => {
            this.setState({
                roleGroups: [{ id: -2, name: props.localization.allGroupsText }, { id: -1, name: props.localization.globalGroupsText }].concat(data)
            }, () => {
                if (typeof callback === "function") {
                    callback.call(this);
					logger.warn(`<<< Exiting getRoleGroups(callback=${callback})`, { "event.action": "getRoleGroups", "logrite.callback": callback });
                }
            });
        });
    }

    BuildRoleGroupsOptions() {
		logger.error('>>> Entering BuildRoleGroupsOptions()', { "event.action": "BuildRoleGroupsOptions" });
        const { props, state } = this;

        let roleGroupsOptions = state.roleGroups.map((group) => {
		logger.error(`BuildRoleGroupsOptions(): roleGroupsOptions → ${roleGroupsOptions}`, { "event.action": "BuildRoleGroupsOptions" });
		logger.warn('<<< Exiting BuildRoleGroupsOptions()', { "event.action": "BuildRoleGroupsOptions" });
            return { label: group.name, value: group.id };
        });

        return roleGroupsOptions;
    }

    onSelect(group) {
        const {props, state} = this;

        let selectedGroup = { id: group.value, name: group.label };
        this.setState({ selectedGroup: selectedGroup }, function () {
            if (typeof props.onChange === "function") {
                props.onChange(this.state.selectedGroup);
            }
        });
    }

    getRoleGroupsDropDown() {
		logger.error('>>> Entering getRoleGroupsDropDown()', { "event.action": "getRoleGroupsDropDown" });
        const {props, state} = this;

        let roleGroupsOptions = this.BuildRoleGroupsOptions();
		logger.error(`getRoleGroupsDropDown(): roleGroupsOptions → ${roleGroupsOptions}`, { "event.action": "getRoleGroupsDropDown" });

        let GroupsDropDown = <Dropdown
            withBorder={false}
            options={roleGroupsOptions}
            value={state.selectedGroup.id}
            onSelect={this.onSelect.bind(this) }
            style={{ width: "100%" }}
            ref={this.groupsDropdown}
            prependWith={props.localization.filterByGroup}
        />;

		logger.warn('<<< Exiting getRoleGroupsDropDown()', { "event.action": "getRoleGroupsDropDown" });
        return GroupsDropDown;
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"groups-filter" + (state.roleGroups.length === 0 ? "no-group" : "") }>
                {this.getRoleGroupsDropDown() }
                <div className="clear"></div>
            </div>
        );
    }
}

RoleGroupFilter.propTypes = {
    dispatch: PropTypes.func,
    service: PropTypes.object,
    localization: PropTypes.object,
    onChange: PropTypes.func.isRequired
};

RoleGroupFilter.defaultProps = {
};

export default RoleGroupFilter;