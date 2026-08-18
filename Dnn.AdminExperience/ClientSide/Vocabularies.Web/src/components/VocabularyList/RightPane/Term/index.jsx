import React, { Component  } from "react";
import PropTypes from "prop-types";
import Collapsible from "react-collapse";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import util from "utils";
import Html from "../../../Html";
import styles from "./style.module.less";
const logger = console;


class Term extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            isOpened: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    toggleTerm(event) {
		logger.debug(`>>> Entering toggleTerm(event=${event})`);
        if (event) {
            event.preventDefault();
			logger.debug(`<<< Exiting toggleTerm(event=${event})`);
        }
        this.setState({
            isOpened: !this.state.isOpened
        });
    }

    onLiClick(event) {
		logger.debug(`>>> Entering onLiClick(event=${event})`);
        event.preventDefault();
        const {props} = this;
        if (props.isEditable) {
            this.toggleTerm();
        } else {
            this.onClick();
			logger.debug(`<<< Exiting onLiClick(event=${event})`);
        }
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`);
        if (props.closeAll) {
            this.setState({
                isOpened: false
            });
			logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`);
        }
    }

    onClick() {
		logger.debug('>>> Entering onClick()');
        const {props} = this;
        props.onClick(props.term);
		logger.debug('<<< Exiting onClick()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        const className = styles.termLi + (props.children.length > 0 ? " has-children" : "") + (state.isOpened ? " opened" : "") + (props.term.selected ? " selected" : "");

         
		logger.debug('<<< Exiting render()');
        return (
            <li className={className}>
                <div>
                    {props.children.length > 0 && !state.isOpened &&
                        <div className="edit-svg" onClick={this.toggleTerm.bind(this)}><SvgIcons.ArrowRightIcon /></div>
                    }
                    {props.children.length > 0 && state.isOpened &&
                        <div className="edit-svg" onClick={this.toggleTerm.bind(this)}><SvgIcons.ArrowDownIcon /></div>
                    }
                    <div onClick={this.onLiClick.bind(this)}>
                        <span className="term-name"><Html html={props.term.Name } /></span>
                        {props.isEditable && util.canEdit() && <div className="edit-button" onClick={this.onClick.bind(this)}><SvgIcons.EditIcon /></div>}
                    </div>
                </div>
                <Collapsible isOpened={state.isOpened}>{props.children}</Collapsible>
            </li>
        );
    }
}

Term.propTypes = {
    term: PropTypes.object,
    children: PropTypes.node,
    isEditable: PropTypes.bool,
    onClick: PropTypes.func,
    closeAll: PropTypes.bool
};

export default Term;