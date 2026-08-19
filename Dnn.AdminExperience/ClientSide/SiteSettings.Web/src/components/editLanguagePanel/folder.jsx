import React, { Component, } from "react";
import PropTypes from "prop-types";
const logger = console;
import { Collapsible, } from "@dnnsoftware/dnn-react-common";
class Folder extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            isOpen: false
        };
		logger.debug('<<< Exiting constructor()');
    }
    toggleCollapse() {
		logger.debug('>>> Entering toggleCollapse()');
        const { props } = this;
        this.setState({
            isOpen: !this.state.isOpen
        });
        if (!props.children) {
            props.onClick(props.folder.NewValue, () => {
                this.setState({ isOpen: true });
            });
			logger.debug('<<< Exiting toggleCollapse()');
        }
    }
     
    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
        const isFolder = props.folder.NewValue.indexOf(".resx") === -1;
		logger.debug('<<< Exiting render()');
        return (
            <li>
                {isFolder && <div onClick={this.toggleCollapse.bind(this)} className="edit-svg"><isOpenIcon /></div>}
                <div className="resource-type-icon" onClick={this.toggleCollapse.bind(this)}><svgIcon /></div>
                <span className={props.isSelected ? "selected-resource" : ""} onClick={this.toggleCollapse.bind(this)}>{props.folder.Name}</span>
                <Collapsible isOpened={this.state.isOpen}>
                    <ul>{props.children}</ul>
                </Collapsible>
            </li>
        );
    }
}

Folder.propTypes = {
    ChildFolders: PropTypes.array,
    getChildFolders: PropTypes.func,
    folder: PropTypes.object,
    children: PropTypes.node,
    onClick: PropTypes.func,
    isSelected: PropTypes.bool
};
export default Folder;
