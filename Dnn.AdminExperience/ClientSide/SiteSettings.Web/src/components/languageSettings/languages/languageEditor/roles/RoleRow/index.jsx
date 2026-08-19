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
		logger.debug('>>> Entering constructor()');
        this.state = {
            selected: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const {props} = this;

        this.setState({
            selected: props.selected
        });
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const { props } = this;
        if (props.selected !== prevProps.selected) {
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
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
		logger.debug('>>> Entering render()');
        const {props} = this;

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        rolesList: state.languages.rolesList
    };
}

export default connect(mapStateToProps)(RoleRow);