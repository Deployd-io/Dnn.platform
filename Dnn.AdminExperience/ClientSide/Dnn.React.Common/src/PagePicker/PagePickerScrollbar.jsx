import React, { Component } from "react";
import PropTypes from "prop-types";
import Scrollbars from "react-custom-scrollbars";
const logger = console;

export default class PagePickerScrollbar extends Component {

    constructor(props, ...rest) {
        super(props, ...rest);
		logger.debug(`>>> Entering constructor(props=${props},rest=${rest})`);
        this.renderView = this.renderView.bind(this);
        this.renderThumbDefault = this.renderThumbDefault.bind(this);
		logger.debug(`<<< Exiting constructor(props=${props},rest=${rest})`);
    }

    renderThumbDefault({style, ...props }) {
		logger.debug(`>>> Entering renderThumbDefault(style=${style},props=${props})`);
        const thumbStyle = {
            borderRadius: "inherit",
            height:"100%",
            backgroundColor: "rgba(204,204,204,.9)"
        };
		logger.debug(`<<< Exiting renderThumbDefault(style=${style},props=${props})`);
        return <div style={{style,...thumbStyle}} {...props} />;
    }

    renderView({ style, ...props }) {
		logger.debug(`>>> Entering renderView(style=${style},props=${props})`);
        const viewStyle = {
            padding: 15
        };
		logger.debug(`<<< Exiting renderView(style=${style},props=${props})`);
        return (
            <div
                className="box"
                style={{ ...style, ...viewStyle }}
                {...props}/>
        );
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
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