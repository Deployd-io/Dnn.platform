import React, { Component } from "react";
import PropTypes from "prop-types";
import Portal from "./Portal";
import TransitionModal from "./TransitionModal";
import TransitionModalContent from "./TransitionModalContent";

import "./style.less";
const logger = console;


class PortableTransitionModal extends Component {

    render() {
		logger.debug('>>> Entering render()');
        let childrenWithProps = false;
        if (this.props.children) {
            childrenWithProps = React.Children.map(this.props.children,
                (child) => {
                    if (!child) {
                        return;
                    }
                    return React.cloneElement(child, {
                        onClose: this.props.onClose
                    });
                }
            );
        }

        const content = childrenWithProps || <TransitionModalContent
            onClose={this.props.onClose}
            onOk={this.props.onOk}
            message={this.props.message}
            header={this.props.header}
            showCancelButton={this.props.showCancelButton}
            cancelButtonText={this.props.cancelButtonText}
            showOkButton={this.props.showOkButton}
            okButtonText={this.props.okButtonText}
        />;
		logger.debug('<<< Exiting render()');
        return <Portal portalId={this.props.portalId} className={this.props.className}>
            <TransitionModal onCloseModal={this.props.onClose}>
                {content}
            </TransitionModal>
        </Portal>;
    }
}

PortableTransitionModal.propTypes = {
    onClose: PropTypes.func.isRequired,
    onOk: PropTypes.func,

    message: PropTypes.string,
    header: PropTypes.string,
    showCancelButton: PropTypes.bool,
    cancelButtonText:PropTypes.string,
    
    showOkButton: PropTypes.bool,
    okButtonText: PropTypes.string,
    className: PropTypes.string,
    children: PropTypes.node,
    portalId: PropTypes.string
};

export default PortableTransitionModal;