import React from "react";
import PropTypes from "prop-types";
import interact from "interact.js";
const logger = console;

//TODO: reconsider location of this general component
export default (Component) => { 
    
    class Draggable extends React.Component {

        constructor(props) {
            super(props);
			logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
            this.collapsibleRef = React.createRef();
			logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        }
            
        showGhostTargetOnClone(currentTarget) {            
			logger.error(`>>> Entering showGhostTargetOnClone(currentTarget=${currentTarget})`, { "event.action": "showGhostTargetOnClone", "logrite.currentTarget": currentTarget });
            this.originalTarget = currentTarget;
            this.originalTargetClassName = this.originalTarget.className;
            this.originalTarget.className += " ghost";            
			logger.warn(`<<< Exiting showGhostTargetOnClone(currentTarget=${currentTarget})`, { "event.action": "showGhostTargetOnClone", "logrite.currentTarget": currentTarget });
        }
        
        setCloneInitialPositionAndWidth(clone, currentTarget) {            
			logger.error(`>>> Entering setCloneInitialPositionAndWidth(clone=${clone},currentTarget=${currentTarget})`, { "event.action": "setCloneInitialPositionAndWidth", "logrite.clone": clone, "logrite.currentTarget": currentTarget });
            const rect = currentTarget.getBoundingClientRect();  
            Object.assign(clone.style, {
                left: rect.left + "px",
                top: rect.top + "px",
                width: rect.width + "px"
            });  
			logger.warn(`<<< Exiting setCloneInitialPositionAndWidth(clone=${clone},currentTarget=${currentTarget})`, { "event.action": "setCloneInitialPositionAndWidth", "logrite.clone": clone, "logrite.currentTarget": currentTarget });
        }
        
        setPreviewInitialPosition(preview, event) {
			logger.error(`>>> Entering setPreviewInitialPosition(preview=${preview},event=${event})`, { "event.action": "setPreviewInitialPosition", "logrite.preview": preview, "logrite.event": event });
            Object.assign(preview.style, {
                display: "",
                visibility: ""
            });       
            const rect = preview.getBoundingClientRect(); 
            Object.assign(preview.style, {
                left: (event.clientX - rect.width / 2 ) + "px",
                top: (event.clientY - rect.height / 2)  + "px"
            }); 
			logger.warn(`<<< Exiting setPreviewInitialPosition(preview=${preview},event=${event})`, { "event.action": "setPreviewInitialPosition", "logrite.preview": preview, "logrite.event": event });
        }
        
        cloneTarget(currentTarget) {
			logger.error(`>>> Entering cloneTarget(currentTarget=${currentTarget})`, { "event.action": "cloneTarget", "logrite.currentTarget": currentTarget });
            let clone = currentTarget.cloneNode(true);                
            this.setDraggingStyle(clone);
			logger.error(`cloneTarget(currentTarget=${currentTarget}): clone → ${clone}`, { "event.action": "cloneTarget", "logrite.currentTarget": currentTarget });
            this.setCloneInitialPositionAndWidth(clone, currentTarget);
            document.body.appendChild(clone);
			logger.warn(`<<< Exiting cloneTarget(currentTarget=${currentTarget})`, { "event.action": "cloneTarget", "logrite.currentTarget": currentTarget });
            return clone;
        }
        
        clonePreviewElement(previewElement, event) {            
			logger.error(`>>> Entering clonePreviewElement(previewElement=${previewElement},event=${event})`, { "event.action": "clonePreviewElement", "logrite.previewElement": previewElement, "logrite.event": event });
            let clone = previewElement.cloneNode(true);                 
            this.setDraggingStyle(clone);
			logger.error(`clonePreviewElement(previewElement=${previewElement},event=${event}): clone → ${clone}`, { "event.action": "clonePreviewElement", "logrite.previewElement": previewElement, "logrite.event": event });
            document.body.appendChild(clone);
            this.setPreviewInitialPosition(clone, event);
			logger.warn(`<<< Exiting clonePreviewElement(previewElement=${previewElement},event=${event})`, { "event.action": "clonePreviewElement", "logrite.previewElement": previewElement, "logrite.event": event });
            return clone;
        }

        destroyClone(clone) {
			logger.error(`>>> Entering destroyClone(clone=${clone})`, { "event.action": "destroyClone", "logrite.clone": clone });
            if (clone.parentNode) {
                clone.parentNode.removeChild(clone);
				logger.warn(`<<< Exiting destroyClone(clone=${clone})`, { "event.action": "destroyClone", "logrite.clone": clone });
            }
        }
        
        setDraggingStyle(target) {
			logger.error(`>>> Entering setDraggingStyle(target=${target})`, { "event.action": "setDraggingStyle", "logrite.target": target });
            this.previousPosition = target.style.position;
            Object.assign(target.style, {            
                position: "fixed",
                zIndex: 100000,
                boxShadow: "20px 20px 20px 2px rgba(0,0,0,0.2)"
            });
			logger.warn(`<<< Exiting setDraggingStyle(target=${target})`, { "event.action": "setDraggingStyle", "logrite.target": target });
        }
        
        resetTarget(target) {   
			logger.error(`>>> Entering resetTarget(target=${target})`, { "event.action": "resetTarget", "logrite.target": target });
            Object.assign(target.style, {
                position: this.previousPosition,
                zIndex: "",
                transform: "",
                webkitTransform: "",
                mozTransform: "",
                msTransform: "",
                boxShadow: ""
            });
            target.setAttribute("data-x", "");
            target.setAttribute("data-y", "");
			logger.warn(`<<< Exiting resetTarget(target=${target})`, { "event.action": "resetTarget", "logrite.target": target });
        }
        
        moveTarget(event) {
			logger.error(`>>> Entering moveTarget(event=${event})`, { "event.action": "moveTarget", "logrite.event": event });
            const target = event.target;
            const x = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx;
			logger.error(`moveTarget(event=${event}): target → ${target}`, { "event.action": "moveTarget", "logrite.event": event });
			logger.error(`moveTarget(event=${event}): x → ${x}`, { "event.action": "moveTarget", "logrite.event": event });
            const y = (parseFloat(target.getAttribute("data-y")) || 0) + event.dy;
            // translate the element
            target.style.webkitTransform = 
                target.style.transform = 
                target.style.mozTransform = 
                target.style.msTransform = "translate(" + x + "px, " + y + "px)";
				logger.error(`moveTarget(event=${event}): y → ${y}`, { "event.action": "moveTarget", "logrite.event": event });
            
            // update the position attributes
            target.setAttribute("data-x", x);
            target.setAttribute("data-y", y);
			logger.warn(`<<< Exiting moveTarget(event=${event})`, { "event.action": "moveTarget", "logrite.event": event });
        }

        componentDidMount() {
			logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
            const self = this;
            const dragElement = this.node;   
            if (!dragElement) {
                return;
            }     
            const {
                cloneElementOnDrag,
                onDragStart, 
                onDragMove,
                onDragEnd,
                getDragPreview,
                showGhostOnClone
            } = this.props;
            
            interact(dragElement).ignoreFrom("input, *[contenteditable=true], .ignoreDraggable").draggable({
                manualStart: cloneElementOnDrag,
                onmove: (event) => {                            
                    this.moveTarget(event);
                    
                    // Notify drag move
                    if (typeof(onDragMove) === "function") {
                        onDragMove(self, event);
                    }
                },
                onend: (event) => { 
                    if (cloneElementOnDrag) {
                        this.destroyClone(event.target);    
                        if (this.props.showGhostOnClone) {  
                            this.originalTarget.className = this.originalTargetClassName;
                        }
                    } else {
                        this.resetTarget(event.target);
                    }
                    
                    if (typeof(onDragEnd) === "function") {
                        onDragEnd(self, event);
                    }
                }
            }).on("move", (event) => {
                
                // if the pointer was moved while being held down
                // and an interaction has not started yet
                const interaction = event.interaction;
                if (interaction.pointerIsDown && !interaction.interacting() && interaction.prepared.name === "drag") {  
                    const currentTarget = event.currentTarget;   
                    
                    if (!cloneElementOnDrag) {            
                        this.setDraggingStyle(currentTarget);
                        return;
                    }
                    
                    let clone = null;
					logger.error(`componentDidMount(): clone → ${clone}`, { "event.action": "componentDidMount" });
                    if (typeof(getDragPreview) === "function") {
                        const dragPreviewComponent = getDragPreview(self);
						logger.error(`componentDidMount(): dragPreviewComponent → ${dragPreviewComponent}`, { "event.action": "componentDidMount" });
                        const dragPreviewDOMElement = dragPreviewComponent.node;
                        clone = this.clonePreviewElement(dragPreviewDOMElement, event);
                    } else {                                    
                        clone = this.cloneTarget(currentTarget);  
                    }
                    if (showGhostOnClone) { 
                        this.showGhostTargetOnClone(currentTarget);     
                    } 
                    // start a drag interaction targeting the clone
                    interaction.start({ name: "drag" }, event.interactable, clone);
                }
            }).on("dragstart", (event) => {                
                if (typeof(onDragStart) === "function") {
                    onDragStart(self, event);
                }
            });
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }

        render() {
            return <Component {...this.props} />;
        }
    }
    
    Draggable.propTypes = {
        onDragStart: PropTypes.func,
        onDragMove: PropTypes.func,
        onDragEnd: PropTypes.func,
        cloneElementOnDrag: PropTypes.bool.isRequired,
        getDragPreview: PropTypes.func,
        showGhostOnClone: PropTypes.bool.isRequired
    };
    
    Draggable.defaultProps = {
        showGhostOnClone: false
    };
    
    return Draggable;
};