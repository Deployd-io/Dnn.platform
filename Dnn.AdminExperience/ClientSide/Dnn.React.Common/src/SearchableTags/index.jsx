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
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.timeout = 300;
        this.setTimeout = null;
		logger.debug(`constructor(props=${props}): this.timeout → ${this.timeout}`);
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

		logger.debug(`constructor(props=${props}): this.setTimeout → ${this.setTimeout}`);
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
        this.tagsFieldRef = React.createRef();
        this.inputFieldRef = React.createRef();
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        document.addEventListener("keypress", this.onKeyDown, false);
        this.resizeInputField();
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("keypress", this.onKeyDown, false);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    hasClass(className, element) {
        return (" " + element.className + " ").indexOf(" " + className + " ") > -1;
    }

    getServiceFramework() {
		logger.debug('>>> Entering getServiceFramework()');
        let sf = this.props.utils.utilities.sf;
        sf.controller = "ProfileService";
		logger.debug(`getServiceFramework(): sf.controller → ${sf.controller}`);
        sf.moduleRoot = "InternalServices";
		logger.debug(`getServiceFramework(): sf.moduleRoot → ${sf.moduleRoot}`);
		logger.debug('<<< Exiting getServiceFramework()');
        return sf;
    }

    getSearchResults() {
		logger.debug('>>> Entering getSearchResults()');
        this.resizeInputField();
        const searchQuery = this.state.newTagText && typeof this.state.newTagText === "string" ? this.state.newTagText : "";
		logger.debug(`getSearchResults(): searchQuery → ${searchQuery}`);
        if (!searchQuery) {
			logger.debug('<<< Exiting getSearchResults()');
            return this.setSearchResults([]);
        }
        this.setState({ loading: true });
        clearTimeout(this.setTimeout);
        this.setTimeout = setTimeout(() => {
            const sf = this.getServiceFramework();
			logger.debug(`getSearchResults(): sf → ${sf}`);
            return sf.get("Search", { q: searchQuery }, this.setSearchResults.bind(this), this.handleError.bind(this));
        }, this.timeout);
    }

    setSearchResults(searchResults) {
        this.setState({ searchResults, selectedIndex: 0, loading: false });
		logger.debug(`<<< Exiting setSearchResults(searchResults=${searchResults})`);
    }

    handleError(error) {
		logger.debug(`>>> Entering handleError(error=${error})`);
        this.setState({ loading: false });
        //console.log("ERROR:", error);
		logger.debug(`<<< Exiting handleError(error=${error})`);
    }

    resizeInputField() {
		logger.debug('>>> Entering resizeInputField()');
        let width = this.tagsFieldRef.current.getBoundingClientRect().width - 30;
        if (this.state.tags.length) {
            width = this.state.newTagText.length ? this.state.newTagText.length * 7 + 6 : 13;
        }
        this.setState({ inputWidth: width, isInputVisible: true });
		logger.debug('<<< Exiting resizeInputField()');
    }

    addTag(_tag) {
		logger.debug(`>>> Entering addTag(_tag=${_tag})`);
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
		logger.debug(`<<< Exiting addTag(_tag=${_tag})`);
    }

    updateSelectedIndex(diff) {
		logger.debug(`>>> Entering updateSelectedIndex(diff=${diff})`);
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
		logger.debug(`<<< Exiting updateSelectedIndex(diff=${diff})`);
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
		logger.debug('>>> Entering onTagFocus()');
        this.setState({ tagInputActive: true });
		logger.debug('<<< Exiting onTagFocus()');
    }

    onTagBlur() {
		logger.debug('>>> Entering onTagBlur()');
        this.setState({ tagInputActive: false });
		logger.debug('<<< Exiting onTagBlur()');
    }

    removeLastTag() {
		logger.debug('>>> Entering removeLastTag()');
        if (this.state.newTagText || this.props.isDisabled) {
            return;
        }
        let tags = this.state.tags;
        tags.pop();
        this.updateTags(tags);
		logger.debug('<<< Exiting removeLastTag()');
    }

    onChange(event) {
        if (this.props.isDisabled) {
            return;
        }
        this.setState({ newTagText: event.target.value }, this.getSearchResults.bind(this));
    }

    selectItem(selectedIndex) {
        this.setState({ selectedIndex });
		logger.debug(`<<< Exiting selectItem(selectedIndex=${selectedIndex})`);
    }

    removeTagByName(tagId) {
		logger.debug(`>>> Entering removeTagByName(tagId=${tagId})`);
        if (this.props.isDisabled) {
            return;
        }
        let tags = this.state.tags;
        tags = tags.filter((_tag) => { return _tag.id !== tagId; });
        this.updateTags(tags);
		logger.debug(`removeTagByName(tagId=${tagId}): tags → ${tags}`);
		logger.debug(`<<< Exiting removeTagByName(tagId=${tagId})`);
    }

    updateTags(tags) {
        this.setState({ tags, newTagText: "", searchResults: [] }, () => {
            this.resizeInputField();
            this.props.onUpdateTags(tags);
        });
		logger.debug(`<<< Exiting updateTags(tags=${tags})`);
    }

    getResultsItems() {
		logger.debug('>>> Entering getResultsItems()');
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
			logger.debug('<<< Exiting getResultsItems()');
            return false;
        }
    }

    focusInput() {
        this.inputFieldRef.current.focus();
    }

    render() {
		logger.debug('>>> Entering render()');
        const Tags = this.state.tags.map((tag, index) => {
		logger.debug(`render(): Tags → ${Tags}`);
            return <Tag tag={tag.name} key={tag.id  + index} onRemove={this.removeTagByName.bind(this, tag.id) }/>;
        });
        
        const inputStyle = {
            width: this.state.inputWidth,
            display: (this.state.isInputVisible === false ? "none" : "block")
        };
        const searchResults = this.getResultsItems();
		logger.debug(`render(): searchResults → ${searchResults}`);
        const placeholderText = this.props.isDisabled || this.state.tags.length ? "" : "Begin typing to search tags";
		logger.debug(`render(): placeholderText → ${placeholderText}`);
        let className = "tags-field" +
            (this.state.tagInputActive ? " active " : "") +
            (this.props.isDisabled ? " disabled" : "") +
            (this.props.error ? " error" : "");

		logger.debug('<<< Exiting render()');
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