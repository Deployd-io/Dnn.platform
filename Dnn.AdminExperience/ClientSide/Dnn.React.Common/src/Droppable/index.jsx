import React from "react";
import PropTypes from "prop-types";
import interact from "interact.js";
const logger = console;

export default (Component) => { 
    
    class Droppable extends React.Component {
    
        raiseEvent(event, callback) {
			logger.debug(`>>> Entering raiseEvent(event=${event},callback=${callback})`);
            const dropZoneRect = event.dropzone._element.getBoundingClientRect();
            const elementRect = event.relatedTarget.getBoundingClientRect();            
			logger.debug(`raiseEvent(event=${event},callback=${callback}): dropZoneRect → ${dropZoneRect}`);
			logger.debug(`raiseEvent(event=${event},callback=${callback}): elementRect → ${elementRect}`);
            const x = elementRect.left - dropZoneRect.left + elementRect.width / 2;
            const y = elementRect.top - dropZoneRect.top + elementRect.height / 2;
            if (callback) {
                callback(event, x, y);
				logger.debug(`<<< Exiting raiseEvent(event=${event},callback=${callback})`);
            }
        }
        
        onDrop(event) {
			logger.debug(`>>> Entering onDrop(event=${event})`);
            this.raiseEvent(event, this.props.onDrop);
			logger.debug(`<<< Exiting onDrop(event=${event})`);
        }
    
        onDropMove(event) {
			logger.debug(`>>> Entering onDropMove(event=${event})`);
            this.raiseEvent(event, this.props.onDropMove);
			logger.debug(`<<< Exiting onDropMove(event=${event})`);
        }
    
        componentDidMount() {            
			logger.debug('>>> Entering componentDidMount()');
            const node = this.node;
            if (!node) {
                return;
            }
            interact.dynamicDrop(true);
            
            interact(this.node).dropzone({
                accept: ".drag-element",
                overlap: 0.50,
                ondropactivate: this.props.onDropActivate,
                ondragenter: this.props.onDragEnter,
                ondragleave:  this.props.onDragLeave,
                ondrop: this.onDrop.bind(this),
                ondropmove: this.onDropMove.bind(this),
                ondropdeactivate: this.props.onDropDeactivate
            });
			logger.debug('<<< Exiting componentDidMount()');
        }
        
        render() {
            return (<Component ref={node => this.node = node} {...this.props} />);
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