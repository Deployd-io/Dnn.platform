import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
import {Scrollbars} from "react-custom-scrollbars";
import { XThinIcon } from "../SvgIcons";
import "./style.less";
const logger = console;


class Modal extends Component {
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        if (this.props.isOpen) {
            this.handleModalOpened();
			logger.debug('<<< Exiting componentDidMount()');
        }
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        if (!prevProps.isOpen && this.props.isOpen) {
            this.handleModalOpened();
        } else if (prevProps.isOpen && !this.props.isOpen) {
            this.handleModalClosed();
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        this.handleModalClosed();
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    handleModalOpened() {
		logger.debug('>>> Entering handleModalOpened()');
        if (document && document.body) {
            document.body.classList.add("ReactModal__Body--open");
        }
        if (this.props.onAfterOpen) {
            this.props.onAfterOpen();
			logger.debug('<<< Exiting handleModalOpened()');
        }
    }

    handleModalClosed() {
		logger.debug('>>> Entering handleModalClosed()');
        if (document && document.body) {
            document.body.classList.remove("ReactModal__Body--open");
			logger.debug('<<< Exiting handleModalClosed()');
        }
    }

    onOverlayMouseDown() {
		logger.debug('>>> Entering onOverlayMouseDown()');
        if (this.props.shouldCloseOnOverlayClick && this.props.onRequestClose) {
            this.props.onRequestClose();
			logger.debug('<<< Exiting onOverlayMouseDown()');
        }
    }

    onContentMouseDown(event) {
		logger.debug(`>>> Entering onContentMouseDown(event=${event})`);
        event.stopPropagation();
		logger.debug(`<<< Exiting onContentMouseDown(event=${event})`);
    }

    onPortalKeyDown(event) {
		logger.debug(`>>> Entering onPortalKeyDown(event=${event})`);
        if (event.key === "Escape" && this.props.onRequestClose) {
            this.props.onRequestClose();
			logger.debug(`<<< Exiting onPortalKeyDown(event=${event})`);
        }
    }

    getScrollbarStyle(props) {
		logger.debug(`>>> Entering getScrollbarStyle(props=${props})`);
		logger.debug(`<<< Exiting getScrollbarStyle(props=${props})`);
        return {
            width: "100%",
            height: props.header ? "calc(100% - 55px)" : "100%",
            boxSizing: "border-box",
            padding: "25px 30px"
        };
    }
    getModalStyles(props) {
		logger.debug(`>>> Entering getModalStyles(props=${props})`);
        let modalWidth = props.modalWidth;
        let modalTopMargin = props.modalTopMargin;
		logger.debug(`getModalStyles(props=${props}): modalWidth → ${modalWidth}`);
		logger.debug(`getModalStyles(props=${props}): modalTopMargin → ${modalTopMargin}`);
        if (document.getElementsByClassName("socialpanel") && document.getElementsByClassName("socialpanel").length > 0 && !props.modalWidth) {
            modalWidth = document.getElementsByClassName("socialpanel")[0].offsetWidth;
        }
        if (document.getElementsByClassName("dnn-persona-bar-page-header") && document.getElementsByClassName("dnn-persona-bar-page-header").length > 0 && !props.modalHeight) {
            modalTopMargin = document.getElementsByClassName("dnn-persona-bar-page-header")[0].offsetHeight;
        }
        const defaultStyles = {
            overlay: {
                position: "fixed",
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                zIndex: "99999",
                backgroundColor: "rgba(0,0,0,0.6)"
            },
            content: {
                top: modalTopMargin + props.dialogVerticalMargin,
                left: props.dialogHorizontalMargin + 85,
                right: "auto",
                bottom: "auto",
                padding: 0,
                borderRadius: 0,
                border: "none",
                width: modalWidth - props.dialogHorizontalMargin * 2,
                height: props.modalHeight || "60%",
                backgroundColor: "#FFFFFF",
                position: "absolute",
                overflow: "auto",
                WebkitOverflowScrolling: "touch",
                outline: "none",
                userSelect: "none",
                WebkitUserSelect: "none",
                MozUserSelect: "none",
                MsUserSelect: "none",
                boxSizing: "border-box"
            }
        };

        const customOverlay = props.style && props.style.overlay ? props.style.overlay : {};
        const customContent = props.style && props.style.content ? props.style.content : {};

		logger.debug(`<<< Exiting getModalStyles(props=${props})`);
        return {
            overlay: {
                ...defaultStyles.overlay,
                ...customOverlay
            },
            content: {
                ...defaultStyles.content,
                ...customContent
            }
        };
    }
     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        if (!props.isOpen || !document || !document.body) {
            return null;
        }

        const modalStyles = this.getModalStyles(props);
		logger.debug(`render(): modalStyles → ${modalStyles}`);
        const scrollBarStyle = this.getScrollbarStyle(props);
		logger.debug('<<< Exiting render()');
        return ReactDOM.createPortal(
            <div className="ReactModalPortal" onKeyDown={this.onPortalKeyDown.bind(this)}>
                <div
                    className="ReactModal__Overlay"
                    style={modalStyles.overlay}
                    onMouseDown={this.onOverlayMouseDown.bind(this)}>
                    <div
                        className="ReactModal__Content"
                        style={modalStyles.content}
                        role="dialog"
                        aria-modal="true"
                        onMouseDown={this.onContentMouseDown.bind(this)}>
                        {props.header &&
                            <div className="modal-header">
                                <h3>{props.header}</h3>
                                {props.headerChildren}
                                <div className="close-modal-button" onClick={props.onRequestClose}>
                                    <XThinIcon />
                                </div>
                            </div>
                        }
                        <Scrollbars style={scrollBarStyle}>
                            <div style={props.contentStyle}>
                                {props.children}
                            </div>
                        </Scrollbars>
                    </div>
                </div>
            </div>,
            document.body
        );
    }
}
Modal.propTypes = {
    isOpen: PropTypes.bool,
    style: PropTypes.object,
    onRequestClose: PropTypes.func,
    children: PropTypes.node,
    dialogVerticalMargin: PropTypes.number,
    dialogHorizontalMargin: PropTypes.number,
    modalWidth: PropTypes.number,
    modalHeight: PropTypes.number,
    modalTopMargin: PropTypes.number,
    header: PropTypes.string,
    headerChildren: PropTypes.node,
    contentStyle: PropTypes.object,
    onAfterOpen: PropTypes.func,
    closeTimeoutMS: PropTypes.number,
    shouldCloseOnOverlayClick: PropTypes.bool
};
Modal.defaultProps = {
    modalWidth: 861,
    modalTopMargin: 100,
    dialogVerticalMargin: 25,
    dialogHorizontalMargin: 30,
    contentStyle: { padding: "25px 30px" },
    shouldCloseOnOverlayClick: true
};
export default Modal;