import React, { Component } from "react";
import PropTypes from "prop-types";
import debounce from "lodash/debounce";
import MagnifyingGlassIcon from "./MagnifyingGlassIcon";
import "./style.less";
const logger = console;

const style = {
    main: {
        position: "relative",
        padding: 0,
        display: "inline-block"
    },
    input: {            
        position: "absolute",
        border: "none",
        backgroundColor: "transparent",
        color: "inherit",        
        paddingLeft: 20,
        width: "100%",
        height: "100%"
    },
    magnifyingGlass: {
        cursor: "pointer", 
        position: "absolute",
        width: 24,
        height: 24,
        right: 20,
        top: "50%",
        transform: "translateY(-50%)"
    }
};

class SearchBox extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            text: ""
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    
    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        this.debouncedSearch = debounce(this.search, 1000);
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }
    
    search() {
		logger.error('>>> Entering search()', { "event.action": "search" });
        // TODO: do not search if no text, except if the text has been deleted
        // if (this.state.text.trim() === "") {
        //     return;
        // }
        
        this.props.onSearch(this.state.text);                
		logger.warn('<<< Exiting search()', { "event.action": "search" });
    }    
    
    onTextChanged(e) {
		logger.error(`>>> Entering onTextChanged(e=${e})`, { "event.action": "onTextChanged", "logrite.e": e });
        this.setState({text: e.target.value}, () => {
            this.debouncedSearch();
        });     
		logger.warn(`<<< Exiting onTextChanged(e=${e})`, { "event.action": "onTextChanged", "logrite.e": e });
    }
    
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"dnn-search-box" + (this.props.className ? " " + this.props.className : "")} style={{...style.main, ...this.props.style}}>
                <input disabled={this.props.inputDisabled} defaultValue={this.props.initialValue} style={{...style.input, ...this.props.inputStyle}} type="search" placeholder={this.props.placeholder} onChange={this.onTextChanged.bind(this)} aria-label="Search" />
                <MagnifyingGlassIcon style={{...style.magnifyingGlass, ...this.props.iconStyle}} onClick={this.search.bind(this)} />
            </div>
        );
    }
}

SearchBox.propTypes = {
    initialValue: PropTypes.string,
    style: PropTypes.object,
    inputStyle: PropTypes.object,
    iconStyle: PropTypes.object,
    className: PropTypes.string,
    onSearch: PropTypes.func.isRequired,
    placeholder: PropTypes.string,
    inputDisabled: PropTypes.bool
};

SearchBox.defaultProps = {
    placeholder: "Search"
};

export default SearchBox;