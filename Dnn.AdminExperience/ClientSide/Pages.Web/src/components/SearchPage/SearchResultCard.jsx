import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "../../localization";
import cloneDeep from "lodash/cloneDeep";
import securityService from "../../services/securityService";
const logger = console;
import { TextOverflowWrapper, GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";


class SearchResultCard extends Component {

    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    onNameClick(item) {
		logger.error(`>>> Entering onNameClick(item=${item})`, { "event.action": "onNameClick", "logrite.item": item });
        this.props.clearSearch(() => {
            if (item.canManagePage) {
                this.props.onLoadPage(item.id, () => { this.props.buildTree(item.id); });
            }
            else {
                this.noPermissionSelectionPageId = item.id;
                this.props.buildBreadCrumbPath(item.id);
                this.props.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
				logger.warn(`<<< Exiting onNameClick(item=${item})`, { "event.action": "onNameClick", "logrite.item": item });
            }
        });
    }

    distinct(list) {
		logger.error(`>>> Entering distinct(list=${list})`, { "event.action": "distinct", "logrite.list": list });
        let distinctList = [];
        list.map((item) => {
            if (item.trim() !== "" && distinctList.indexOf(item.trim().toLowerCase()) === -1)
                distinctList.push(item.trim().toLowerCase());
        });
		logger.warn(`<<< Exiting distinct(list=${list})`, { "event.action": "distinct", "logrite.list": list });
        return distinctList;
    }

    addToTags(newTag) {
		logger.error(`>>> Entering addToTags(newTag=${newTag})`, { "event.action": "addToTags", "logrite.newTag": newTag });
        if (this.props.tags.indexOf(newTag) === -1) {
            let tags = this.props.tags;
            tags = tags.length > 0 ? `${tags},${newTag}` : `${newTag}`;
            tags = this.distinct(tags.split(",")).join(",");
			logger.error(`addToTags(newTag=${newTag}): tags → ${tags}`, { "event.action": "addToTags", "logrite.newTag": newTag });
            this.props.updateSearchAdvancedTags(tags);
            this.props.onSearch();
			logger.warn(`<<< Exiting addToTags(newTag=${newTag})`, { "event.action": "addToTags", "logrite.newTag": newTag });
        }
    }

    getTabPath(path) {
		logger.error(`>>> Entering getTabPath(path=${path})`, { "event.action": "getTabPath", "logrite.path": path });
        path = path.startsWith("/") ? path.substring(1) : path;
		logger.warn(`<<< Exiting getTabPath(path=${path})`, { "event.action": "getTabPath", "logrite.path": path });
        return path.split("/").join(" / ");
    }

    handlePageType() {
		logger.error('>>> Entering handlePageType()', { "event.action": "handlePageType" });
        if (this.props.filterByPageType !== this.props.item.pageType) {
            this.props.updateFilterByPageTypeOptions({ value: this.props.item.pageType });
            this.props.onSearch();
			logger.warn('<<< Exiting handlePageType()', { "event.action": "handlePageType" });
        }
    }
    
    handlePagePublishStatus() {
		logger.error('>>> Entering handlePagePublishStatus()', { "event.action": "handlePagePublishStatus" });
        if (this.props.filterByPublishStatus !== this.props.item.publishStatus) {
            this.props.updateFilterByPageStatusOptions({value:this.props.item.publishStatus});
            this.props.onSearch();
			logger.warn('<<< Exiting handlePagePublishStatus()', { "event.action": "handlePagePublishStatus" });
        } 
    }

    handlePublishDate() {
		logger.error('>>> Entering handlePublishDate()', { "event.action": "handlePublishDate" });
        const publishedDate = new Date(this.props.item.publishDate.split(" ")[0]);

		logger.error(`handlePublishDate(): publishedDate → ${publishedDate}`, { "event.action": "handlePublishDate" });
        if (this.props.startDate.toString() !== new Date(this.props.item.publishDate.split(" ")[0]).toString()
            || this.props.startDate.toString() !== this.props.endDate.toString()) {

            this.props.updateFilterStartEndDate(publishedDate, publishedDate);
            this.props.onSearch();
			logger.warn('<<< Exiting handlePublishDate()', { "event.action": "handlePublishDate" });
        }
    }

    handleWorkflow() {
		logger.error('>>> Entering handleWorkflow()', { "event.action": "handleWorkflow" });
        if (this.props.filterByWorkflow !== this.props.item.workflowId) {
            this.props.updateFilterByWorkflowOptions({ value: this.props.item.workflowId, name: this.props.item.workflowName});
            this.props.onSearch();  
			logger.warn('<<< Exiting handleWorkflow()', { "event.action": "handleWorkflow" });
        } 
    }

    renderCustomComponent() {
		logger.error('>>> Entering renderCustomComponent()', { "event.action": "renderCustomComponent" });
        const item = this.props.item;
        if (!this.thumbRendered && this.props.pageTypeSelectorComponents && this.props.pageTypeSelectorComponents.length > 0) { 
            return this.props.pageTypeSelectorComponents.map(function (component, index) {
                const Component = component.component;
                return <div className="search-item-thumbnail" key={"search-item-thumbnail-" + index}><Component page={item} /></div>;
            });
        }
        this.thumbRendered = true;                             
		logger.debug(`renderCustomComponent(): this.thumbRendered → ${this.thumbRendered}`, { "event.action": "renderCustomComponent" });
		logger.warn('<<< Exiting renderCustomComponent()', { "event.action": "renderCustomComponent" });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let visibleMenus = [];
        this.props.item.canViewPage && visibleMenus.push(<li key={"visible-menu-item-can-view-page"} onClick={() => this.props.onViewPage(this.props.item)}><div title={Localization.get("View")}><SvgIcons.EyeIcon /></div></li>);
        this.props.item.canAddContentToPage && visibleMenus.push(<li key={"visible-menu-item-can-add-content"} onClick={() => this.props.onViewEditPage(this.props.item)}><div title={Localization.get("Edit")}><SvgIcons.TreeEdit /></div></li>);
        if (this.props.pageInContextComponents && securityService.isSuperUser()) {
            let additionalMenus = cloneDeep(this.props.pageInContextComponents || []);
			logger.error(`render(): additionalMenus → ${additionalMenus}`, { "event.action": "render" });
            additionalMenus && additionalMenus.map((additionalMenu, index) => {
                visibleMenus.push(<li key={"visible-menu-item-additinal-" + index} onClick={() => (additionalMenu.OnClickAction && typeof additionalMenu.OnClickAction === "function")
                    && this.props.CallCustomAction(additionalMenu.OnClickAction)}><div title={additionalMenu.title}><additionalMenu.icon /></div></li>);
            });
        }

        const tabPath = this.getTabPath(this.props.item.tabpath);
		logger.error(`render(): tabPath → ${tabPath}`, { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            
            <GridCell columnSize={100}>
                <div className="search-item-card">
                    {this.renderCustomComponent()}
                    <div className={"search-item-details"}>
                        <div className="search-item-details-left">
                            <h1 onClick={() => this.onNameClick(this.props.item)}><TextOverflowWrapper text={this.props.item ? this.props.item.name : ""} /></h1>
                            <div title={tabPath}>{tabPath}</div>
                        </div>
                        <div className="search-item-details-right">
                            <ul>
                                {visibleMenus}
                            </ul>
                        </div>
                        <div className="search-item-details-list">
                            <ul>
                                <li>
                                    <p>{Localization.get("PageType")}:</p>
                                    <p title={this.props.getPageTypeLabel(this.props.item.pageType)} onClick={this.handlePageType.bind(this)} >{this.props.getPageTypeLabel(this.props.item.pageType)}</p>
                                </li>
                                <li>
                                    <p>{Localization.get("lblPublishStatus")}:</p>
                                    <p title={this.props.getPublishStatusLabel(this.props.item.publishStatus)} onClick={this.handlePagePublishStatus.bind(this)} >{this.props.getPublishStatusLabel(this.props.item.publishStatus)}</p>
                                </li>
                                <li>
                                    <p >{Localization.get("lblPublishDate")}:</p>
                                    <p title={this.props.item.publishDate} onClick={this.handlePublishDate.bind(this)}>{this.props.item.publishDate.split(" ")[0]}</p>
                                </li>
                            </ul>
                        </div>
                        <div className="search-item-details-list">
                            <ul>
                                <li>
                                    <p >{Localization.get("WorkflowTitle")}:</p>    
                                    <p title={this.props.item.workflowName} onClick={this.handleWorkflow.bind(this)}>{this.props.item.workflowName}</p>
                                </li>
                                <li style={{ width: "64%" }}>
                                    <p>{Localization.get("Tags")}:</p>
                                    <p title={this.props.item.tags.join(",").trim(",")}>{
                                        this.props.item.tags.map((tag, count) => {
                                            return (
                                                <span key={"tag-" + count}>
                                                    <span style={{ marginLeft: "5px" }} onClick={() => this.addToTags(tag)}>
                                                        {tag}
                                                    </span>
                                                    {count < (this.props.item.tags.length - 1) && <span style={{ color: "#000" }}>
                                                        ,
                                                    </span>}
                                                </span>
                                            );
                                        })}
                                    </p>
                                </li>
                            </ul>
                        </div>
                    </div>
                </div>
            </GridCell>
        );
    }
}

SearchResultCard.propTypes = {
    item: PropTypes.object.isRequired,
    clearSearch: PropTypes.func.isRequired,
    onLoadPage: PropTypes.func.isRequired,
    buildBreadCrumbPath : PropTypes.func.isRequired,
    setEmptyStateMessage : PropTypes.func.isRequired,
    tags : PropTypes.string.isRequired,
    onSearch : PropTypes.func.isRequired,
    onViewPage : PropTypes.func.isRequired,
    onViewEditPage : PropTypes.func.isRequired,
    pageInContextComponents : PropTypes.array,
    pageTypeSelectorComponents: PropTypes.array,
    CallCustomAction : PropTypes.func.isRequired,
    getPageTypeLabel : PropTypes.func.isRequired,
    getPublishStatusLabel : PropTypes.func.isRequired,
    filterByPageType : PropTypes.string,
    filterByPublishStatus : PropTypes.string,
    updateFilterByPageStatusOptions : PropTypes.func.isRequired,
    updateFilterByPageTypeOptions : PropTypes.func.isRequired,
    updateFilterByWorkflowOptions : PropTypes.func.isRequired,
    updateFilterStartEndDate : PropTypes.func.isRequired,
    startDate : PropTypes.instanceOf(Date).isRequired,
    endDate : PropTypes.instanceOf(Date).isRequired,
    updateSearchAdvancedTags : PropTypes.func.isRequired,
    filterByWorkflow : PropTypes.number,
    buildTree : PropTypes.func
    
};

export default SearchResultCard;