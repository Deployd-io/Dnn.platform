import React, { Component } from "react";
import PropTypes from "prop-types";
import SearchIcon from "./img/search.svg";
import FileIcon from "./img/pages.svg";
const logger = console;
import { Scrollbars } from "react-custom-scrollbars";

export default class FilePicker extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            showFilePicker: false,
            searchFileText: ""
        };
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
        this.setState({ showFilePicker: false, searchFileText: "" });
		logger.debug('<<< Exiting hide()');
    }

    onFileNameClick(file) {
		logger.debug(`>>> Entering onFileNameClick(file=${file})`);
        this.hide();
        this.props.onFileClick(file);
		logger.debug(`<<< Exiting onFileNameClick(file=${file})`);
    }

    onChangeSearchFileText(e) {
		logger.debug(`>>> Entering onChangeSearchFileText(e=${e})`);
        const searchFileText = e.target.value;
        this.setState({ searchFileText });
		logger.debug(`<<< Exiting onChangeSearchFileText(e=${e})`);
    }

    clearSearch() {
		logger.debug('>>> Entering clearSearch()');
        this.setState({ searchFileText: "" });
		logger.debug('<<< Exiting clearSearch()');
    }

    onFilesClick() {
		logger.debug('>>> Entering onFilesClick()');
        const {showFilePicker} = this.state;
        if (!showFilePicker) {
            if (!this.props.files) {
                this.props.getFiles();
            }
        }
        this.setState({ showFilePicker: !showFilePicker });
		logger.debug('<<< Exiting onFilesClick()');
    }

    isMatchSearch(fileName) {
		logger.debug(`>>> Entering isMatchSearch(fileName=${fileName})`);
        if (!this.state.searchFileText) {
            return true;
        }
        const name = fileName.toLowerCase();
		logger.debug(`isMatchSearch(fileName=${fileName}): name → ${name}`);
		logger.debug(`<<< Exiting isMatchSearch(fileName=${fileName})`);
        return name.indexOf(this.state.searchFileText) !== -1;
    }

    getItem(isMatchSearch, child) {
		logger.debug(`>>> Entering getItem(isMatchSearch=${isMatchSearch},child=${child})`);
        if (isMatchSearch) {
             
            return <li>
                <div className="icon" onClick={this.onFileNameClick.bind(this, child) }><FileIcon /></div>
                <div className="item-name" onClick={this.onFileNameClick.bind(this, child) }>{child.data.value}</div>
            </li>;
        }
		logger.debug(`<<< Exiting getItem(isMatchSearch=${isMatchSearch},child=${child})`);
        return false;
    }

    getFiles() {
		logger.debug('>>> Entering getFiles()');
        if (!this.props.files) {
            return false;
        }

        const files = this.props.files.map((child) => {
            const isMatchSearch = this.isMatchSearch(child.data.value);
		logger.debug('<<< Exiting getFiles()');
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
		logger.debug('>>> Entering render()');
         
        const selectedFileText = this.props.selectedFile ? this.props.selectedFile.value : this.props.notSpecifiedText;

        const files = this.getFiles();
		logger.debug(`render(): files → ${files}`);

		logger.debug('<<< Exiting render()');
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
