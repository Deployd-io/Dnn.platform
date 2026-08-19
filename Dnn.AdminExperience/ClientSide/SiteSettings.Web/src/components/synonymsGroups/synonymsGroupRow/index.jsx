import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class SynonymsGroupRow extends Component {
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
		logger.debug('<<< Exiting componentDidMount()');
    }

    toggle() {
		logger.debug('>>> Entering toggle()');
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id);
			logger.debug('<<< Exiting toggle()');
        }
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.debug('<<< Exiting render()');
        return (
            <div className={"collapsible-component-synonyms" + (opened ? " row-opened" : "") + (this.props.id === "add" ? " row-new-item" : "")}>
                {props.visible &&
                    <div className={"collapsible-header-synonyms " + !opened} >
                        <div className={"row"}>
                            <div className="synonyms-item item-row-tags">
                                {props.tags}</div>
                            <div className="synonyms-item item-row-editButton">
                                <div className={opened ? "delete-icon-hidden" : "delete-icon"} onClick={props.onDelete.bind(this)}><SvgIcons.TrashIcon /></div>
                                <div className={opened ? "edit-icon-active" : "edit-icon"} onClick={this.toggle.bind(this)}><SvgIcons.EditIcon /></div>
                            </div>
                        </div>
                    </div>
                }
                <Collapsible className="synonyms-editor-wrapper" isOpened={opened}>{opened && props.children}</Collapsible>
            </div>
        );
    }
}

SynonymsGroupRow.propTypes = {
    groupId: PropTypes.number,
    tags: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool,
    children: PropTypes.node
};

SynonymsGroupRow.defaultProps = {
    collapsed: true
};
export default (SynonymsGroupRow);
