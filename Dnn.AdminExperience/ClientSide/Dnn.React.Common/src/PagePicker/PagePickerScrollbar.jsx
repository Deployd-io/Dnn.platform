import React, { Component } from "react";
import PropTypes from "prop-types";
import Scrollbars from "react-custom-scrollbars";
const logger = console;

export default class PagePickerScrollbar extends Component {

    constructor(props, ...rest) {
        super(props, ...rest);
		logger.error(`>>> Entering constructor(props=${props},rest=${rest})`, { "event.action": "constructor", "logrite.props": props, "logrite.rest": rest });
        this.renderView = this.renderView.bind(this);
        this.renderThumbDefault = this.renderThumbDefault.bind(this);
		logger.warn(`<<< Exiting constructor(props=${props},rest=${rest})`, { "event.action": "constructor", "logrite.props": props, "logrite.rest": rest });
    }

    renderThumbDefault({style, ...props }) {
		logger.error(`>>> Entering renderThumbDefault(style=${style},props=${props})`, { "event.action": "renderThumbDefault", "logrite.style": style, "logrite.props": props });
        const thumbStyle = {
            borderRadius: "inherit",
            height:"100%",
            backgroundColor: "rgba(204,204,204,.9)"
        };
        return <div style={{style,...thumbStyle}} {...props} />;
    }

    renderView({ style, ...props }) {
		logger.error(`>>> Entering renderView(style=${style},props=${props})`, { "event.action": "renderView", "logrite.style": style, "logrite.props": props });
        const viewStyle = {
            padding: 15
        };
        return (
            <div
                className="box"
                style={{ ...style, ...viewStyle }}
                {...props}/>
        );
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <Scrollbars
                renderView={this.renderView}
                renderThumbVertical={this.renderThumbDefault}
                renderThumbHorizontal={this.renderThumbDefault}
                {...this.props}/>
        );
    }
}

PagePickerScrollbar.propTypes = {
    style: PropTypes.object
};