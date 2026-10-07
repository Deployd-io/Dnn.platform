import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import "react-widgets/styles.css";
import "./style.less";
import resx from "../../../resources";
import { GridCell, SearchBox, Dropdown, IconButton }  from "@dnnsoftware/dnn-react-common";
import RoleGroupEditor from "../RoleEditor/RoleGroupEditor";
import {
    roles as RolesActions
} from "../../../actions";
import util from "utils";
const logger = console;

let canEdit = false;
class FiltersBar extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            selectedGroup: {
                label: resx.get("GlobalRolesGroup"),
                value: -1
            },
            showPopup: false
        };
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.EDIT;
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }
    onDeleteGroup(e) {
		logger.error(`>>> Entering onDeleteGroup(e=${e})`, { "event.action": "onDeleteGroup", "logrite.e": e });
        e.stopPropagation();

        const {props} = this;
        this.closeDropDown();
        util.utilities.confirm(resx.get("DeleteRoleGroup.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
            props.dispatch(RolesActions.deleteRoleGroup(this.getCurrentGroup(), () => {
                util.utilities.notify(resx.get("DeleteRoleGroup.Message"));
                let {selectedGroup} = this.state;
                selectedGroup.value = -1;
                selectedGroup.label = resx.get("GlobalRolesGroup");
                this.setState({
                    selectedGroup
                });
                this.props.onRoleGroupChanged(selectedGroup);
            }));
        }, () => {
        });
		logger.warn(`<<< Exiting onDeleteGroup(e=${e})`, { "event.action": "onDeleteGroup", "logrite.e": e });
    }

    onSave(group) {
		logger.error(`>>> Entering onSave(group=${group})`, { "event.action": "onSave", "logrite.group": group });
        this.updateSelectedGroup({ label: group.name, value: group.id });
        let {showPopup} = this.state;
		logger.warn(`<<< Exiting onSave(group=${group})`, { "event.action": "onSave", "logrite.group": group });
        showPopup = !showPopup;
        this.setState({
            showPopup
        });
    }
    closeDropDown() {
		logger.error('>>> Entering closeDropDown()', { "event.action": "closeDropDown" });
        /*This is done in order to keep the dropdown closed on click on edit/delete*/
        let {state} = this.groupsDropdownRef;
		logger.debug(`closeDropDown(): state.dropDownOpen → ${state.dropDownOpen}`, { "event.action": "closeDropDown" });
        if (state) {
			logger.warn('<<< Exiting closeDropDown()', { "event.action": "closeDropDown" });
            state.dropDownOpen = false;
            this.groupsDropdownRef.setState({
                state
            });
        }
    }
    onSelect(option) {
        this.updateSelectedGroup(option);
        this.props.onRoleGroupChanged(option);
    }
    toggleEditGroup() {
		logger.error('>>> Entering toggleEditGroup()', { "event.action": "toggleEditGroup" });
        let {showPopup} = this.state;
        showPopup = !showPopup;
        this.setState({
            showPopup
        }, () => {
            this.closeDropDown();
        });
		logger.warn('<<< Exiting toggleEditGroup()', { "event.action": "toggleEditGroup" });
    }
    updateSelectedGroup(option) {
		logger.error(`>>> Entering updateSelectedGroup(option=${option})`, { "event.action": "updateSelectedGroup", "logrite.option": option });
        let { label} = option;
        let { value} = option;
        let {selectedGroup} = this.state;
        selectedGroup.label = label;
        selectedGroup.value = value;
		logger.warn(`<<< Exiting updateSelectedGroup(option=${option})`, { "event.action": "updateSelectedGroup", "logrite.option": option });
        this.setState({
            selectedGroup
        });
    }

    getCurrentLabel() {
		logger.error('>>> Entering getCurrentLabel()', { "event.action": "getCurrentLabel" });
        let {props} = this;
        let selectedGroup = Object.assign({}, this.state.selectedGroup);
		logger.error(`getCurrentLabel(): selectedGroup → ${selectedGroup}`, { "event.action": "getCurrentLabel" });
        let value = selectedGroup.value;
        let label = selectedGroup.label;
        /*eslint-disable no-unused-vars*/
        let group = { id: value, name: label };
        if (props.roleGroups.some(group => group.id === this.state.selectedGroup.value)) {
            group = Object.assign({}, props.roleGroups.filter(group => group.id === value)[0]);
        }
        if (value > -1) {
            if (canEdit) {
                label = <div className="group-actions">{label}
                    <div className="role-group-actions">
                        <IconButton type="edit" onClick={this.toggleEditGroup.bind(this) } />
                        {this.props.DeleteAllowed && <IconButton type="trash" onClick={this.onDeleteGroup.bind(this) } />}
                    </div>
                </div>;
            }
        }
		logger.error(`getCurrentLabel(): group → ${group}`, { "event.action": "getCurrentLabel" });
        return label;
    }
    getCurrentGroup() {
		logger.error('>>> Entering getCurrentGroup()', { "event.action": "getCurrentGroup" });
        let {props} = this;
        let selectedGroup = Object.assign({}, this.state.selectedGroup);
		logger.error(`getCurrentGroup(): selectedGroup → ${selectedGroup}`, { "event.action": "getCurrentGroup" });
        let value = selectedGroup.value;
        let label = selectedGroup.label;
        let group = { id: value, name: label };
        if (props.roleGroups.some(group => group.id === this.state.selectedGroup.value)) {
            group = Object.assign({}, props.roleGroups.filter(group => group.id === value)[0]);
        }
		logger.error(`getCurrentGroup(): group → ${group}`, { "event.action": "getCurrentGroup" });
        return group;
    }


    getRoleGroupsDropDown() {
		logger.error('>>> Entering getRoleGroupsDropDown()', { "event.action": "getRoleGroupsDropDown" });
        let label = this.getCurrentLabel();
        let roleGroupsOptions = this.BuildRoleGroupsOptions();
		logger.error(`getRoleGroupsDropDown(): label → ${label}`, { "event.action": "getRoleGroupsDropDown" });
        let GroupsDropDown = <Dropdown  style={{ width: "100%" }}
            withBorder={false}
            options={roleGroupsOptions}
            label={label }
            onSelect={this.onSelect.bind(this) }
            ref={node => this.groupsDropdownRef = node}
        />;
		logger.warn('<<< Exiting getRoleGroupsDropDown()', { "event.action": "getRoleGroupsDropDown" });
        return <div className="groups-filter">{GroupsDropDown}<div className="clear"></div></div>;
    }

    BuildRoleGroupsOptions() {
        let {roleGroups} = this.props;
        return roleGroups.map((group) => {
            return { label: group.name, value: group.id };
        });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="filter-container">
            <GridCell columnSize={35} >
                {this.props.roleGroups.length > 0 && this.getRoleGroupsDropDown() }
                {
                    this.state.showPopup &&
                    <div className="edit-group-popup">
                        <RoleGroupEditor
                            onSave={this.onSave.bind(this) }
                            onClick={this.closeDropDown.bind(this) }
                            onCancel={this.toggleEditGroup.bind(this) }
                            title="Edit Group" group={this.getCurrentGroup() } />
                    </div>
                }
            </GridCell>
            <GridCell columnSize={30} >
                <div>&nbsp; </div></GridCell>
            <GridCell columnSize={35} >
                <div className="search-filter">
                    <SearchBox placeholder={resx.get("SearchPlaceHolder") } onSearch={this.props.onKeywordChanged.bind(this) } maxLength={50} />
                    <div className="clear"></div>
                </div>
            </GridCell>
        </div>;
    }
}
FiltersBar.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onRoleGroupChanged: PropTypes.func.isRequired,
    onKeywordChanged: PropTypes.func.isRequired,
    roleGroups: PropTypes.array.isRequired,
    DeleteAllowed: PropTypes.bool
};
function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        roleGroups: state.roles.roleGroups
    };
}
export default connect(mapStateToProps)(FiltersBar);