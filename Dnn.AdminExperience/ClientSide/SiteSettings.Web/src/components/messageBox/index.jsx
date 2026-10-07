import React, { Component } from "react";
import PropTypes from "prop-types";
import Modal from "react-modal";
const logger = console;
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

const modalStyles = {
    overlay: {
        zIndex: "9999",
        backgroundColor: "rgba(0, 0, 0, 0)"
    },
    content: {
        position: "fixed",
        top: "30%",
        left: "370px",
        right: "initial",
        bottom: "initial",
        borderRadius: 0,
        border: "none",
        width: "485px",
        padding: 0,
        userSelect: "none",
        WebkitUserSelect: "none",
        MozUserSelect: "none",
        MsUserSelect: "none",
        opacity: ".9"
    }
};

class MessageBox extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    onClose() {
		logger.error('>>> Entering onClose()', { "event.action": "onClose" });
        const {props} = this;
        props.onClose();
		logger.warn('<<< Exiting onClose()', { "event.action": "onClose" });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <Modal
                isOpen={props.isOpened}
                style={modalStyles}>
                <div className="dnn-message-box">
                    <div className="top-bar">
                        <div className="close-button" onClick={this.onClose.bind(this)}><SvgIcons.XThinIcon /></div>
                    </div>
                    <div className="content">
                        <div className="message-icon"><SvgIcons.CheckMarkIcon /></div>
                        <div className="message-text">{props.message}</div>
                        {props.link &&
                            <div className="message-link"><a target="_blank" rel="noopener noreferrer" href={props.link}>{props.link}</a></div>
                        }
                    </div>
                </div>
            </Modal>
        );
    }
}

MessageBox.propTypes = {
    message: PropTypes.string.isRequired,
    link: PropTypes.string,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired
};

MessageBox.defaultProps = {
    isOpened: false
};

export default MessageBox;