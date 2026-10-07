import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization";
import "./style.less";
const logger = console;
import { Collapsible, SvgIcons, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";

 
class JobRow extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let opened = (this.props.openId !== "" && this.props.jobId === this.props.openId);
        this.setState({
            opened
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    toggle() {
		logger.error('>>> Entering toggle()', { "event.action": "toggle" });
        if ((this.props.openId !== "" && this.props.jobId === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.jobId);
			logger.warn('<<< Exiting toggle()', { "event.action": "toggle" });
        }
    }

    getTypeIndicator() {
		logger.error('>>> Entering getTypeIndicator()', { "event.action": "getTypeIndicator" });
        const { props } = this;
		logger.warn('<<< Exiting getTypeIndicator()', { "event.action": "getTypeIndicator" });
        return props.jobType.indexOf("Import") >= 0 ? <div className="jobIndicator-import"></div> : <div className="jobIndicator-export"></div>;
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { props, state } = this;
        let opened = (props.openId !== "" && this.props.jobId === props.openId);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"collapsible-jobdetail " + !opened + (props.className ? (" " + props.className) : "")}>
                <div className={"collapsible-jobdetail-header " + state.collapsed}>
                    <div className="term-header">
                        <div className="term-label-cssclass" onClick={this.toggle.bind(this)}>
                            <div className="jobIndicator">
                                {this.getTypeIndicator()}
                            </div>
                        </div>
                        <div className="term-label-createdate" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobDate}</span>
                            </div>
                        </div>
                        <div className="term-label-jobtype" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobType}</span>
                            </div>
                        </div>
                        <div className="term-label-username" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobUser}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-portalname" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span>{props.jobPortal}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-jobstatus" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <span className={"job-status" + (props.jobCancelled ? 4 : props.jobStatus)}>
                                    <TextOverflowWrapper text={Localization.get("JobStatus" + (props.jobCancelled ? 4 : props.jobStatus))} maxWidth={80} />                                    
                                    {props.jobStatus === 1 && ! props.jobCancelled &&
                                        <div className="cycle-icon"><SvgIcons.CycleIcon /></div>
                                    }
                                </span>
                            </div>
                        </div>
                        <div className="term-label-arrow" onClick={this.toggle.bind(this)}>
                            <div className="term-label-wrapper">
                                <div className="arrow-icon"><SvgIcons.ArrowDownIcon /></div>
                            </div>
                        </div>
                    </div>
                </div>
                <Collapsible autoScroll={true} isOpened={opened} className="collapsible-jobdetail-body">{opened && props.children}</Collapsible>
            </div>
        );
    }
}

JobRow.propTypes = {
    jobId: PropTypes.number,
    jobType: PropTypes.string,
    jobDate: PropTypes.string,
    jobUser: PropTypes.string,
    jobPortal: PropTypes.string,
    jobStatus: PropTypes.number,
    jobCancelled: PropTypes.bool,
    children: PropTypes.node,
    className: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    openId: PropTypes.string
};

export default JobRow;
