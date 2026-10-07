import React, {Component} from "react";
import PropTypes from "prop-types";
import { Scrollbars } from "react-custom-scrollbars";
import style from "./style.module.less";
const logger = console;

class Gallery extends Component {

    getElementSize() {
		logger.error('>>> Entering getElementSize()', { "event.action": "getElementSize" });
        const { size } = this.props;
		logger.warn('<<< Exiting getElementSize()', { "event.action": "getElementSize" });
        return size === "big" ? 198 : 130;        
    }

    calculateGalleryWidth() {
		logger.error('>>> Entering calculateGalleryWidth()', { "event.action": "calculateGalleryWidth" });
        const { children } = this.props;
		logger.warn('<<< Exiting calculateGalleryWidth()', { "event.action": "calculateGalleryWidth" });
        return this.getElementSize() * children.length;
    }

    scrollToSelectedItem(scrollToIndex) {
		logger.error(`>>> Entering scrollToSelectedItem(scrollToIndex=${scrollToIndex})`, { "event.action": "scrollToSelectedItem", "logrite.scrollToIndex": scrollToIndex });
        const indexToScroll = scrollToIndex !== -1 ? scrollToIndex : 0; 
        const scrollbars = this.scrollbarsRef;
        if (scrollbars) {
            const scrollClientWidth = scrollbars.getClientWidth();
			logger.error(`scrollToSelectedItem(scrollToIndex=${scrollToIndex}): scrollClientWidth → ${scrollClientWidth}`, { "event.action": "scrollToSelectedItem", "logrite.scrollToIndex": scrollToIndex });
            const scrollLeft = indexToScroll * this.getElementSize();
			logger.error(`scrollToSelectedItem(scrollToIndex=${scrollToIndex}): scrollLeft → ${scrollLeft}`, { "event.action": "scrollToSelectedItem", "logrite.scrollToIndex": scrollToIndex });
            const currentScrollLeft = scrollbars.getScrollLeft();
			logger.error(`scrollToSelectedItem(scrollToIndex=${scrollToIndex}): currentScrollLeft → ${currentScrollLeft}`, { "event.action": "scrollToSelectedItem", "logrite.scrollToIndex": scrollToIndex });
            const lowBoundary = currentScrollLeft;
            const highBoundary = currentScrollLeft + scrollClientWidth;        
            if (scrollLeft > lowBoundary && scrollLeft < highBoundary) {
                return;
            }
            this.scrollbarsRef.scrollLeft(scrollLeft);
        }
		logger.warn(`<<< Exiting scrollToSelectedItem(scrollToIndex=${scrollToIndex})`, { "event.action": "scrollToSelectedItem", "logrite.scrollToIndex": scrollToIndex });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        setTimeout(() => this.scrollToSelectedItem(this.props.scrollToIndex), 0);
		logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
    }
    
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        setTimeout(() => this.scrollToSelectedItem(this.props.scrollToIndex), 0);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    render() {  
		logger.error('>>> Entering render()', { "event.action": "render" });
        const width = this.calculateGalleryWidth();    
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={style.moduleContainer}>
                <Scrollbars ref={this.scrollbarsRef}
                    className="container"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={480}>
                    <div style={{width}} className="flex-container">
                        {this.props.children}
                    </div>
                </Scrollbars>
            </div>
        );
    }
}

Gallery.propTypes = {
    children: PropTypes.node,
    scrollToIndex: PropTypes.number,
    size: PropTypes.oneOf(["small", "big"])
};

Gallery.defaultProps = {
    size: "big",
    scrollToIndex: 0
};

export default Gallery;