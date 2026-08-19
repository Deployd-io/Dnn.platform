import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
const logger = console;

export default class TransitionModal extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            isOpen: false
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        setTimeout(() => { this.setState({ isOpen: true }); }, 0);
		logger.debug('<<< Exiting componentDidMount()');
    }

    onCloseModal() {
		logger.debug('>>> Entering onCloseModal()');
        if (this.props.onCloseModal) {
            setTimeout(this.props.onCloseModal, 400);
			logger.debug('<<< Exiting onCloseModal()');
        }
    }

    onClose() {
		logger.debug('>>> Entering onClose()');
        this.setState({ isOpen: false }, this.onCloseModal);
		logger.debug('<<< Exiting onClose()');
    }

    render() {
		logger.debug('>>> Entering render()');
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
		logger.debug('<<< Exiting render()');
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