import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

class Dropdown extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            isTimeSelectorVisible: false,
            className: ""
        };
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`constructor(): this.state → ${this.state}`);
        this.timePickerRef = React.createRef();
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
        const node = this.timePickerRef.current;
        if (node && node.contains(e.target)) {
            return;
        }
        this.hideTimeSelector();
		logger.debug(`<<< Exiting handleClick(e=${e})`);
    }

    onUpdate(value) {
		logger.debug(`>>> Entering onUpdate(value=${value})`);
        this.hideTimeSelector();
        setTimeout( ()=> {this.props.onUpdate(value);}, 200);
		logger.debug(`<<< Exiting onUpdate(value=${value})`);
    }
    
    showTimeSelector() {
		logger.debug('>>> Entering showTimeSelector()');
        this.setState({ isTimeSelectorVisible: true }, () => {
            setTimeout(() => {
                this.setState({ className: "show" });
            }, 0);
        });
		logger.debug('<<< Exiting showTimeSelector()');
    }

    hideTimeSelector() {
		logger.debug('>>> Entering hideTimeSelector()');
        this.setState({ className: "" }, () => {
            setTimeout(() => {
                this.setState({ isTimeSelectorVisible: false });
            }, 200);
        });
		logger.debug('<<< Exiting hideTimeSelector()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const {options, label, className} = this.props;
        const layoutOptions = options.map((option) => {
		logger.debug('<<< Exiting render()');
            return <div className="time-option" key={option.value} onClick={this.onUpdate.bind(this, option.value) }>{option.label}</div>;
        });
        
        return (
            <div className={"dnn-time-picker " +  (className || "")} ref={this.timePickerRef}>
                <div className="time-text" onClick={this.showTimeSelector.bind(this)}>
                    <span className="time-text-label" title={label}>{label}</span>
                    {this.state.isTimeSelectorVisible && <div className={"time-selector " + this.state.className}>
                        <div className="time-options">
                            {layoutOptions}
                        </div>
                    </div>}
                </div>
            </div >
        );
    }
}

Dropdown.propTypes = {
    onUpdate: PropTypes.func.isRequired,
    options: PropTypes.array.isRequired,
    label: PropTypes.string,
    className: PropTypes.string
};

export default Dropdown;