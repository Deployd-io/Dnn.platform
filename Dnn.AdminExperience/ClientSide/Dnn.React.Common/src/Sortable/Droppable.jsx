import React from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
import interact from "interact.js";
const logger = console;

export default (Component) => { 
    
    class Droppable extends React.Component {
    
        raiseEvent(event, callback) {
			logger.error(`>>> Entering raiseEvent(event=${event},callback=${callback})`, { "event.action": "raiseEvent", "logrite.event": event, "logrite.callback": callback });
            const dropZoneRect = event.dropzone._element.getBoundingClientRect();
            const elementRect = event.relatedTarget.getBoundingClientRect();            
			logger.error(`raiseEvent(event=${event},callback=${callback}): dropZoneRect → ${dropZoneRect}`, { "event.action": "raiseEvent", "logrite.event": event, "logrite.callback": callback });
			logger.error(`raiseEvent(event=${event},callback=${callback}): elementRect → ${elementRect}`, { "event.action": "raiseEvent", "logrite.event": event, "logrite.callback": callback });
            const x = elementRect.left - dropZoneRect.left + elementRect.width / 2;
            const y = elementRect.top - dropZoneRect.top + elementRect.height / 2;
            if (callback) {
                callback(event, x, y);
				logger.warn(`<<< Exiting raiseEvent(event=${event},callback=${callback})`, { "event.action": "raiseEvent", "logrite.event": event, "logrite.callback": callback });
            }
        }
        
        onDrop(event) {
			logger.error(`>>> Entering onDrop(event=${event})`, { "event.action": "onDrop", "logrite.event": event });
            this.raiseEvent(event, this.props.onDrop);
			logger.warn(`<<< Exiting onDrop(event=${event})`, { "event.action": "onDrop", "logrite.event": event });
        }
    
        onDropMove(event) {
			logger.error(`>>> Entering onDropMove(event=${event})`, { "event.action": "onDropMove", "logrite.event": event });
            this.raiseEvent(event, this.props.onDropMove);
			logger.warn(`<<< Exiting onDropMove(event=${event})`, { "event.action": "onDropMove", "logrite.event": event });
        }
    
        componentDidMount() {            
			logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
            const node = ReactDOM.findDOMNode(this);
            if (!node) {
                return;
            }
            interact.dynamicDrop(true);
            
            interact(ReactDOM.findDOMNode(this)).dropzone({
                accept: ".drag-element",
                overlap: 0.50,
                ondropactivate: this.props.onDropActivate,
                ondragenter: this.props.onDragEnter,
                ondragleave:  this.props.onDragLeave,
                ondrop: this.onDrop.bind(this),
                ondropmove: this.onDropMove.bind(this),
                ondropdeactivate: this.props.onDropDeactivate
            });
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
        
        render() {
            return (<Component {...this.props} />);
        }
    }
    Droppable.propTypes = {     
        onDrop: PropTypes.func,
        onDropMove: PropTypes.func,
        onDropActivate: PropTypes.func,
        onDragEnter: PropTypes.func,
        onDragLeave: PropTypes.func,
        onDropDeactivate: PropTypes.func
    };
    return Droppable;
};