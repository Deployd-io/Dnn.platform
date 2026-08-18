import React, { Component } from "react";
import PropTypes from "prop-types";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import "./style.less";
const logger = console;

class DnnTabs extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.uniqueId = Date.now() * Math.random();
		logger.debug('<<< Exiting constructor()');
        this.state = {
            selectedIndex: 0
        };
    }

    onSelect(index, last) {
        const { props } = this;
        if (typeof props.selectedIndex === "undefined") {
            this.setState({
                selectedIndex: index
            });
        }
        if (props.onSelect) {
            props.onSelect(index, last);
        }
    }

    getTabHeaders() {
		logger.debug('>>> Entering getTabHeaders()');
        const {props} = this;
		logger.debug('<<< Exiting getTabHeaders()');
        return props.tabHeaders.map((tabHeader, index) => {
            return <Tab key={this.uniqueId + "-" + index}>{tabHeader}</Tab>;
        });
    }
    
    getTabPanels() {
        const {props} = this;
        let children = [];        
        if (Object.prototype.toString.call(props.children) === "[object Array]") {
            for (let i = 0; i < (props.children.length); i++) {
                children.push(<TabPanel key={this.uniqueId + "panel-" + i}>{props.children[i]}</TabPanel>);
            }
            return children;
        }
        return <TabPanel key={this.uniqueId + "panel-0"}>{props.children}</TabPanel>;
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        const className = (props.className ? " " + props.className : "");
		logger.debug('<<< Exiting render()');
        return (
            <Tabs className={"dnn-tabs react-tabs" + className + (" " + props.type) } onSelect={this.onSelect.bind(this) }
                selectedIndex={typeof props.selectedIndex !== "undefined" ? props.selectedIndex : state.selectedIndex}>
                <TabList>
                    {this.getTabHeaders() }
                </TabList>
                {this.getTabPanels() }
            </Tabs>
        );
    }
}

DnnTabs.propTypes = {
    selectedIndex: PropTypes.number,
    tabHeaders: PropTypes.array,
    className: PropTypes.string,
    children: PropTypes.node,
    onSelect: PropTypes.func
};

DnnTabs.defaultProps = {
    type: "primary"
};

export default DnnTabs;