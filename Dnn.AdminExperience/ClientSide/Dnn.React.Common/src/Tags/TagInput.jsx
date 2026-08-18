import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

const KEY = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    COMMA: 188,
    DOWN_ARROW: 40,
    UP_ARROW: 38
};

export default class TagInput extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    handleClick(e) {
		logger.debug(`>>> Entering handleClick(e=${e})`);
        if (!this.props.container || this.props.container.contains(e.target)) {
            return;
        }   

        if (this.props.newTagText) {
            this.props.addTag(this.props.newTagText);
        }
        
        this.close();
		logger.debug(`<<< Exiting handleClick(e=${e})`);
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.focusInput();
        document.addEventListener("click", this.handleClick, true);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        this.close();
        document.removeEventListener("click", this.handleClick, true);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    addTag(tag) {
		logger.debug(`>>> Entering addTag(tag=${tag})`);
        this.props.addTag(tag);
        this.inputField.focus();
		logger.debug(`<<< Exiting addTag(tag=${tag})`);
    }

    onChange(event) {
        this.props.onAddingNewTagChange(event.target.value);        
    }

    close() {
		logger.debug('>>> Entering close()');
        this.props.onClose();
		logger.debug('<<< Exiting close()');
    }

    removeLastTag() {
		logger.debug('>>> Entering removeLastTag()');
        if (this.props.newTagText) {
            return;
        }
        this.props.removeLastTag();
		logger.debug('<<< Exiting removeLastTag()');
    }

    onKeyDown(event) {
        const {props} = this;
        switch (event.keyCode) {
            case KEY.ENTER:
            case KEY.COMMA:
            case KEY.TAB:
                event.preventDefault();
                if (props.suggestions.length > 0 && props.selectedIndex > -1 && props.suggestions[props.selectedIndex]) {
                    this.addTag(props.suggestions[props.selectedIndex].value);
                } else {
                    this.addTag(this.props.newTagText);
                }
                break;
            case KEY.BACKSPACE:
                this.removeLastTag();
                break;
            case KEY.DOWN_ARROW:
                this.props.onArrowDown();
                break;
            case KEY.UP_ARROW:
                this.props.onArrowUp();
                break;
        }
    }

    focusInput() {
		logger.debug('>>> Entering focusInput()');
        if (typeof(this.props.onFocus) === "function") {
            this.props.onFocus(this.props.newTagText);
			logger.debug('<<< Exiting focusInput()');
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const {opts} = this.props;

		logger.debug('<<< Exiting render()');
        return (
            <div>
                <div className="input-container">
                    <input
                        ref={(input) => this.inputField = input}
                        type="text"
                        placeholder={this.props.addTagsPlaceholder}
                        onKeyDown={this.onKeyDown.bind(this)}
                        value={this.props.newTagText}
                        aria-label="Tag"
                        onChange={this.onChange.bind(this)}
                        {...opts}
                    />
                </div>
            </div>
        );
    }
}

TagInput.propTypes = {
    newTagText: PropTypes.string.isRequired,
    onAddingNewTagChange: PropTypes.func.isRequired,
    opts: PropTypes.object.isRequired,
    addTag: PropTypes.func.isRequired,
    onClose: PropTypes.func.isRequired,
    removeLastTag: PropTypes.func.isRequired,
    addTagsPlaceholder: PropTypes.string.isRequired,
    onFocus: PropTypes.func,
    container: PropTypes.object,
    onArrowUp: PropTypes.func.isRequired,
    onArrowDown: PropTypes.func.isRequired,
    selectedIndex: PropTypes.number.isRequired,
    suggestions: PropTypes.array.isRequired
};

TagInput.defaultProps = {
    selectedIndex:-1,
    suggestions:[]
};
