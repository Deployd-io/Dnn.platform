import React, { Component } from "react";
import PropTypes from "prop-types";
import SearchIcon from "./img/search.svg";
import FileIcon from "./img/pages.svg";
const logger = console;
import { Scrollbars } from "react-custom-scrollbars";

export default class FilePicker extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            showFilePicker: false,
            searchFileText: ""
        };
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
        this.setState({ showFilePicker: false, searchFileText: "" });
		logger.warn('<<< Exiting hide()', { "event.action": "hide" });
    }

    onFileNameClick(file) {
		logger.error(`>>> Entering onFileNameClick(file=${file})`, { "event.action": "onFileNameClick", "logrite.file": file });
        this.hide();
        this.props.onFileClick(file);
		logger.warn(`<<< Exiting onFileNameClick(file=${file})`, { "event.action": "onFileNameClick", "logrite.file": file });
    }

    onChangeSearchFileText(e) {
		logger.error(`>>> Entering onChangeSearchFileText(e=${e})`, { "event.action": "onChangeSearchFileText", "logrite.e": e });
        const searchFileText = e.target.value;
        this.setState({ searchFileText });
		logger.warn(`<<< Exiting onChangeSearchFileText(e=${e})`, { "event.action": "onChangeSearchFileText", "logrite.e": e });
    }

    clearSearch() {
		logger.error('>>> Entering clearSearch()', { "event.action": "clearSearch" });
        this.setState({ searchFileText: "" });
		logger.warn('<<< Exiting clearSearch()', { "event.action": "clearSearch" });
    }

    onFilesClick() {
		logger.error('>>> Entering onFilesClick()', { "event.action": "onFilesClick" });
        const {showFilePicker} = this.state;
        if (!showFilePicker) {
            if (!this.props.files) {
                this.props.getFiles();
            }
        }
        this.setState({ showFilePicker: !showFilePicker });
		logger.warn('<<< Exiting onFilesClick()', { "event.action": "onFilesClick" });
    }

    isMatchSearch(fileName) {
		logger.error(`>>> Entering isMatchSearch(fileName=${fileName})`, { "event.action": "isMatchSearch", "logrite.fileName": fileName });
        if (!this.state.searchFileText) {
            return true;
        }
        const name = fileName.toLowerCase();
		logger.error(`isMatchSearch(fileName=${fileName}): name → ${name}`, { "event.action": "isMatchSearch", "logrite.fileName": fileName });
        return name.indexOf(this.state.searchFileText) !== -1;
    }

    getItem(isMatchSearch, child) {
		logger.error(`>>> Entering getItem(isMatchSearch=${isMatchSearch},child=${child})`, { "event.action": "getItem", "logrite.isMatchSearch": isMatchSearch, "logrite.child": child });
        if (isMatchSearch) {
             
            return <li>
                <div className="icon" onClick={this.onFileNameClick.bind(this, child) }><FileIcon /></div>
                <div className="item-name" onClick={this.onFileNameClick.bind(this, child) }>{child.data.value}</div>
            </li>;
        }
		logger.warn(`<<< Exiting getItem(isMatchSearch=${isMatchSearch},child=${child})`, { "event.action": "getItem", "logrite.isMatchSearch": isMatchSearch, "logrite.child": child });
        return false;
    }

    getFiles() {
		logger.error('>>> Entering getFiles()', { "event.action": "getFiles" });
        if (!this.props.files) {
            return false;
        }

        const files = this.props.files.map((child) => {
            const isMatchSearch = this.isMatchSearch(child.data.value);
		logger.warn('<<< Exiting getFiles()', { "event.action": "getFiles" });
            return this.getItem(isMatchSearch, child);
        });
        return <ul>
            <li>
                <div className="icon" onClick={this.onFileNameClick.bind(this) }><FileIcon /></div>
                <div className="item-name none-specified" onClick={this.onFileNameClick.bind(this) }>{this.props.notSpecifiedText}</div>
            </li>
            {files}
        </ul>;
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        const selectedFileText = this.props.selectedFile ? this.props.selectedFile.value : this.props.notSpecifiedText;

        const files = this.getFiles();
		logger.error(`render(): files → ${files}`, { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="drop-down" ref={node => this.node = node}>
            <div className="selected-item" onClick={this.onFilesClick.bind(this) }>
                {selectedFileText}
            </div>
            <div className={"item-picker-container" + (this.state.showFilePicker ? " show" : "") } >
                <div className="inner-box">
                    <div className="search">
                        <input type="text" value={this.state.searchFileText} onChange={this.onChangeSearchFileText.bind(this) } placeholder={this.props.searchFilesPlaceHolderText} aria-label="Search" />
                        {this.state.searchFileText && <div onClick={this.clearSearch.bind(this) } className="clear-button">×</div>}
                        <div className="search-icon"><SearchIcon /></div>
                    </div>
                    <div className="items">
                        <Scrollbars className="scrollArea content-vertical"
                            autoHeight
                            autoHeightMin={0}
                            autoHeightMax={200}>
                            <div className="item-picker">
                                {files}
                            </div>
                        </Scrollbars>
                    </div>
                </div>
            </div>
        </div>;
    }
}

FilePicker.propTypes = {
    files: PropTypes.object.isRequired,
    onFileClick: PropTypes.func.isRequired,
    selectedFile: PropTypes.object.isRequired,
    getFiles: PropTypes.func.isRequired,

    notSpecifiedText: PropTypes.string,
    searchFilesPlaceHolderText: PropTypes.string
};
