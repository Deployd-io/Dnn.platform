import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class SiteAliasRow extends Component {
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
        }
        else {
            this.props.OpenCollapse(this.props.id);
			logger.debug('<<< Exiting toggle()');
        }
    }

     
    getBooleanDisplay(prop) {
		logger.debug(`>>> Entering getBooleanDisplay(prop=${prop})`);
        if (this.props.id !== "add") {
            if (prop) {
                return <div className="checkMarkIcon"><SvgIcons.CheckMarkIcon /></div>;
            }
            else return <span>&nbsp; </span>;
        }
        else return <span>-</span>;
		logger.debug(`<<< Exiting getBooleanDisplay(prop=${prop})`);
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.debug('<<< Exiting render()');
        return (
            <div className={"collapsible-component-alias" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-alias " + !opened} >
                    <div className={"row"}>
                        <div title={props.name} className="alias-item item-row-alias">
                            {props.alias}</div>
                        <div className="alias-item item-row-browser">
                            {props.browser}</div>
                        <div className="alias-item item-row-theme">
                            {props.skin}&nbsp;</div>
                        {props.showLanguageColumn &&
                            <div className="alias-item item-row-language">
                                {props.language}&nbsp;</div>
                        }
                        <div className="alias-item item-row-primary">
                            {this.getBooleanDisplay(props.isPrimary)}</div>
                        <div className="alias-item item-row-actionButtons">
                            {props.deletable &&
                                <div className={opened ? "delete-icon-hidden" : "delete-icon"} onClick={props.onDelete.bind(this)}><SvgIcons.TrashIcon /></div>
                            }
                            {props.editable &&
                                <div className={opened ? "edit-icon-active" : "edit-icon"} onClick={this.toggle.bind(this)}><SvgIcons.EditIcon /></div>
                            }
                        </div>
                    </div>
                </div>
                <Collapsible isOpened={opened} className="collapsible-alias-body">{opened && props.children}</Collapsible>
            </div>
        );
    }
}

SiteAliasRow.propTypes = {
    aliasId: PropTypes.number,
    alias: PropTypes.string,
    browser: PropTypes.string,
    skin: PropTypes.string,
    language: PropTypes.string,
    isPrimary: PropTypes.bool,
    deletable: PropTypes.bool,
    editable: PropTypes.bool,
    showLanguageColumn: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    children: PropTypes.node,
    name: PropTypes.string
};

SiteAliasRow.defaultProps = {
    collapsed: true
};
export default (SiteAliasRow);
