import React, { Component } from "react";
import PropTypes from "prop-types";
import {
    ArrowLeftIcon,
    ArrowRightIcon,
    ArrowEndLeftIcon,
    ArrowEndRightIcon
} from "../SvgIcons";
import DropDown from "../Dropdown";
import "./style.less";
const logger = console;

class Pager extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            currentPage: 0,
            pageSize: props.pageSize,
            totalPages: 0,
            startIndex: 0,
            endIndex: props.numericCounters !== undefined ? props.numericCounters : 0
        };        
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.calculateTotalPages(this.props);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (prevProps.totalRecords !== this.props.totalRecords)
            this.calculateTotalPages(this.props);
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
    }
    
    formatCommaSeparate(number) {
		logger.error(`>>> Entering formatCommaSeparate(number=${number})`, { "event.action": "formatCommaSeparate", "logrite.number": number });
        const numberFormat = new Intl.NumberFormat();
		logger.error(`formatCommaSeparate(number=${number}): numberFormat → ${numberFormat}`, { "event.action": "formatCommaSeparate", "logrite.number": number });
        return numberFormat.format(number);
    }
    calculateTotalPages(props) {
		logger.error(`>>> Entering calculateTotalPages(props=${props})`, { "event.action": "calculateTotalPages", "logrite.props": props });
        const { state } = this;
        let { startIndex } = state;
        let { endIndex } = state;
		logger.debug(`calculateTotalPages(props=${props}): state.totalPages → ${state.totalPages}`, { "event.action": "calculateTotalPages", "logrite.props": props });
		logger.debug(`calculateTotalPages(props=${props}): state.endIndex → ${state.endIndex}`, { "event.action": "calculateTotalPages", "logrite.props": props });
        let { currentPage } = state;
        if (state.pageSize >= props.totalRecords || props.totalRecords === 0) {
            state.totalPages = 1;
            state.endIndex = 1;
        } else {
            if (state.endIndex === 1) {
                state.endIndex = props.numericCounters !== undefined ? props.numericCounters : 0;
            }
            let totalPages = parseInt(props.totalRecords / state.pageSize);
			logger.error(`calculateTotalPages(props=${props}): totalPages → ${totalPages}`, { "event.action": "calculateTotalPages", "logrite.props": props });
            if (props.totalRecords % state.pageSize !== 0) {
                totalPages++;
            }
            state.totalPages = totalPages;
            if (props.numericCounters > 0) {
                if (currentPage >= totalPages - 1) {
                    endIndex = totalPages;
                    startIndex = (endIndex - props.numericCounters) > 0 ? (endIndex - props.numericCounters) : 0;
					logger.error(`calculateTotalPages(props=${props}): startIndex → ${startIndex}`, { "event.action": "calculateTotalPages", "logrite.props": props });
                }
                else if (currentPage <= 0) {
                    startIndex = 0;
                    endIndex = (startIndex + props.numericCounters) > totalPages - 1 ? totalPages : (startIndex + props.numericCounters);
                }
                state.startIndex = startIndex;
                state.endIndex = endIndex;
            }

        }
        if (state.currentPage >= state.totalPages) {
            state.currentPage = state.totalPages - 1;
        }
        if (props.resetIndex) {
            state.currentPage = 0;
            state.startIndex = 0;
        }

        this.setState({
            state
        });
    }

    format() {
        let format = arguments[0];
        let methodsArgs = arguments;
        return format.replace(/{(\d+)}/gi, function (value, index) {
            let argsIndex = parseInt(index) + 1;
            return methodsArgs[argsIndex];
        });
    }
    onPageSizeSelected(option) {
		logger.error(`>>> Entering onPageSizeSelected(option=${option})`, { "event.action": "onPageSizeSelected", "logrite.option": option });
        let { state } = this;
        if (state.pageSize !== option.value) {
            state.pageSize = option.value;
			logger.debug(`onPageSizeSelected(option=${option}): state.pageSize → ${state.pageSize}`, { "event.action": "onPageSizeSelected", "logrite.option": option });
            this.setState({
                state
            });
            this.calculateTotalPages(this.props);
            this.onPageChanged(0);
			logger.warn(`<<< Exiting onPageSizeSelected(option=${option})`, { "event.action": "onPageSizeSelected", "logrite.option": option });
        }
    }
    onPageChanged(step) {
		logger.error(`>>> Entering onPageChanged(step=${step})`, { "event.action": "onPageChanged", "logrite.step": step });
        let { currentPage } = this.state;
        let { state } = this;
        switch (step) {
            case ">": {
                currentPage++;
                break;
            }
            case "<": {
                currentPage--;
                break;
            }
            case ">>": {
                currentPage = state.totalPages - 1;
				logger.debug(`onPageChanged(step=${step}): currentPage → ${currentPage}`, { "event.action": "onPageChanged", "logrite.step": step });
                break;
            }
            case "<<": {
                currentPage = 0;
                break;
            }
            default: {
                currentPage = step;
                break;
            }
        }
        let { startIndex } = state;
        let { endIndex } = state;
        if (this.props.numericCounters > 0) {
            let boxesHalf = parseInt(this.props.numericCounters / 2);
			logger.error(`onPageChanged(step=${step}): boxesHalf → ${boxesHalf}`, { "event.action": "onPageChanged", "logrite.step": step });
            if (currentPage < startIndex && currentPage < state.totalPages && currentPage > 0) {
                startIndex = (startIndex - boxesHalf) < 0 ? 0 : startIndex - boxesHalf;
				logger.error(`onPageChanged(step=${step}): startIndex → ${startIndex}`, { "event.action": "onPageChanged", "logrite.step": step });
                endIndex = startIndex > 0 ? endIndex - boxesHalf : endIndex;
				logger.error(`onPageChanged(step=${step}): endIndex → ${endIndex}`, { "event.action": "onPageChanged", "logrite.step": step });
            }
            else if (currentPage >= endIndex - 1 && currentPage < (state.totalPages - 1) && currentPage > 0) {
                endIndex = (endIndex + boxesHalf) > state.totalPages ? state.totalPages : endIndex + boxesHalf;
                startIndex = endIndex < state.totalPages ? startIndex + boxesHalf :
                    ((endIndex - this.props.numericCounters) > 0 ? (endIndex - this.props.numericCounters) : 0);
					logger.warn(`<<< Exiting onPageChanged(step=${step})`, { "event.action": "onPageChanged", "logrite.step": step });
            }
            else if (currentPage >= state.totalPages - 1) {
                endIndex = state.totalPages;
                startIndex = (endIndex - this.props.numericCounters) > 0 ? (endIndex - this.props.numericCounters) : 0;
            }
            else if (currentPage <= 0) {
                startIndex = 0;
                endIndex = (startIndex + this.props.numericCounters) > state.totalPages - 1 ? state.totalPages : (startIndex + this.props.numericCounters);
            }
            if (endIndex > startIndex + this.props.numericCounters) {
                endIndex = startIndex + this.props.numericCounters;
            }
        }
        this.setState({
            currentPage,
            startIndex,
            endIndex
        });
        this.props.onPageChanged(currentPage, state.pageSize);
    }

    getPageSummary() {
		logger.error('>>> Entering getPageSummary()', { "event.action": "getPageSummary" });
        if (this.props.showSummary) {
            const { props, state } = this;
            if (props.totalRecords <= 0) {
                return null;
            }
            let start = state.currentPage * state.pageSize + 1;
            let end = start + state.pageSize - 1;
            if (end > props.totalRecords) {
                end = props.totalRecords;
            }
            return this.format(this.props.summaryText, this.formatCommaSeparate(start), this.formatCommaSeparate(end), this.formatCommaSeparate(props.totalRecords));
        }
		logger.warn('<<< Exiting getPageSummary()', { "event.action": "getPageSummary" });
        return "&nbsp;";
    }
    getPagingBoxes() {
		logger.error('>>> Entering getPagingBoxes()', { "event.action": "getPagingBoxes" });
        let { state } = this;
        let { currentPage } = state;
        let { startIndex } = state;
        let { endIndex } = state;
        let pagingBoxes = [];
        if (this.props.numericCounters > 1) {
            for (let i = startIndex; i < endIndex; i++) {
                let step = i + 1;
                if (i !== currentPage) {
                    pagingBoxes = pagingBoxes.concat(<li key={"li" + i} className="pages do-not-close" onClick={this.onPageChanged.bind(this, i)}>{this.formatCommaSeparate(step)}</li>);
                } else {
                    pagingBoxes = pagingBoxes.concat(<li key={"li" + i} className="pages current do-not-close">{this.formatCommaSeparate(i + 1)}</li>);
                }
            }
        }
        else if (this.props.numericCounters === 1) {
            pagingBoxes = pagingBoxes.concat(<li key="li-single" className="pages current do-not-close">{this.formatCommaSeparate(currentPage + 1)}</li>);
        }
		logger.warn('<<< Exiting getPagingBoxes()', { "event.action": "getPagingBoxes" });
        return pagingBoxes;
    }
    getPageSizeDropDown() {
        let pageSizeOptions = [];
        if (this.props.totalRecords >= 1) pageSizeOptions.push({ "value": 10, "label": this.format(this.props.pageSizeOptionText, this.formatCommaSeparate(10)) });
        if (this.props.totalRecords >= 10) pageSizeOptions.push({ "value": 25, "label": this.format(this.props.pageSizeOptionText, this.formatCommaSeparate(25)) });
        if (this.props.totalRecords >= 25) pageSizeOptions.push({ "value": 50, "label": this.format(this.props.pageSizeOptionText, this.formatCommaSeparate(50)) });
        if (this.props.totalRecords >= 50) pageSizeOptions.push({ "value": 100, "label": this.format(this.props.pageSizeOptionText, this.formatCommaSeparate(100)) });
        if (this.props.totalRecords >= 100) pageSizeOptions.push({ "value": 250, "label": this.format(this.props.pageSizeOptionText, this.formatCommaSeparate(250)) });

        if (!pageSizeOptions.some(option => option.value === this.props.pageSize)) {
            pageSizeOptions = pageSizeOptions.concat({ "value": this.props.pageSize, "label": this.format(this.props.pageSizeOptionText, this.formatCommaSeparate(this.props.pageSize)) });
            pageSizeOptions = pageSizeOptions.sort(function (a, b) {
                let valueA = a.value;
                let valueB = b.value;
                if (valueA < valueB)
                    return -1;
                if (valueA > valueB)
                    return 1;
                return 0;
            });
        }

        return (<DropDown
            options={pageSizeOptions}
            value={this.props.pageSize}
            onSelect={this.onPageSizeSelected.bind(this)}
            withBorder={!this.props.pageSizeDropDownWithoutBorder}
        />
        );
    }
     
    renderIcon(OnClick, Type, Disabled) {
		logger.error(`>>> Entering renderIcon(OnClick=${OnClick},Type=${Type},Disabled=${Disabled})`, { "event.action": "renderIcon", "logrite.OnClick": OnClick, "logrite.Type": Type, "logrite.Disabled": Disabled });
        if (!Disabled) {
            return <li className={"do-not-close pages prev"} onClick={OnClick.bind(this)}>
                <span className="icon-button"><Type /></span>
            </li>;
        } else {
			logger.warn(`<<< Exiting renderIcon(OnClick=${OnClick},Type=${Type},Disabled=${Disabled})`, { "event.action": "renderIcon", "logrite.OnClick": OnClick, "logrite.Type": Type, "logrite.Disabled": Disabled });
            return <li className={"do-not-close pages prev disabled"} >
                <span className="icon-flat"><Type /></span>
            </li>;
        }
    }
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { state, props } = this;
        if (state.totalPages > 1 || (props.totalRecords >= 10 && state.totalPages === 1 && this.props.showPageSizeOptions)) {
            return (
                <div className="dnn-pager do-not-close" style={props.style}>
                    <div className="dnn-pager-summary-box">
                        {this.getPageSummary()}
                    </div>
                    <div className="dnn-pager-control">
                        <div className="dnn-pager-paging-box">
                            <ul>
                                {
                                    this.props.showStartEndButtons &&
                                    this.renderIcon(this.onPageChanged.bind(this, "<<"), ArrowEndLeftIcon, state.currentPage < 1)
                                }
                                {
                                    this.renderIcon(this.onPageChanged.bind(this, "<"), ArrowLeftIcon, state.currentPage < 1)
                                }
                                {this.getPagingBoxes()}
                                {
                                    this.renderIcon(this.onPageChanged.bind(this, ">"), ArrowRightIcon, state.totalPages <= (state.currentPage + 1))
                                }
                                {
                                    this.props.showStartEndButtons &&
                                    this.renderIcon(this.onPageChanged.bind(this, ">>"), ArrowEndRightIcon, state.totalPages <= (state.currentPage + 1))
                                }
                            </ul>
                        </div>
                        {this.props.showPageInfo && !this.props.showPageSizeOptions &&
                            <div className="dnn-pager-options-info-box">
                                {this.format(this.props.pageInfoText, this.formatCommaSeparate(state.currentPage + 1), this.formatCommaSeparate(state.totalPages))}
                            </div>
                        }
                        {
                            this.props.showPageSizeOptions &&
                            <div className="dnn-pager-pageSize-box">
                                {this.getPageSizeDropDown()}
                            </div>
                        }
                    </div>
                </div >
            );
        } else {
			logger.warn('<<< Exiting render()', { "event.action": "render" });
            return <div />;
        }
    }
}
Pager.propTypes = {
    showStartEndButtons: PropTypes.bool,
    showPageSizeOptions: PropTypes.bool,
    pageSizeDropDownWithoutBorder: PropTypes.bool,
    showSummary: PropTypes.bool,
    showPageInfo: PropTypes.bool,
    summaryText: PropTypes.string,
    pageInfoText: PropTypes.string,
    numericCounters: PropTypes.number,
    pageSizeOptionText: PropTypes.string,
    pageSize: PropTypes.number,
    style: PropTypes.object,
    totalRecords: PropTypes.number.isRequired,
    onPageChanged: PropTypes.func.isRequired,
    resetIndex: PropTypes.bool,
    culture: PropTypes.string
};

Pager.defaultProps = {
    showStartEndButtons: true,
    showPageSizeOptions: true,
    pageSizeDropDownWithoutBorder: false,
    numericCounters: 1,
    showSummary: true,
    showPageInfo: false,
    summaryText: "Showing {0} - {1} of {2}",
    pageInfoText: "Page {0} of {1}",
    pageSizeOptionText: "{0} per page",
    pageSize: 10,
    resetIndex: false,
    culture: "en-US"
};
/*
showPageSizeOptions and showPageInfo are mutually exclusive.
Preferred value for numericCounters is odd. 
*/

/*Sample onPageChanged method*/
// onPageChanged(currentPage, pageSize) {
//         let {state} = this;
//         if (pageSize !== undefined && state.pageSize !== pageSize) {
//             state.pageSize = pageSize;
//         }
//         state.currentPage = currentPage;
//         this.setState({
//             state
//         });
//         this.getData();
//     }

export default (Pager);