import React, {Component } from "react";
import PropTypes from "prop-types";
import { Collapsible } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class BulletinItemRow extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    uncollapse() {
		logger.debug('>>> Entering uncollapse()');
        this.setState({
            collapsed: false
        });
		logger.debug('<<< Exiting uncollapse()');
    }

    collapse() {
		logger.debug('>>> Entering collapse()');
        this.setState({
            collapsed: true
        });
		logger.debug('<<< Exiting collapse()');
    }

    toggle() {
		logger.debug('>>> Entering toggle()');
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            this.collapse();
			logger.debug('<<< Exiting toggle()');
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
		logger.debug('<<< Exiting render()');
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
