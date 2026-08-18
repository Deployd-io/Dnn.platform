import React, {Component} from "react";
import PropTypes from "prop-types";
import { Scrollbars } from "react-custom-scrollbars";
import style from "./style.module.less";
const logger = console;

class Gallery extends Component {

    getElementSize() {
		logger.debug('>>> Entering getElementSize()');
        const { size } = this.props;
		logger.debug('<<< Exiting getElementSize()');
        return size === "big" ? 198 : 130;        
    }

    calculateGalleryWidth() {
		logger.debug('>>> Entering calculateGalleryWidth()');
        const { children } = this.props;
		logger.debug('<<< Exiting calculateGalleryWidth()');
        return this.getElementSize() * children.length;
    }

    scrollToSelectedItem(scrollToIndex) {
		logger.debug(`>>> Entering scrollToSelectedItem(scrollToIndex=${scrollToIndex})`);
        const indexToScroll = scrollToIndex !== -1 ? scrollToIndex : 0; 
        const scrollbars = this.scrollbarsRef;
        if (scrollbars) {
            const scrollClientWidth = scrollbars.getClientWidth();
			logger.debug(`scrollToSelectedItem(scrollToIndex=${scrollToIndex}): scrollClientWidth → ${scrollClientWidth}`);
            const scrollLeft = indexToScroll * this.getElementSize();
			logger.debug(`scrollToSelectedItem(scrollToIndex=${scrollToIndex}): scrollLeft → ${scrollLeft}`);
            const currentScrollLeft = scrollbars.getScrollLeft();
			logger.debug(`scrollToSelectedItem(scrollToIndex=${scrollToIndex}): currentScrollLeft → ${currentScrollLeft}`);
            const lowBoundary = currentScrollLeft;
            const highBoundary = currentScrollLeft + scrollClientWidth;        
            if (scrollLeft > lowBoundary && scrollLeft < highBoundary) {
                return;
            }
            this.scrollbarsRef.scrollLeft(scrollLeft);
        }
		logger.debug(`<<< Exiting scrollToSelectedItem(scrollToIndex=${scrollToIndex})`);
    }

    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        setTimeout(() => this.scrollToSelectedItem(this.props.scrollToIndex), 0);
		logger.debug('<<< Exiting componentDidUpdate()');
    }
    
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        setTimeout(() => this.scrollToSelectedItem(this.props.scrollToIndex), 0);
		logger.debug('<<< Exiting componentDidMount()');
    }

    render() {  
		logger.debug('>>> Entering render()');
        const width = this.calculateGalleryWidth();    
		logger.debug('<<< Exiting render()');
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