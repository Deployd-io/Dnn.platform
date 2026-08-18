import React, { Component } from "react";
import PropTypes from "prop-types";
import FolderIcon from "./img/folder.svg";
const logger = console;

export default class Folders extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            openFolders: []
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const { props } = this;
        if (!props.folders || !props.folders.children || !props.folders.children[0] || !props.folders.children[0].data || !props.folders.children[0].data.key) {
            return;
        }
        const rootKey = props.folders.children[0].data.key;
        let {openFolders} = this.state;
        openFolders.push(rootKey);
        if (this.state.openFolders !== openFolders) {
            this.setState({ openFolders });
        }
		logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
    }

    toggleFolder(key) {
		logger.debug(`>>> Entering toggleFolder(key=${key})`);
        let {openFolders} = this.state;
        if (openFolders.some(id => id === key)) {
            openFolders = openFolders.filter(id => id !== key);
        } else {
            openFolders.push(key);
        }
        this.setState({ openFolders });
		logger.debug(`<<< Exiting toggleFolder(key=${key})`);
    }

    onParentClick(item) {
		logger.debug(`>>> Entering onParentClick(item=${item})`);
        if (!item.data.hasChildren) {
            return;
        }
        this.toggleFolder(item.data.key);
        if (item.children && item.children.length) {
            return;
        }
        this.props.getChildren(item.data.key);
		logger.debug(`<<< Exiting onParentClick(item=${item})`);
    }

    onFolderNameClick(folder) {
		logger.debug(`>>> Entering onFolderNameClick(folder=${folder})`);
        this.props.onFolderClick(folder.data);
		logger.debug(`<<< Exiting onFolderNameClick(folder=${folder})`);
    }

    getFolders(folder) {
		logger.debug(`>>> Entering getFolders(folder=${folder})`);
        if (!folder) {
            return false;
        }
        const children = folder.children.map((child) => {
             
            const isOpen = this.state.openFolders.some(id => id === child.data.key);
			logger.debug(`getFolders(folder=${folder}): isOpen → ${isOpen}`);
            const className = isOpen ? "open" : "";
            return <li className={className} key={child.data.key}>
                {child.data.hasChildren && <div className="has-children" onClick={this.onParentClick.bind(this, child) }></div>}
                <div onClick={this.onFolderNameClick.bind(this, child)}>
                    <div className="icon"><FolderIcon /></div>
                    <div className="item-name">{child.data.value}</div>
                </div>
                {child.data.hasChildren && this.getFolders(child) }
            </li>;
        });
		logger.debug(`<<< Exiting getFolders(folder=${folder})`);
        return <ul>{children}</ul>;
    }

    render() {
		logger.debug('>>> Entering render()');
         
        const folders = this.getFolders(this.props.folders);
		logger.debug('<<< Exiting render()');
        return <div className="item-picker">
            {folders}
        </div>;
    }
}


Folders.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderClick: PropTypes.func.isRequired,
    getChildren: PropTypes.func.isRequired
};

