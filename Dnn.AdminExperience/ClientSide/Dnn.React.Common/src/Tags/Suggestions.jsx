import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;
import {Scrollbars} from "react-custom-scrollbars";

const style = {
    list: {
        height: 170,
        border: "1px solid #E3E3E3",
        backgroundColor: "#FFFFFF"
    }
};

export default class Suggestions extends Component {

    onSelectSuggestion(tag) {
		logger.debug(`>>> Entering onSelectSuggestion(tag=${tag})`);
        this.props.onSelectSuggestion(tag);
		logger.debug(`<<< Exiting onSelectSuggestion(tag=${tag})`);
    }

    onScrollUpdate(scroll) {
		logger.debug(`>>> Entering onScrollUpdate(scroll=${scroll})`);
        if (typeof (this.props.onScrollUpdate) === "function") {
            this.props.onScrollUpdate(scroll);
			logger.debug(`<<< Exiting onScrollUpdate(scroll=${scroll})`);
        }
    }

    getSuggestions() {
		logger.debug('>>> Entering getSuggestions()');
        let suggestions = this.props.suggestions.map((suggestion, index) => {
            let className = this.props.selectedIndex > -1 && index === this.props.selectedIndex ? "selected" : "";
            return <div ref={(itemRef) => this.itemRef = index === 0 ? itemRef : null} className={`suggestion ${className}`} key={index}
                onClick={this.onSelectSuggestion.bind(this, suggestion.value)}>{suggestion.value}</div>;
        });
		logger.debug('<<< Exiting getSuggestions()');
        return suggestions;
    }

    keySelectionHandler(scrollBars) {
        const selectedIndex = this.props.selectedIndex;
        if (!scrollBars) return;
        if (this.itemRef) {
            const viewHeight = scrollBars.getClientHeight();
            const h = this.itemRef.clientHeight;
            const selectedItemScroll = h * selectedIndex;
            const currentScroll = scrollBars.getScrollTop();
            setTimeout(() => {
                if (selectedItemScroll < currentScroll) {
                    scrollBars.scrollTop(currentScroll - viewHeight);
                } else if (selectedItemScroll + h > (currentScroll + viewHeight )) {
                    scrollBars.scrollTop(selectedItemScroll + h + currentScroll + viewHeight);
                }
            });
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const suggestions = this.getSuggestions();
        const keySelectionHandler = this.keySelectionHandler.bind(this);
		logger.debug('<<< Exiting render()');
        return (<div className="suggestions">
            <Scrollbars ref={keySelectionHandler}
                style={style.list}
                onUpdate={this.onScrollUpdate.bind(this)}>
                {suggestions}
            </Scrollbars>
        </div>);
    }
}


Suggestions.propTypes = {
    onSelectSuggestion: PropTypes.func.isRequired,
    suggestions: PropTypes.array,
    onScrollUpdate: PropTypes.func,
    selectedIndex: PropTypes.number.isRequired
};

Suggestions.defaultProps = {
    suggestions: [],
    selectedIndex: -1
};