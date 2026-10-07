import React, { Component } from "react";
import PropTypes from "prop-types";
import ArrowSvg from "./arrow_bullet.svg";
const logger = console;
import { global } from "../../global";

const styles = global.styles;
const merge = styles.merge;

import { nanoid } from "nanoid/non-secure";

const style = (direction) => {
	logger.error(`>>> Entering style(direction=${direction})`, { "event.action": "style", "logrite.direction": direction });
	logger.warn(`<<< Exiting style(direction=${direction})`, { "event.action": "style", "logrite.direction": direction });
    return {
        transition: "all .15s ease-in",
        cursor: "pointer",
        width: "100%",
        transform: `rotate(${direction})`
    };
};

export class ArrowIcon extends Component {

    constructor(props) {
        super();
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.direction = props.direction;
        this.id = nanoid();
		logger.debug(`constructor(props=${props}): this.direction → ${this.direction}`, { "event.action": "constructor", "logrite.props": props });
        this.state = {};
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`, { "event.action": "constructor", "logrite.props": props });
    }


    onMouseDown() {
		logger.error('>>> Entering onMouseDown()', { "event.action": "onMouseDown" });
        if (!this.state.arrow_bullet) {
            this.arrow_bullet = document.getElementById(this.id);
        }
        this.animate(this.props.shouldAnimate);
		logger.warn('<<< Exiting onMouseDown()', { "event.action": "onMouseDown" });
    }

    animate(bool) {
		logger.error(`>>> Entering animate(bool=${bool})`, { "event.action": "animate", "logrite.bool": bool });
        const left = () => {
			logger.error('>>> Entering left()', { "event.action": "left" });
            this.setState({ selected: !this.state.selected });
            const arrow_css = this.arrow_bullet.style;
            arrow_css.transform = (this.state.selected) ? "rotate(0deg)" : "rotate(90deg)";
			logger.warn('<<< Exiting left()', { "event.action": "left" });
        };
        const right = () => null;
        bool ? left() : right();
		logger.warn(`<<< Exiting animate(bool=${bool})`, { "event.action": "animate", "logrite.bool": bool });
    }

    reset(bool) {
		logger.error(`>>> Entering reset(bool=${bool})`, { "event.action": "reset", "logrite.bool": bool });
        if (bool) {
            this.setState({ selected: false });
			logger.warn(`<<< Exiting reset(bool=${bool})`, { "event.action": "reset", "logrite.bool": bool });
        }
    }


    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
         
        const marginTop = styles.margin({ top: -3 });
		logger.error(`render(): marginTop → ${marginTop}`, { "event.action": "render" });
        const padding = styles.padding({ all: 2 });
		logger.error(`render(): padding → ${padding}`, { "event.action": "render" });
        const baseStyles = style(this.direction);
		logger.error(`render(): baseStyles → ${baseStyles}`, { "event.action": "render" });
        const height = {height:"20px"};
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div
                id={this.id}
                style={merge(marginTop, padding, baseStyles, height)}
                src={ArrowSvg}
                alt="arrow_icon"
                onClick={this.onMouseDown.bind(this)}><ArrowSvg /></div>
        );
         
    }

}

ArrowIcon.propTypes = {
    direction: PropTypes.string.isRequired,
    animate: PropTypes.string,
    shouldAnimate: PropTypes.bool
};
