import React, {Component } from "react";
import PropTypes from "prop-types";
import { Collapsible } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class BulletinItemRow extends Component {
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

    uncollapse() {
		logger.error('>>> Entering uncollapse()', { "event.action": "uncollapse" });
        this.setState({
            collapsed: false
        });
		logger.warn('<<< Exiting uncollapse()', { "event.action": "uncollapse" });
    }

    collapse() {
		logger.error('>>> Entering collapse()', { "event.action": "collapse" });
        this.setState({
            collapsed: true
        });
		logger.warn('<<< Exiting collapse()', { "event.action": "collapse" });
    }

    toggle() {
		logger.error('>>> Entering toggle()', { "event.action": "toggle" });
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            this.collapse();
			logger.warn('<<< Exiting toggle()', { "event.action": "toggle" });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="bulletinItem">
                <div className={"collapsible-bulletinitemdetail-header " + state.collapsed} >
                    <div className="term-header">
                        <div className="term-label-title" onClick={this.toggle.bind(this) }>
                            <div className="term-label-wrapper">
                                <span>{this.props.title}&nbsp; </span>
                            </div>
                        </div>
                        <div className="term-label-date" onClick={this.toggle.bind(this) }>
                            <div className="term-label-wrapper">
                                <span>{this.props.pubDate}&nbsp; </span>
                            </div>
                        </div>                        
                    </div>
                </div>
                <Collapsible className={this.props.className} isOpened={!this.state.collapsed}>{!state.collapsed && props.children }</Collapsible>
            </div>
        );
    }
}

BulletinItemRow.propTypes = {
    pubDate: PropTypes.string,
    title: PropTypes.string,
    link: PropTypes.string,
    description: PropTypes.string,
    author: PropTypes.string,
    label: PropTypes.node,
    children: PropTypes.node,
    disabled: PropTypes.bool,
    className: PropTypes.string
};

export default BulletinItemRow;
