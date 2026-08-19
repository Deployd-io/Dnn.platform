import React from "react";
import ReactDOM from "react-dom";
import PropTypes from "prop-types";
import interact from "interact.js";
const logger = console;

//TODO: reconsider location of this general component
export default (Component) => { 
    
    class Draggable extends React.Component {

        constructor() {
            super();
			logger.debug('>>> Entering constructor()');
            this.initialScrollY = 0;
			logger.debug(`constructor(): this.initialScrollY → ${this.initialScrollY}`);
			logger.debug('<<< Exiting constructor()');
        }

        showGhostTargetOnClone(currentTarget) {            
			logger.debug(`>>> Entering showGhostTargetOnClone(currentTarget=${currentTarget})`);
            this.originalTarget = currentTarget;
            this.originalTargetClassName = this.originalTarget.className;
            this.originalTarget.className += " ghost";            
			logger.debug(`<<< Exiting showGhostTargetOnClone(currentTarget=${currentTarget})`);
        }
        
        setCloneInitialPositionAndWidth(clone, currentTarget) {            
			logger.debug(`>>> Entering setCloneInitialPositionAndWidth(clone=${clone},currentTarget=${currentTarget})`);
            const rect = currentTarget.getBoundingClientRect();  
            Object.assign(clone.style, {
                left: rect.left + "px",
                top: rect.top + "px",
                width: rect.width + "px"
            });  
			logger.debug(`<<< Exiting setCloneInitialPositionAndWidth(clone=${clone},currentTarget=${currentTarget})`);
        }
        
        setPreviewInitialPosition(preview, event) {
			logger.debug(`>>> Entering setPreviewInitialPosition(preview=${preview},event=${event})`);
            Object.assign(preview.style, {
                display: "",
                visibility: ""
            });       
            const rect = preview.getBoundingClientRect(); 
            Object.assign(preview.style, {
                left: (event.clientX - rect.width / 2 ) + "px",
                top: (event.clientY - rect.height / 2)  + "px"
            }); 
			logger.debug(`<<< Exiting setPreviewInitialPosition(preview=${preview},event=${event})`);
        }
        
        cloneTarget(currentTarget) {
			logger.debug(`>>> Entering cloneTarget(currentTarget=${currentTarget})`);
            let clone = currentTarget.cloneNode(true);                
            this.setDraggingStyle(clone);
			logger.debug(`cloneTarget(currentTarget=${currentTarget}): clone → ${clone}`);
            this.setCloneInitialPositionAndWidth(clone, currentTarget);
            document.body.appendChild(clone);
			logger.debug(`<<< Exiting cloneTarget(currentTarget=${currentTarget})`);
            return clone;
        }
        
        clonePreviewElement(previewElement, event) {            
			logger.debug(`>>> Entering clonePreviewElement(previewElement=${previewElement},event=${event})`);
            let clone = previewElement.cloneNode(true);                 
            this.setDraggingStyle(clone);
			logger.debug(`clonePreviewElement(previewElement=${previewElement},event=${event}): clone → ${clone}`);
            document.body.appendChild(clone);
            this.setPreviewInitialPosition(clone, event);
			logger.debug(`<<< Exiting clonePreviewElement(previewElement=${previewElement},event=${event})`);
            return clone;
        }

        destroyClone(clone) {
			logger.debug(`>>> Entering destroyClone(clone=${clone})`);
            if (clone.parentNode) {
                clone.parentNode.removeChild(clone);
				logger.debug(`<<< Exiting destroyClone(clone=${clone})`);
            }
        }
        
        setDraggingStyle(target) {
			logger.debug(`>>> Entering setDraggingStyle(target=${target})`);
            this.previousPosition = target.style.position;
            Object.assign(target.style, {            
                position: "fixed",
                zIndex: 100000,
                boxShadow: "20px 20px 20px 2px rgba(0,0,0,0.2)"
            });
			logger.debug(`<<< Exiting setDraggingStyle(target=${target})`);
        }
        
        resetTarget(target) {   
			logger.debug(`>>> Entering resetTarget(target=${target})`);
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
			logger.debug(`<<< Exiting resetTarget(target=${target})`);
        }
        
        moveTarget(event) {
			logger.debug(`>>> Entering moveTarget(event=${event})`);
            const target = event.target;
            const x = (parseFloat(target.getAttribute("data-x")) || 0) + event.dx;
			logger.debug(`moveTarget(event=${event}): target → ${target}`);
			logger.debug(`moveTarget(event=${event}): x → ${x}`);
            const afterScroll = this.initialScrollY - window.scrollY;
            this.initialScrollY = window.scrollY;
            const y = ((parseFloat(target.getAttribute("data-y")) || 0) + event.dy) + afterScroll;
			logger.debug(`moveTarget(event=${event}): this.initialScrollY → ${this.initialScrollY}`);
			logger.debug(`moveTarget(event=${event}): y → ${y}`);
            
            // translate the element
            target.style.webkitTransform = 
                target.style.transform = 
                target.style.mozTransform = 
                target.style.msTransform = "translate(" + x + "px, " + y + "px)";
            
            // update the position attributes
            target.setAttribute("data-x", x);
            target.setAttribute("data-y", y);
			logger.debug(`<<< Exiting moveTarget(event=${event})`);
        }

        componentDidMount() {
			logger.debug('>>> Entering componentDidMount()');
            const self = this;
            const dragElement = ReactDOM.findDOMNode(this);   
			logger.debug(`componentDidMount(): dragElement → ${dragElement}`);
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
					logger.debug(`componentDidMount(): clone → ${clone}`);
                    if (typeof(getDragPreview) === "function") {
                        const dragPreviewComponent = getDragPreview(self);
						logger.debug(`componentDidMount(): dragPreviewComponent → ${dragPreviewComponent}`);
                        const dragPreviewDOMElement = ReactDOM.findDOMNode(dragPreviewComponent);
						logger.debug(`componentDidMount(): dragPreviewDOMElement → ${dragPreviewDOMElement}`);
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
                this.initialScrollY = window.scrollY;
                if (typeof(onDragStart) === "function") {
                    onDragStart(self, event);
                }
            });
			logger.debug('<<< Exiting componentDidMount()');
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