import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactCollapse from "react-collapse";
import scroll from "scroll";
import "./style.less";
const logger = console;

const defaultDelay = 300;

const page = /Firefox/.test(navigator.userAgent) ?
    document.documentElement :
    document.body;

export default class Collapsible extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
            this.scrollTimeout = null;
			logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
        }
    }
    scroll(contentHeight) {
		logger.error(`>>> Entering scroll(contentHeight=${contentHeight})`, { "event.action": "scroll", "logrite.contentHeight": contentHeight });
        const {scrollDelay} = this.props;
        const delay = scrollDelay || scrollDelay === 0 ? scrollDelay : defaultDelay;
        if (!this.props.isOpened || !this.props.autoScroll ) {
            return;
        }
        this.scrollTimeout = setTimeout(()=> {
            const collapsible = this.collapsibleRef;
            const collapsibleTop = collapsible.getBoundingClientRect().top;
			logger.error(`scroll(contentHeight=${contentHeight}): collapsibleTop → ${collapsibleTop}`, { "event.action": "scroll", "logrite.contentHeight": contentHeight });
            const bodyTop = document.body.getBoundingClientRect().top;
			logger.error(`scroll(contentHeight=${contentHeight}): bodyTop → ${bodyTop}`, { "event.action": "scroll", "logrite.contentHeight": contentHeight });
            let bottom = collapsibleTop - bodyTop + contentHeight + 40;
            
            if (bottom > window.innerHeight ) {
                bottom = bottom - window.innerHeight;
                if (bottom > window.scrollY) {
                    scroll.top(page, bottom);
                }
            }
        }, delay);
		logger.warn(`<<< Exiting scroll(contentHeight=${contentHeight})`, { "event.action": "scroll", "logrite.contentHeight": contentHeight });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {isOpened} = this.props;
        const className = this.props.className || "";
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div ref={(node) => this.collapsibleRef = node} style={{height: "100%", width:"100%"}}>
                <ReactCollapse
                    isOpened={isOpened}
                    theme={{collapse: `${className} ReactCollapse--collapse`, content: "ReactCollapse--content"}}
                    onRest={({contentHeight}) => this.scroll(contentHeight)}
                    onWork={({contentHeight}) => this.scroll(contentHeight)}>
                    {this.props.children}
                </ReactCollapse>
            </div>
        );
    }
}

Collapsible.propTypes = {
    isOpened: PropTypes.bool,
    autoScroll: PropTypes.bool,
    scrollDelay: PropTypes.number,
    children: PropTypes.node,
    className: PropTypes.string
};
