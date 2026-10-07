import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

export default class TransitionModal extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            isOpen: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        setTimeout(() => { this.setState({ isOpen: true }); }, 0);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    onCloseModal() {
		logger.error('>>> Entering onCloseModal()', { "event.action": "onCloseModal" });
        if (this.props.onCloseModal) {
            setTimeout(this.props.onCloseModal, 400);
			logger.warn('<<< Exiting onCloseModal()', { "event.action": "onCloseModal" });
        }
    }

    onClose() {
		logger.error('>>> Entering onClose()', { "event.action": "onClose" });
        this.setState({ isOpen: false }, this.onCloseModal);
		logger.warn('<<< Exiting onClose()', { "event.action": "onClose" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const className = "transition-modal" + (this.state.isOpen ? " open" : "");
        const childrenWithProps = React.Children.map(this.props.children,
            (child) => {
                if (!child) {
                    return;
                }
                return React.cloneElement(child, {
                    onClose: this.onClose.bind(this)
                });
            }
        );
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={className}>
                <div className="overlay"></div>
                <div className="modal-content">
                    <div className="close" onClick={this.onClose.bind(this) }>×</div>
                    {childrenWithProps}
                </div>
            </div>
        );
    }
}

TransitionModal.propTypes = {
    onCloseModal: PropTypes.func,
    children: PropTypes.node
};