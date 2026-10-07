import React, { Component } from "react";
import PropTypes from "prop-types";
import Folders from "./Folders";
import { Scrollbars } from "react-custom-scrollbars";
import SearchIcon from "./img/search.svg";
const logger = console;

export default class FolderPicker extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            showFolderPicker: false,
            searchFolderText: ""
        };
        this.timeOut = null;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
        this.handleClick = this.handleClick.bind(this);
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`, { "event.action": "componentWillUnmount" });
    }

    handleClick(e) {
		logger.error(`>>> Entering handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
        if (!this._isMounted) { return; }
        if (this.node && this.node.contains(e.target) || e.target.className === "clear-button") {
            return;
        }
        this.hide();
		logger.warn(`<<< Exiting handleClick(e=${e})`, { "event.action": "handleClick", "logrite.e": e });
    }

    hide() {
		logger.error('>>> Entering hide()', { "event.action": "hide" });
        this.setState({ showFolderPicker: false});
		logger.warn('<<< Exiting hide()', { "event.action": "hide" });
    }

    onFolderClick(folder) {
		logger.error(`>>> Entering onFolderClick(folder=${folder})`, { "event.action": "onFolderClick", "logrite.folder": folder });
        this.hide();
        this.props.onFolderClick(folder);
		logger.warn(`<<< Exiting onFolderClick(folder=${folder})`, { "event.action": "onFolderClick", "logrite.folder": folder });
    }

    onChangeSearchFolderText(e) {
		logger.error(`>>> Entering onChangeSearchFolderText(e=${e})`, { "event.action": "onChangeSearchFolderText", "logrite.e": e });
        const searchFolderText = e.target.value ? e.target.value : "";
        this.setState({ searchFolderText });
		logger.error(`onChangeSearchFolderText(e=${e}): searchFolderText → ${searchFolderText}`, { "event.action": "onChangeSearchFolderText", "logrite.e": e });
        clearTimeout(this.timeOut);
        this.timeOut = setTimeout(() => {this.props.searchFolder(searchFolderText.toLowerCase());}, 500);
		logger.warn(`<<< Exiting onChangeSearchFolderText(e=${e})`, { "event.action": "onChangeSearchFolderText", "logrite.e": e });
    }

    clearSearch(e) {
		logger.error(`>>> Entering clearSearch(e=${e})`, { "event.action": "clearSearch", "logrite.e": e });
        e.preventDefault();
        this.setState({ searchFolderText: "" });
        this.props.searchFolder();
		logger.warn(`<<< Exiting clearSearch(e=${e})`, { "event.action": "clearSearch", "logrite.e": e });
    }   

    onFoldersClick() {
		logger.error('>>> Entering onFoldersClick()', { "event.action": "onFoldersClick" });
        const {showFolderPicker} = this.state;
        this.setState({ showFolderPicker: !showFolderPicker });
		logger.warn('<<< Exiting onFoldersClick()', { "event.action": "onFoldersClick" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        const selectedFolderText = this.props.selectedFolder ? this.props.selectedFolder.value : this.props.notSpecifiedText;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="drop-down" ref={node => this.node = node}>
            <div className="selected-item" onClick={this.onFoldersClick.bind(this) }>
                {selectedFolderText}
            </div>
            <div className={"item-picker-container" + (this.state.showFolderPicker ? " show" : "") } >
                <div className="inner-box">
                    <div className="search">
                        <input type="text" value={this.state.searchFolderText} onChange={this.onChangeSearchFolderText.bind(this) } placeholder={this.props.searchFoldersPlaceHolderText} aria-label="Search" />
                        {this.state.searchFolderText && <div onClick={this.clearSearch.bind(this)} className="clear-button">×</div>}
                        <div className="search-icon"><SearchIcon /></div>
                    </div>
                    <div className="items">
                        <Scrollbars className="scrollArea content-vertical"
                            autoHeight
                            autoHeightMin={0}
                            autoHeightMax={200}>
                            <Folders
                                folders={this.props.folders}
                                getChildren={this.props.getChildren}
                                onFolderClick={this.onFolderClick.bind(this) }/>
                        </Scrollbars>
                    </div>
                </div>
            </div>
        </div>;
    }
}


FolderPicker.propTypes = {
    folders: PropTypes.object.isRequired,
    onFolderClick: PropTypes.func.isRequired,
    getChildren: PropTypes.func.isRequired,
    selectedFolder: PropTypes.object.isRequired,
    searchFolder: PropTypes.func.isRequired,

    notSpecifiedText: PropTypes.string,
    searchFoldersPlaceHolderText: PropTypes.string
};
