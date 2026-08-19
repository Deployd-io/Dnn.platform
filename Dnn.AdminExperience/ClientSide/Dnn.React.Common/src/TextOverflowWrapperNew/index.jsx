import React, { Component } from "react";
import PropTypes from "prop-types";
import { Tooltip as ReactTooltip } from "react-tooltip";

import "./style.less";
const logger = console;

const generateID = () => "a" + Math.random().toString(36).substr(2, 10);
class TextOverflowWrapperNew extends Component {
  constructor() {
    super();
	logger.debug('>>> Entering constructor()');
    this.state = {
      isTooltipActive: false,
      id: generateID(),
    };
	logger.debug('<<< Exiting constructor()');
  }

  showTooltip() {
	logger.debug('>>> Entering showTooltip()');
    this.setState({
      isTooltipActive: true,
    });
	logger.debug('<<< Exiting showTooltip()');
  }

  hideTooltip() {
	logger.debug('>>> Entering hideTooltip()');
    this.setState({
      isTooltipActive: false,
    });
	logger.debug('<<< Exiting hideTooltip()');
  }

  render() {
	logger.debug('>>> Entering render()');
    const { props, state } = this;

    const hotspotStyles = {
      marginLeft: "20px",
      backgroundColor: "transparent",
      position: "absolute",
      top: 0,
      left: 0,
      height: "20px",
      width: "200px",
      zIndex: 10000,
    };

	logger.debug('<<< Exiting render()');
    return (
      <div>
        <div
          data-tooltip-id={state.id}
          style={props.hotspotStyles || hotspotStyles}
          id={state.id}
        ></div>
        <ReactTooltip
          id={state.id}
          variant={props.type || "light"}
          place={props.place || "bottom"}
          float={props.float !== false}
          offset={props.offset}
          className={props.className || "page-picker-tooltip-style"}
          border={props.bool || false}
        >
          <div>{props.text}</div>
        </ReactTooltip>
      </div>
    );
  }
}

TextOverflowWrapperNew.propTypes = {
  text: PropTypes.string,
  hotspotStyles: PropTypes.object,
  type: PropTypes.string,
  place: PropTypes.string,
  float: PropTypes.bool,
  offset: PropTypes.number,
  className: PropTypes.string,
  border: PropTypes.bool,
};

export default TextOverflowWrapperNew;

