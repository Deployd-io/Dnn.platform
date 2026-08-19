import React, { Component } from "react";
import PropTypes from "prop-types";
import Folders from "./Folders";
import { Scrollbars } from "react-custom-scrollbars";
import SearchIcon from "./img/search.svg";
const logger = console;

export default class FolderPicker extends Component {

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

    onFolderClick(folder) {
		logger.debug(`>>> Entering onFolderClick(folder=${folder})`);
        this.hide();
        this.props.onFolderClick(folder);
		logger.debug(`<<< Exiting onFolderClick(folder=${folder})`);
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

    render() {
		logger.debug('>>> Entering render()');
         
        const selectedFolderText = this.props.selectedFolder ? this.props.selectedFolder.value : this.props.notSpecifiedText;

		logger.debug('<<< Exiting render()');
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
