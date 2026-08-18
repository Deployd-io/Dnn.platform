import React, { Component } from "react";
import PropTypes from "prop-types";
import Folders from "./Folders";
import { Scrollbars } from "react-custom-scrollbars";
import SearchIconImage from "./img/search.svg";
import "./style.less";
const logger = console;

export default class FolderSelector extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            showFolderPicker: false,
            searchFolderText: ""
        };
        this.timeOut = null;
		logger.debug(`constructor(): this.state → ${this.state}`);
        this.handleClick = this.handleClick.bind(this);
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    handleClick(e) {
		logger.debug(`>>> Entering handleClick(e=${e})`);
        if (!this._isMounted) { return; }
        if (this.node && this.node.contains(e.target) || e.target.className === "clear-button") {
            return;
        }
        this.hide();
		logger.debug(`<<< Exiting handleClick(e=${e})`);
    }

    hide() {
		logger.debug('>>> Entering hide()');
        this.setState({ showFolderPicker: false});
		logger.debug('<<< Exiting hide()');
    }

    onFolderChange(folder) {
		logger.debug(`>>> Entering onFolderChange(folder=${folder})`);
        this.hide();
        this.props.onFolderChange(folder);
		logger.debug(`<<< Exiting onFolderChange(folder=${folder})`);
    }

    onChangeSearchFolderText(e) {
		logger.debug(`>>> Entering onChangeSearchFolderText(e=${e})`);
        const searchFolderText = e.target.value ? e.target.value : "";
        this.setState({ searchFolderText });
		logger.debug(`onChangeSearchFolderText(e=${e}): searchFolderText → ${searchFolderText}`);
        clearTimeout(this.timeOut);
        this.timeOut = setTimeout(() => {this.props.searchFolder(searchFolderText.toLowerCase());}, 500);
		logger.debug(`<<< Exiting onChangeSearchFolderText(e=${e})`);
    }

    clearSearch(e) {
		logger.debug(`>>> Entering clearSearch(e=${e})`);
        e.preventDefault();
        this.setState({ searchFolderText: "" });
        this.props.searchFolder();
		logger.debug(`<<< Exiting clearSearch(e=${e})`);
    }   

    onFoldersClick() {
		logger.debug('>>> Entering onFoldersClick()');
        const {showFolderPicker} = this.state;
        this.setState({ showFolderPicker: !showFolderPicker });
		logger.debug('<<< Exiting onFoldersClick()');
    }

    getSearchIcon() {
        return <div className="search-icon"><SearchIconImage /></div>;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {selectedFolder, folders, onParentExpands, noFolderSelectedValue, searchFolderPlaceHolder} = this.props;
        const selectedFolderText = selectedFolder ? selectedFolder.value : "<" + noFolderSelectedValue + ">";
		logger.debug(`render(): selectedFolderText → ${selectedFolderText}`);
        const searchIcon = this.getSearchIcon();

		logger.debug('<<< Exiting render()');
        return ( 
            <div className="dnn-folder-selector" ref={node => this.node = node}>
                <div className="selected-item" onClick={this.onFoldersClick.bind(this) }>
                    {selectedFolderText}
                </div>
                <div className={"folder-selector-container" + (this.state.showFolderPicker ? " show" : "") } >
                    <div className="inner-box">
                        <div className="search">
                            <input 
                                type="text" 
                                value={this.state.searchFolderText} 
                                onChange={this.onChangeSearchFolderText.bind(this) } 
                                placeholder={searchFolderPlaceHolder}
                                aria-label="Search" />
                            {this.state.searchFolderText && 
                                <div onClick={this.clearSearch.bind(this)} className="clear-button">×</div>
                            }
                            {searchIcon}
                        </div>
                        <div className="items">
                            <Scrollbars className="scrollArea content-vertical"
                                autoHeight
                                autoHeightMin={0}
                                autoHeightMax={200}>
                                <Folders
                                    folders={folders}
                                    onParentExpands={onParentExpands}
                                    onFolderChange={this.onFolderChange.bind(this) }/>
                            </Scrollbars>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}


FolderSelector.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderChange: PropTypes.func.isRequired,
    onParentExpands: PropTypes.func.isRequired,
    selectedFolder: PropTypes.object,
    searchFolder: PropTypes.func.isRequired,
    noFolderSelectedValue: PropTypes.string.isRequired,
    searchFolderPlaceHolder: PropTypes.string.isRequired
};