import React, { Component } from "react";
import PropTypes from "prop-types";
import ArrowSvg from "./arrow_bullet.svg";
const logger = console;
import { global } from "../../global";

const styles = global.styles;
const merge = styles.merge;

import { nanoid } from "nanoid/non-secure";

const style = (direction) => {
	logger.debug(`>>> Entering style(direction=${direction})`);
	logger.debug(`<<< Exiting style(direction=${direction})`);
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
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.direction = props.direction;
        this.id = nanoid();
		logger.debug(`constructor(props=${props}): this.direction → ${this.direction}`);
		logger.debug(`constructor(props=${props}): id → ${id}`);
        this.state = {};
		logger.debug(`constructor(props=${props}): this.state → ${this.state}`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }


    onMouseDown() {
		logger.debug('>>> Entering onMouseDown()');
        if (!this.state.arrow_bullet) {
            this.arrow_bullet = document.getElementById(this.id);
        }
        this.animate(this.props.shouldAnimate);
		logger.debug('<<< Exiting onMouseDown()');
    }

    animate(bool) {
		logger.debug(`>>> Entering animate(bool=${bool})`);
        const left = () => {
			logger.debug('>>> Entering left()');
            this.setState({ selected: !this.state.selected });
            const arrow_css = this.arrow_bullet.style;
            arrow_css.transform = (this.state.selected) ? "rotate(0deg)" : "rotate(90deg)";
			logger.debug('<<< Exiting left()');
        };
        const right = () => null;
        bool ? left() : right();
		logger.debug(`<<< Exiting animate(bool=${bool})`);
    }

    reset(bool) {
		logger.debug(`>>> Entering reset(bool=${bool})`);
        if (bool) {
            this.setState({ selected: false });
			logger.debug(`<<< Exiting reset(bool=${bool})`);
        }
    }


    render() {
		logger.debug('>>> Entering render()');
         
        const marginTop = styles.margin({ top: -3 });
		logger.debug(`render(): marginTop → ${marginTop}`);
        const padding = styles.padding({ all: 2 });
		logger.debug(`render(): padding → ${padding}`);
        const baseStyles = style(this.direction);
		logger.debug(`render(): baseStyles → ${baseStyles}`);
        const height = {height:"20px"};
		logger.debug('<<< Exiting render()');
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
