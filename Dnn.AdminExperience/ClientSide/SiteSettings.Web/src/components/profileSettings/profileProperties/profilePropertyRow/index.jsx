import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class ProfilePropertyRow extends Component {
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

    isSystemProperty(name) {
        let flag = false;
        let systemProperties = ["lastname", "firstname", "preferredtimezone", "preferredlocale"];
        if (name) {
            if (systemProperties.indexOf(name.toLowerCase()) > -1) {
                flag = true;
            }
        }
        return flag;
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.debug('<<< Exiting render()');
        return (
            <div className={"collapsible-component-properties" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-header-properties " + !opened} >
                    <div className={"row"}>
                        <div title={props.name} className="property-item item-row-name">
                            {props.name}&nbsp; </div>
                        <div className="property-item item-row-dataType">
                            {props.dataType}</div>
                        <div className="property-item item-row-defaultVisibility">
                            {props.defaultVisibility}</div>
                        <div className="property-item item-row-required">
                            {this.getBooleanDisplay(props.required)}</div>
                        <div className="property-item item-row-visible">
                            {this.getBooleanDisplay(props.visible)}</div>
                        <div className="property-item item-row-editButton">
                            <div className={opened ? "order-icon-hidden" : "order-icon"}><SvgIcons.DragRowIcon /></div>
                            {!this.isSystemProperty(props.name) &&
                                <div className={opened ? "delete-icon-hidden" : "delete-icon"} onClick={props.onDelete}><SvgIcons.TrashIcon /></div>
                            }
                            <div className={opened ? "edit-icon-active" : "edit-icon"} onClick={this.toggle.bind(this)}><SvgIcons.EditIcon /></div>
                        </div>
                    </div>
                </div>
                <Collapsible isOpened={opened} autoScroll={true} className="collapsible-body-properties">
                    {opened && props.children}
                </Collapsible>
            </div>
        );
    }
}

ProfilePropertyRow.propTypes = {
    propertyId: PropTypes.number,
    name: PropTypes.string,
    dataType: PropTypes.string,
    defaultVisibility: PropTypes.string,
    required: PropTypes.bool,
    visible: PropTypes.bool,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    onDelete: PropTypes.func,
    onMoveUp: PropTypes.func,
    onMoveDown: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    children: PropTypes.node
};

ProfilePropertyRow.defaultProps = {
    collapsed: true
};
export default (ProfilePropertyRow);
