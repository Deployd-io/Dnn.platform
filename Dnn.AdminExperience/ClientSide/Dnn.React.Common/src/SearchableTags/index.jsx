import React, { Component } from "react";
import PropTypes from "prop-types";
import Tooltip from "../Tooltip";
import Tag from "./Tag";

import "./style.less";
const logger = console;

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    ESCAPE: 27,
    UP: 38,
    DOWN: 40

};

export default class SearchableTags extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.timeout = 300;
        this.setTimeout = null;
		logger.debug(`constructor(props=${props}): this.timeout → ${this.timeout}`, { "event.action": "constructor", "logrite.props": props });
		logger.debug(`constructor(props=${props}): this.setTimeout → ${this.setTimeout}`, { "event.action": "constructor", "logrite.props": props });
        this.onKeyDown = this.onKeyDown.bind(this);
        this.state = {
            tags: this.props.tags || [],
            newTagText: "",
            isInputVisible: true,
            tagInputActive: false,
            selectedIndex: 0,
            loading: false,
            inputWidth: 13,
            searchResults: []
        };

		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
        this.tagsFieldRef = React.createRef();
        this.inputFieldRef = React.createRef();
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        document.addEventListener("keypress", this.onKeyDown, false);
        this.resizeInputField();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        document.removeEventListener("keypress", this.onKeyDown, false);
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    hasClass(className, element) {
        return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
    }

    getServiceFramework() {
		logger.error('>>> Entering getServiceFramework()', { "event.action": "getServiceFramework" });
        let sf = this.props.utils.utilities.sf;
        sf.controller = "ProfileService";
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`, { "event.action": "getServiceFramework" });
        sf.moduleRoot = "InternalServices";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "getServiceFramework" });
		logger.warn('<<< Exiting getServiceFramework()', { "event.action": "getServiceFramework" });
        return sf;
    }

    getSearchResults() {
		logger.error('>>> Entering getSearchResults()', { "event.action": "getSearchResults" });
        this.resizeInputField();
        const searchQuery = this.state.newTagText && typeof this.state.newTagText === "string" ? this.state.newTagText : "";
		logger.error(`getSearchResults(): searchQuery → ${searchQuery}`, { "event.action": "getSearchResults" });
        if (!searchQuery) {
			logger.warn('<<< Exiting getSearchResults()', { "event.action": "getSearchResults" });
            return this.setSearchResults([]);
        }
        this.setState({ loading: true });
        clearTimeout(this.setTimeout);
        this.setTimeout = setTimeout(() => {
            const sf = this.getServiceFramework();
			logger.error(`getSearchResults(): sf → ${sf}`, { "event.action": "getSearchResults" });
            return sf.get("Search", { q: searchQuery }, this.setSearchResults.bind(this), this.handleError.bind(this));
        }, this.timeout);
    }

    setSearchResults(searchResults) {
		logger.error(`>>> Entering setSearchResults(searchResults=${searchResults})`, { "event.action": "setSearchResults", "logrite.searchResults": searchResults });
        this.setState({ searchResults, selectedIndex: 0, loading: false });
		logger.warn(`<<< Exiting setSearchResults(searchResults=${searchResults})`, { "event.action": "setSearchResults", "logrite.searchResults": searchResults });
    }

    handleError(error) {
		logger.error(`>>> Entering handleError(error=${error})`, { "event.action": "handleError", "logrite.error": error });
        this.setState({ loading: false });
        //console.log("ERROR:", error);
		logger.warn(`<<< Exiting handleError(error=${error})`, { "event.action": "handleError", "logrite.error": error });
    }

    resizeInputField() {
		logger.error('>>> Entering resizeInputField()', { "event.action": "resizeInputField" });
        let width = this.tagsFieldRef.current.getBoundingClientRect().width - 30;
        if (this.state.tags.length) {
            width = this.state.newTagText.length ? this.state.newTagText.length * 7 + 6 : 13;
        }
        this.setState({ inputWidth: width, isInputVisible: true });
		logger.warn('<<< Exiting resizeInputField()', { "event.action": "resizeInputField" });
    }

    addTag(_tag) {
		logger.error(`>>> Entering addTag(_tag=${_tag})`, { "event.action": "addTag", "logrite._tag": _tag });
        if (!this.state.searchResults.length || this.props.isDisabled) {
            return;
        }
        const index = this.state.selectedIndex || 0;
        const tag = _tag || this.state.searchResults[index];
        if (!tag) {
            return;
        }
        this.setState({ isInputVisible: false });
        let {tags} = this.state;
        tags.push(tag);
        this.updateTags(tags);
        setTimeout(() => { this.inputFieldRef.current.focus(); }, 0);
		logger.warn(`<<< Exiting addTag(_tag=${_tag})`, { "event.action": "addTag", "logrite._tag": _tag });
    }

    updateSelectedIndex(diff) {
		logger.error(`>>> Entering updateSelectedIndex(diff=${diff})`, { "event.action": "updateSelectedIndex", "logrite.diff": diff });
        if (this.props.isDisabled) {
            return;
        }
        let selectedIndex = this.state.selectedIndex + diff;
        if (selectedIndex < 0) {
            selectedIndex = 0;
        }
        if (selectedIndex >= this.state.searchResults.length) {
            selectedIndex = this.state.searchResults.length - 1;
        }
        this.setState({ selectedIndex });
		logger.warn(`<<< Exiting updateSelectedIndex(diff=${diff})`, { "event.action": "updateSelectedIndex", "logrite.diff": diff });
    }

    onKeyDown(event) {
        switch (event.keyCode) {
            case KEY.ENTER:
                return this.addTag();
            case KEY.TAB:
                event.preventDefault();
                return this.addTag();
            case KEY.BACKSPACE:
                return this.removeLastTag();
            case KEY.UP:
                return this.updateSelectedIndex(-1);
            case KEY.DOWN:
                return this.updateSelectedIndex(1);
        }
    }

    onTagFocus() {
		logger.error('>>> Entering onTagFocus()', { "event.action": "onTagFocus" });
        this.setState({ tagInputActive: true });
		logger.warn('<<< Exiting onTagFocus()', { "event.action": "onTagFocus" });
    }

    onTagBlur() {
		logger.error('>>> Entering onTagBlur()', { "event.action": "onTagBlur" });
        this.setState({ tagInputActive: false });
		logger.warn('<<< Exiting onTagBlur()', { "event.action": "onTagBlur" });
    }

    removeLastTag() {
		logger.error('>>> Entering removeLastTag()', { "event.action": "removeLastTag" });
        if (this.state.newTagText || this.props.isDisabled) {
            return;
        }
        let tags = this.state.tags;
        tags.pop();
        this.updateTags(tags);
		logger.warn('<<< Exiting removeLastTag()', { "event.action": "removeLastTag" });
    }

    onChange(event) {
        if (this.props.isDisabled) {
            return;
        }
        this.setState({ newTagText: event.target.value }, this.getSearchResults.bind(this));
    }

    selectItem(selectedIndex) {
		logger.error(`>>> Entering selectItem(selectedIndex=${selectedIndex})`, { "event.action": "selectItem", "logrite.selectedIndex": selectedIndex });
        this.setState({ selectedIndex });
		logger.warn(`<<< Exiting selectItem(selectedIndex=${selectedIndex})`, { "event.action": "selectItem", "logrite.selectedIndex": selectedIndex });
    }

    removeTagByName(tagId) {
		logger.error(`>>> Entering removeTagByName(tagId=${tagId})`, { "event.action": "removeTagByName", "logrite.tagId": tagId });
        if (this.props.isDisabled) {
            return;
        }
        let tags = this.state.tags;
        tags = tags.filter((_tag) => { return _tag.id !== tagId; });
        this.updateTags(tags);
		logger.error(`removeTagByName(tagId=${tagId}): tags → ${tags}`, { "event.action": "removeTagByName", "logrite.tagId": tagId });
    }

    updateTags(tags) {
		logger.error(`>>> Entering updateTags(tags=${tags})`, { "event.action": "updateTags", "logrite.tags": tags });
        this.setState({ tags, newTagText: "", searchResults: [] }, () => {
            this.resizeInputField();
            this.props.onUpdateTags(tags);
        });
		logger.warn(`<<< Exiting updateTags(tags=${tags})`, { "event.action": "updateTags", "logrite.tags": tags });
    }

    getResultsItems() {
		logger.error('>>> Entering getResultsItems()', { "event.action": "getResultsItems" });
        if (this.state.searchResults.length) {
            return this.state.searchResults.map((result, index) => {
                const className = index === this.state.selectedIndex ? "selected" : "";
                return <div
                    key={result.id}
                    className={"result-item " + className}
                    onClick={this.addTag.bind(this, result) }
                    onMouseEnter={this.selectItem.bind(this, index) }
                >
                    {result.name}</div>;
            });
        } else if (!this.state.searchResults.length && this.state.newTagText) {
            const text = this.state.loading ? "Searching..." : "No Results";
            return <div className="result-item">{text}</div>;
        } else {
			logger.warn('<<< Exiting getResultsItems()', { "event.action": "getResultsItems" });
            return false;
        }
    }

    focusInput() {
        this.inputFieldRef.current.focus();
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const Tags = this.state.tags.map((tag, index) => {
		logger.error(`render(): Tags → ${Tags}`, { "event.action": "render" });
            return <Tag tag={tag.name} key={tag.id  + index} onRemove={this.removeTagByName.bind(this, tag.id) }/>;
        });
        
        const inputStyle = {
            width: this.state.inputWidth,
            display: (this.state.isInputVisible === false ? "none" : "block")
        };
        const searchResults = this.getResultsItems();
		logger.error(`render(): searchResults → ${searchResults}`, { "event.action": "render" });
        const placeholderText = this.props.isDisabled || this.state.tags.length ? "" : "Begin typing to search tags";
		logger.error(`render(): placeholderText → ${placeholderText}`, { "event.action": "render" });
        let className = "tags-field" +
            (this.state.tagInputActive ? " active " : "") +
            (this.props.isDisabled ? " disabled" : "") +
            (this.props.error ? " error" : "");

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div
            className={ className }
            onClick={this.focusInput.bind(this) }
            ref={this.tagsFieldRef}>
            <div type="text" className="dark-form-control">
                {Tags}
                <input
                    disabled={this.props.isDisabled}
                    ref={this.inputFieldRef}
                    type="text"
                    placeholder={placeholderText}
                    style={inputStyle}
                    onKeyDown={this.onKeyDown.bind(this) }
                    value={this.state.newTagText}
                    onChange={this.onChange.bind(this) }
                    onFocus={this.onTagFocus.bind(this) }
                    onBlur={this.onTagBlur.bind(this) } 
                    aria-label="Tag" />
            </div>
            {searchResults && <div className="tag-search-results">
                {searchResults}
            </div>}
            {this.props.error && this.props.errorMessage && <Tooltip
                messages={[this.props.errorMessage]}
                type="error"
                tooltipPlace={"top"}
                rendered={this.props.error}/>}
        </div>;
    }
}

SearchableTags.propTypes = {
    utils: PropTypes.object.isRequired,
    tags: PropTypes.array.isRequired,
    onUpdateTags: PropTypes.func.isRequired,
    
    error: PropTypes.bool,
    errorMessage: PropTypes.string,
    
    isDisabled: PropTypes.bool
};