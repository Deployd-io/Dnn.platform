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
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const {itemTemplate, dragItemTemplate, searchKeyword} = this.props;
        pageHierarchyManager.utility = utils.getUtilities();
        pageHierarchyManager.resx = pageHierarchyManager.utility.resx.Pages;
        pageHierarchyManager._viewModel = {};
        pageHierarchyManager.callPageSettings = this.callPageSettings.bind(this);
        pageHierarchyManager.init(this.node, this.initCallback.bind(this));
        pageHierarchyManager.setItemTemplate(itemTemplate);
        pageHierarchyManager.setDragItemTemplate(dragItemTemplate);
        pageHierarchyManager.setSearchKeyword(searchKeyword);   
        pageHierarchyManager.setCurrentTabIdAndSiteRoot(utils.getCurrentPageId(), utils.getSiteRoot());
        pageHierarchyManager.selectedPagePathChanged = p => this.props.onSelectedPagePathChanged([...p]);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
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
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }
    
    initCallback() {
		logger.error('>>> Entering initCallback()', { "event.action": "initCallback" });
        const {createdPage} = this.props;

        if (createdPage !== null) {
            pageHierarchyManager.addPage(createdPage);
			logger.warn('<<< Exiting initCallback()', { "event.action": "initCallback" });
        }
    }

    callPageSettings(action, params) {
		logger.error(`>>> Entering callPageSettings(action=${action},params=${params})`, { "event.action": "callPageSettings", "logrite.action": action, "logrite.params": params });
        const pageId = params[0];
        this.props.onPageSettings(pageId);
		logger.warn(`<<< Exiting callPageSettings(action=${action},params=${params})`, { "event.action": "callPageSettings", "logrite.action": action, "logrite.params": params });
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
