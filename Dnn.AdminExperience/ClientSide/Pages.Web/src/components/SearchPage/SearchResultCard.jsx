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
		logger.debug(`>>> Entering constructor(props=${props})`);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    onNameClick(item) {
        this.props.clearSearch(() => {
            if (item.canManagePage) {
                this.props.onLoadPage(item.id, () => { this.props.buildTree(item.id); });
            }
            else {
                this.noPermissionSelectionPageId = item.id;
                this.props.buildBreadCrumbPath(item.id);
                this.props.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
            }
        });
		logger.debug(`<<< Exiting onNameClick(item=${item})`);
    }

    distinct(list) {
		logger.debug(`>>> Entering distinct(list=${list})`);
        let distinctList = [];
        list.map((item) => {
            if (item.trim() !== "" && distinctList.indexOf(item.trim().toLowerCase()) === -1)
                distinctList.push(item.trim().toLowerCase());
        });
		logger.debug(`<<< Exiting distinct(list=${list})`);
        return distinctList;
    }

    addToTags(newTag) {
		logger.debug(`>>> Entering addToTags(newTag=${newTag})`);
        if (this.props.tags.indexOf(newTag) === -1) {
            let tags = this.props.tags;
            tags = tags.length > 0 ? `${tags},${newTag}` : `${newTag}`;
            tags = this.distinct(tags.split(",")).join(",");
			logger.debug(`addToTags(newTag=${newTag}): tags → ${tags}`);
            this.props.updateSearchAdvancedTags(tags);
            this.props.onSearch();
			logger.debug(`<<< Exiting addToTags(newTag=${newTag})`);
        }
    }

    getTabPath(path) {
		logger.debug(`>>> Entering getTabPath(path=${path})`);
        path = path.startsWith("/") ? path.substring(1) : path;
		logger.debug(`getTabPath(path=${path}): path → ${path}`);
		logger.debug(`<<< Exiting getTabPath(path=${path})`);
        return path.split("/").join(" / ");
    }

    handlePageType() {
		logger.debug('>>> Entering handlePageType()');
        if (this.props.filterByPageType !== this.props.item.pageType) {
            this.props.updateFilterByPageTypeOptions({ value: this.props.item.pageType });
            this.props.onSearch();
			logger.debug('<<< Exiting handlePageType()');
        }
    }
    
    handlePagePublishStatus() {
		logger.debug('>>> Entering handlePagePublishStatus()');
        if (this.props.filterByPublishStatus !== this.props.item.publishStatus) {
            this.props.updateFilterByPageStatusOptions({value:this.props.item.publishStatus});
            this.props.onSearch();
			logger.debug('<<< Exiting handlePagePublishStatus()');
        } 
    }

    handlePublishDate() {
		logger.debug('>>> Entering handlePublishDate()');
        const publishedDate = new Date(this.props.item.publishDate.split(" ")[0]);

		logger.debug(`handlePublishDate(): publishedDate → ${publishedDate}`);
        if (this.props.startDate.toString() !== new Date(this.props.item.publishDate.split(" ")[0]).toString()
            || this.props.startDate.toString() !== this.props.endDate.toString()) {

            this.props.updateFilterStartEndDate(publishedDate, publishedDate);
            this.props.onSearch();
			logger.debug('<<< Exiting handlePublishDate()');
        }
    }

    handleWorkflow() {
		logger.debug('>>> Entering handleWorkflow()');
        if (this.props.filterByWorkflow !== this.props.item.workflowId) {
            this.props.updateFilterByWorkflowOptions({ value: this.props.item.workflowId, name: this.props.item.workflowName});
            this.props.onSearch();  
			logger.debug('<<< Exiting handleWorkflow()');
        } 
    }

    renderCustomComponent() {
		logger.debug('>>> Entering renderCustomComponent()');
        const item = this.props.item;
        if (!this.thumbRendered && this.props.pageTypeSelectorComponents && this.props.pageTypeSelectorComponents.length > 0) { 
            return this.props.pageTypeSelectorComponents.map(function (component, index) {
                const Component = component.component;
                return <div className="search-item-thumbnail" key={"search-item-thumbnail-" + index}><Component page={item} /></div>;
            });
        }
        this.thumbRendered = true;                             
		logger.debug(`renderCustomComponent(): this.thumbRendered → ${this.thumbRendered}`);
		logger.debug('<<< Exiting renderCustomComponent()');
    }

     
    render() {
		logger.debug('>>> Entering render()');
        let visibleMenus = [];
        this.props.item.canViewPage && visibleMenus.push(<li key={"visible-menu-item-can-view-page"} onClick={() => this.props.onViewPage(this.props.item)}><div title={Localization.get("View")}><SvgIcons.EyeIcon /></div></li>);
        this.props.item.canAddContentToPage && visibleMenus.push(<li key={"visible-menu-item-can-add-content"} onClick={() => this.props.onViewEditPage(this.props.item)}><div title={Localization.get("Edit")}><SvgIcons.TreeEdit /></div></li>);
        if (this.props.pageInContextComponents && securityService.isSuperUser()) {
            let additionalMenus = cloneDeep(this.props.pageInContextComponents || []);
			logger.debug(`render(): additionalMenus → ${additionalMenus}`);
            additionalMenus && additionalMenus.map((additionalMenu, index) => {
                visibleMenus.push(<li key={"visible-menu-item-additinal-" + index} onClick={() => (additionalMenu.OnClickAction && typeof additionalMenu.OnClickAction === "function")
                    && this.props.CallCustomAction(additionalMenu.OnClickAction)}><div title={additionalMenu.title}><additionalMenu.icon /></div></li>);
            });
        }

        const tabPath = this.getTabPath(this.props.item.tabpath);
		logger.debug(`render(): tabPath → ${tabPath}`);

		logger.debug('<<< Exiting render()');
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