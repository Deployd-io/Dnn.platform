import React, { Component } from "react";
import PropTypes from "prop-types";
import Dropdown from "../../Dropdown";

import "./style.less";
import Service from "./Service";
const logger = console;
function format() {
	logger.debug('>>> Entering format()');
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
		logger.debug(`<<< Exiting function(argsIndex=${argsIndex})`);
	logger.debug('<<< Exiting format()');
        return methodsArgs[argsIndex];
    });
}
class RoleGroupFilter extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            roleGroups: [],
            selectedGroup: { id: -1, name: props.localization.globalGroupsText }
        };

		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
        this.groupsDropdown = React.createRef();
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.getRoleGroups();
		logger.debug('<<< Exiting componentDidMount()');
    }

    closeDropdown() {
		logger.debug('>>> Entering closeDropdown()');
        /*This is done in order to keep the dropdown closed on click on edit/delete*/
        let {state} = this.groupsDropdown;
		logger.debug('<<< Exiting closeDropdown()');
        state.dropDownOpen = true;
        this.groupsDropdown.setState({
            state
        });
    }

    getRoleGroups(callback) {
		logger.debug(`>>> Entering getRoleGroups(callback=${callback})`);
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
					logger.debug(`<<< Exiting getRoleGroups(callback=${callback})`);
                }
            });
        });
    }

    BuildRoleGroupsOptions() {
		logger.debug('>>> Entering BuildRoleGroupsOptions()');
        const { props, state } = this;

        let roleGroupsOptions = state.roleGroups.map((group) => {
		logger.debug(`BuildRoleGroupsOptions(): roleGroupsOptions → ${roleGroupsOptions}`);
		logger.debug('<<< Exiting BuildRoleGroupsOptions()');
            return { label: group.name, value: group.id };
        });

        return roleGroupsOptions;
    }

    onSelect(group) {
        const {props, state} = this;

        let selectedGroup = { id: group.value, name: group.label };
        this.setState({ selectedGroup: selectedGroup }, function () {
			logger.debug(`>>> Entering setState(selectedGroup=${selectedGroup})`);
            if (typeof props.onChange === "function") {
                props.onChange(this.state.selectedGroup);
				logger.debug(`<<< Exiting setState(selectedGroup=${selectedGroup})`);
            }
        });
    }

    getRoleGroupsDropDown() {
		logger.debug('>>> Entering getRoleGroupsDropDown()');
        const {props, state} = this;

        let roleGroupsOptions = this.BuildRoleGroupsOptions();
		logger.debug(`getRoleGroupsDropDown(): roleGroupsOptions → ${roleGroupsOptions}`);

        let GroupsDropDown = <Dropdown
            withBorder={false}
            options={roleGroupsOptions}
            value={state.selectedGroup.id}
            onSelect={this.onSelect.bind(this) }
            style={{ width: "100%" }}
            ref={this.groupsDropdown}
            prependWith={props.localization.filterByGroup}
        />;

		logger.debug('<<< Exiting getRoleGroupsDropDown()');
        return GroupsDropDown;
    }


    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;

		logger.debug('<<< Exiting render()');
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