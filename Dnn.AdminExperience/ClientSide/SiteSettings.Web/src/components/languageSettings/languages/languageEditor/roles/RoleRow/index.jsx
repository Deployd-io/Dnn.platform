import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;
import { GridCell, Checkbox } from "@dnnsoftware/dnn-react-common";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "actions";

class RoleRow extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            selected: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {props} = this;

        this.setState({
            selected: props.selected
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (props.selected !== prevProps.selected) {
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            this.setState({
                selected: props.selected
            });
        }
    }

    onChange(role, event) {
        let {props} = this;
        this.setState({
            selected: event
        });
        props.dispatch(LanguagesActions.SelectLanguageRoles(props.rolesList, role, event));
        props.onSelectChange(role, event);
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"collapsible-component-language-roles"}>
                <div className={"collapsible-header-language-roles "} >
                    <GridCell title={props.roleName} columnSize={60} >
                        {props.roleName}</GridCell>
                    <GridCell columnSize={40} >
                        <Checkbox
                            style={{ float: "left" }}
                            value={props.selected}
                            onChange={this.onChange.bind(this, props.roleName) } /></GridCell>
                </div>
            </div>
        );
    }
}

RoleRow.propTypes = {
    dispatch: PropTypes.func.isRequired,
    roleName: PropTypes.string,
    roleId: PropTypes.number,
    selected: PropTypes.bool,
    onSelectChange: PropTypes.func,
    id: PropTypes.string,
    rolesList: PropTypes.array
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        rolesList: state.languages.rolesList
    };
}

export default connect(mapStateToProps)(RoleRow);