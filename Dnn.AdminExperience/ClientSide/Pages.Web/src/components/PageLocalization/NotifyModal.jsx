import { TransitionModal } from "@dnnsoftware/dnn-react-common";
import React, {Component} from "react";
import PropTypes from "prop-types";
import Notify from "./Notify";

import "./NotifyModal.less";
const logger = console;


class NotifyModal extends Component {
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <TransitionModal onCloseModal={this.props.onClose}>
            <Notify 
                onSend={this.props.onSend}
                onUpdateMessage={this.props.onUpdateMessage}
                notifyMessage={this.props.notifyMessage}
            />
        </TransitionModal>;
    }
}

NotifyModal.propTypes = {
    onClose: PropTypes.func.isRequired,
    onSend: PropTypes.func.isRequired,
    onUpdateMessage: PropTypes.func.isRequired,
    notifyMessage: PropTypes.string.isRequired
};

export default NotifyModal;