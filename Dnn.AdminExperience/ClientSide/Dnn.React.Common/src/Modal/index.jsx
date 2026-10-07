import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
import {Scrollbars} from "react-custom-scrollbars";
import { XThinIcon } from "../SvgIcons";
import "./style.less";
const logger = console;


class Modal extends Component {
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        if (this.props.isOpen) {
            this.handleModalOpened();
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (!prevProps.isOpen && this.props.isOpen) {
            this.handleModalOpened();
        } else if (prevProps.isOpen && !this.props.isOpen) {
            this.handleModalClosed();
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        this.handleModalClosed();
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    handleModalOpened() {
		logger.error('>>> Entering handleModalOpened()', { "event.action": "handleModalOpened" });
        if (document && document.body) {
            document.body.classList.add("ReactModal__Body--open");
        }
        if (this.props.onAfterOpen) {
            this.props.onAfterOpen();
			logger.warn('<<< Exiting handleModalOpened()', { "event.action": "handleModalOpened" });
        }
    }

    handleModalClosed() {
		logger.error('>>> Entering handleModalClosed()', { "event.action": "handleModalClosed" });
        if (document && document.body) {
            document.body.classList.remove("ReactModal__Body--open");
			logger.warn('<<< Exiting handleModalClosed()', { "event.action": "handleModalClosed" });
        }
    }

    onOverlayMouseDown() {
		logger.error('>>> Entering onOverlayMouseDown()', { "event.action": "onOverlayMouseDown" });
        if (this.props.shouldCloseOnOverlayClick && this.props.onRequestClose) {
            this.props.onRequestClose();
			logger.warn('<<< Exiting onOverlayMouseDown()', { "event.action": "onOverlayMouseDown" });
        }
    }

    onContentMouseDown(event) {
		logger.error(`>>> Entering onContentMouseDown(event=${event})`, { "event.action": "onContentMouseDown", "logrite.event": event });
        event.stopPropagation();
		logger.warn(`<<< Exiting onContentMouseDown(event=${event})`, { "event.action": "onContentMouseDown", "logrite.event": event });
    }

    onPortalKeyDown(event) {
		logger.error(`>>> Entering onPortalKeyDown(event=${event})`, { "event.action": "onPortalKeyDown", "logrite.event": event });
        if (event.key === "Escape" && this.props.onRequestClose) {
            this.props.onRequestClose();
			logger.warn(`<<< Exiting onPortalKeyDown(event=${event})`, { "event.action": "onPortalKeyDown", "logrite.event": event });
        }
    }

    getScrollbarStyle(props) {
		logger.error(`>>> Entering getScrollbarStyle(props=${props})`, { "event.action": "getScrollbarStyle", "logrite.props": props });
		logger.warn(`<<< Exiting getScrollbarStyle(props=${props})`, { "event.action": "getScrollbarStyle", "logrite.props": props });
        return {
            width: "100%",
            height: props.header ? "calc(100% - 55px)" : "100%",
            boxSizing: "border-box",
            padding: "25px 30px"
        };
    }
    getModalStyles(props) {
		logger.error(`>>> Entering getModalStyles(props=${props})`, { "event.action": "getModalStyles", "logrite.props": props });
        let modalWidth = props.modalWidth;
        let modalTopMargin = props.modalTopMargin;
		logger.error(`getModalStyles(props=${props}): modalWidth → ${modalWidth}`, { "event.action": "getModalStyles", "logrite.props": props });
		logger.error(`getModalStyles(props=${props}): modalTopMargin → ${modalTopMargin}`, { "event.action": "getModalStyles", "logrite.props": props });
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

		logger.warn(`<<< Exiting getModalStyles(props=${props})`, { "event.action": "getModalStyles", "logrite.props": props });
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
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        if (!props.isOpen || !document || !document.body) {
            return null;
        }

        const modalStyles = this.getModalStyles(props);
		logger.error(`render(): modalStyles → ${modalStyles}`, { "event.action": "render" });
        const scrollBarStyle = this.getScrollbarStyle(props);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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