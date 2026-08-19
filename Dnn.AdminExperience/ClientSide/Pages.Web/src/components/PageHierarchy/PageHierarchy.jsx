import React, {Component} from "react";
import PropTypes from "prop-types";
import {pageHierarchyManager} from "./pages.pageHierarchy";
import utils from "../../utils";
import "./css/pages-hierarchy.css";
import Html from "../Html";
import pagesHtml from "./pages.html?raw";
const logger = console;

class PageHierarchy extends Component {
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        const {itemTemplate, dragItemTemplate, searchKeyword} = this.props;
        pageHierarchyManager.utility = utils.getUtilities();
        pageHierarchyManager.resx = pageHierarchyManager.utility.resx.Pages;
		logger.debug(`componentDidMount(): pageHierarchyManager.resx → ${pageHierarchyManager.resx}`);
        pageHierarchyManager._viewModel = {};
		logger.debug(`componentDidMount(): pageHierarchyManager._viewModel → ${pageHierarchyManager._viewModel}`);
        pageHierarchyManager.callPageSettings = this.callPageSettings.bind(this);
        pageHierarchyManager.init(this.node, this.initCallback.bind(this));
        pageHierarchyManager.setItemTemplate(itemTemplate);
        pageHierarchyManager.setDragItemTemplate(dragItemTemplate);
        pageHierarchyManager.setSearchKeyword(searchKeyword);   
        pageHierarchyManager.setCurrentTabIdAndSiteRoot(utils.getCurrentPageId(), utils.getSiteRoot());
        pageHierarchyManager.selectedPagePathChanged = p => this.props.onSelectedPagePathChanged([...p]);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const {itemTemplate, dragItemTemplate, searchKeyword, selectedPage} = prevProps;
        if (itemTemplate && itemTemplate !== this.props.itemTemplate) {
            pageHierarchyManager.setItemTemplate(this.props.itemTemplate);
        }    

        if (dragItemTemplate && dragItemTemplate !== this.props.dragItemTemplate) {
            pageHierarchyManager.setDragItemTemplate(this.props.dragItemTemplate);
        }    

        if (searchKeyword && searchKeyword !== this.props.searchKeyword) {
            pageHierarchyManager.setSearchKeyword(this.props.searchKeyword);
        }

        if (selectedPage && selectedPage !== this.props.selectedPage) {
            pageHierarchyManager.selectPageFromBreadCrumbs(this.props.selectedPage);
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }
    }
    
    initCallback() {
		logger.debug('>>> Entering initCallback()');
        const {createdPage} = this.props;

        if (createdPage !== null) {
            pageHierarchyManager.addPage(createdPage);
			logger.debug('<<< Exiting initCallback()');
        }
    }

    callPageSettings(action, params) {
		logger.debug(`>>> Entering callPageSettings(action=${action},params=${params})`);
        const pageId = params[0];
        this.props.onPageSettings(pageId);
		logger.debug(`<<< Exiting callPageSettings(action=${action},params=${params})`);
    }
    
    render() {
        return <div ref={node => this.node = node}><Html html={pagesHtml} /></div>;  
    }
} 

PageHierarchy.propTypes = {
    itemTemplate: PropTypes.string.isRequired,
    dragItemTemplate: PropTypes.string.isRequired,
    searchKeyword: PropTypes.string.isRequired,
    onPageSettings: PropTypes.func.isRequired,
    onSelectedPagePathChanged: PropTypes.func.isRequired,
    selectedPage: PropTypes.object,
    createdPage: PropTypes.object
};

PageHierarchy.defaultProps = {
    itemTemplate: "pages-list-item-template",
    dragItemTemplate: "pages-drag-item-template"
};

export default PageHierarchy; 
