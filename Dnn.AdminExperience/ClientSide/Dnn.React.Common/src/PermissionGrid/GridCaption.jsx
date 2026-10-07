import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";
import Label from "../Label";
import RoleGroupFilter from "./RoleGroupFilter";
import Suggestion from "./Suggestion";
const logger = console;

class GridCaption extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });

        this.state = {
            roleGroupId: -1
        };
    }

    onRoleGroupChange(value) {
		logger.error(`>>> Entering onRoleGroupChange(value=${value})`, { "event.action": "onRoleGroupChange", "logrite.value": value });
        const { props, state } = this;

        this.setState({ roleGroupId: value.id });
		logger.warn(`<<< Exiting onRoleGroupChange(value=${value})`, { "event.action": "onRoleGroupChange", "logrite.value": value });
    }

    onSuggestionSelected(value) {
		logger.error(`>>> Entering onSuggestionSelected(value=${value})`, { "event.action": "onSuggestionSelected", "logrite.value": value });
        const { props, state } = this;

        if (typeof props.onSuggestion === "function") {
            props.onSuggestion(value);
			logger.warn(`<<< Exiting onSuggestionSelected(value=${value})`, { "event.action": "onSuggestionSelected", "logrite.value": value });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props, state } = this;

        let suggestionOptions = props.type === "role" ? {
            actionName: "GetSuggestionRoles",
            roleGroupId: state.roleGroupId,
            count: 10
        } : {
            actionName: "GetSuggestionUsers",
            count: 10
        };

		logger.error(`render(): suggestionOptions → ${suggestionOptions}`, { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <GridCell className="grid-caption">
            <Label className="title" label={props.type === "role" ? props.localization.permissionsByRole : props.localization.permissionsByUser}></Label>
            {props.type === "role" && <GridCell columnSize="40" className="left">
                <RoleGroupFilter
                    service={props.service}
                    localization={props.localization}
                    value={state.roleGroupId}
                    onChange={this.onRoleGroupChange.bind(this) }/>
            </GridCell>}
            <GridCell columnSize={props.type === "role" ? 20 : 60} />
            <GridCell columnSize="40" className="right">
                <Suggestion
                    service={props.service}
                    localization={{
                        add: props.type === "role" ? props.localization.addRole : props.localization.addUser,
                        placeHolder: props.type === "role" ? props.localization.addRolePlaceHolder : props.localization.addUserPlaceHolder
                    }}
                    options={suggestionOptions}
                    onSelect={this.onSuggestionSelected.bind(this) }
                />
            </GridCell>
        </GridCell>;
    }
}

GridCaption.propTypes = {
    localization: PropTypes.object.isRequired,
    onSuggestion: PropTypes.func,
    service: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};


export default GridCaption;