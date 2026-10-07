import React, {Component} from "react";
import PropTypes from "prop-types";
import Localization from "localization";
import Module from "./Module";
import { Checkbox, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./PageLanguage.less";
import utils from "../../utils";
const logger = console;

class PageLanguage extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            allModulesLinked: false,
            allModulesSelected: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    updateAllModules(key, value) {
		logger.error(`>>> Entering updateAllModules(key=${key},value=${value})`, { "event.action": "updateAllModules", "logrite.key": key, "logrite.value": value });
        const {modules} = this.props;
        const CultureCode = this.props.local.CultureCode;
        modules.forEach((module, index) => {
            if (module.IsShared && key === "IsLocalized") {
                return;
            }
            this.props.onUpdateModules(CultureCode, index, value, key);
        });
		logger.warn(`<<< Exiting updateAllModules(key=${key},value=${value})`, { "event.action": "updateAllModules", "logrite.key": key, "logrite.value": value });
    }

    selectAllModules(e) {
		logger.error(`>>> Entering selectAllModules(e=${e})`, { "event.action": "selectAllModules", "logrite.e": e });
        const allModulesSelected = e;
        this.setState({ allModulesSelected }, ()=> {
            this.updateAllModules("IsTranslated", e);
        });
		logger.warn(`<<< Exiting selectAllModules(e=${e})`, { "event.action": "selectAllModules", "logrite.e": e });
    }

    linkAllModules() {
		logger.error('>>> Entering linkAllModules()', { "event.action": "linkAllModules" });
        const allModulesLinked = !this.state.allModulesLinked;
        this.setState({ allModulesLinked }, () => {
            this.updateAllModules("IsLocalized", allModulesLinked);
        });
		logger.warn('<<< Exiting linkAllModules()', { "event.action": "linkAllModules" });
    } 

    onUpdatePages(key, e) {
		logger.error(`>>> Entering onUpdatePages(key=${key},e=${e})`, { "event.action": "onUpdatePages", "logrite.key": key, "logrite.e": e });
        const value = e.target ? e.target.value: e;
        const CultureCode = this.props.local.CultureCode;
        this.props.onUpdatePages(CultureCode, key, value);
		logger.warn(`<<< Exiting onUpdatePages(key=${key},e=${e})`, { "event.action": "onUpdatePages", "logrite.key": key, "logrite.e": e });
    }

    onViewPage(url, e) {
		logger.error(`>>> Entering onViewPage(url=${url},e=${e})`, { "event.action": "onViewPage", "logrite.url": url, "logrite.e": e });
        utils.getUtilities().closePersonaBar(function () {
            window.parent.location = url;
        });

        e.preventDefault();
        e.stopPropagation();
		logger.warn(`<<< Exiting onViewPage(url=${url},e=${e})`, { "event.action": "onViewPage", "logrite.url": url, "logrite.e": e });
    }

    onSettingPage(url, e) {
		logger.error(`>>> Entering onSettingPage(url=${url},e=${e})`, { "event.action": "onSettingPage", "logrite.url": url, "logrite.e": e });
        let panelId = window.$(".socialpanel:visible").attr("id");
        utils.getUtilities().panelViewData(panelId, {tab: [0]});
		logger.error(`onSettingPage(url=${url},e=${e}): panelId → ${panelId}`, { "event.action": "onSettingPage", "logrite.url": url, "logrite.e": e });
        window.parent.location = url;
        
        e.preventDefault();
        e.stopPropagation();
		logger.warn(`<<< Exiting onSettingPage(url=${url},e=${e})`, { "event.action": "onSettingPage", "logrite.url": url, "logrite.e": e });
    }

    onDeletePage(page, e) {
		logger.error(`>>> Entering onDeletePage(page=${page},e=${e})`, { "event.action": "onDeletePage", "logrite.page": page, "logrite.e": e });
        let confirmText = Localization.get("DeleteTranslationConfirm");
        confirmText = confirmText.replace("{0}", page.CultureCode);
        utils.confirm(
            confirmText,
            Localization.get("Delete"),
            Localization.get("Cancel"),
            () => {
                this.props.onDeletePage(page);
            }
        );
        e.preventDefault();
        e.stopPropagation();
		logger.warn(`<<< Exiting onDeletePage(page=${page},e=${e})`, { "event.action": "onDeletePage", "logrite.page": page, "logrite.e": e });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const iconSrc = this.props.local && this.props.local.Icon ? this.props.local.Icon : "";
        const cultureCode = this.props.local && this.props.local.CultureCode ? this.props.local.CultureCode : "";
		logger.error(`render(): iconSrc → ${iconSrc}`, { "event.action": "render" });
		logger.error(`render(): cultureCode → ${cultureCode}`, { "event.action": "render" });
        const page = this.props.page || {};
        const modules = this.props.modules || [];
        const isCurrentPage = page.TabId === utils.getCurrentPageId();
		logger.error(`render(): isCurrentPage → ${isCurrentPage}`, { "event.action": "render" });
        const isSpecialPage = page && page.IsSpecial ? true : false;
        const moduleComponents = modules.map((module, index) => {
            return <Module
                key={module.id}
                isDefault={this.props.isDefault}
                module={module}
                onUpdateModules={this.props.onUpdateModules}
                onDeleteModule={this.props.onDeleteModule}
                onRestoreModule={this.props.onRestoreModule}
                index={index}
                cultureCode={cultureCode}
            />;
        });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className="page-language">
                <div className="page-language-row">
                    <img src={iconSrc} alt={cultureCode} />
                    <span>{cultureCode}</span>
                    {!this.props.isDefault && !isSpecialPage && <a className="icon"
                        onClick={this.onDeletePage.bind(this, page)} 
                        aria-label="Delete"><SvgIcons.TrashIcon />
                    </a>}
                    {!isCurrentPage && <a className="icon" 
                        onClick={this.onSettingPage.bind(this, page.PageUrl)} 
                        aria-label="Settings"><SvgIcons.SettingsIcon />
                    </a>}
                    <a className="icon" 
                        href={page.PageUrl} 
                        onClick={this.onViewPage.bind(this, page.PageUrl)} 
                        aria-label="View"><SvgIcons.EyeIcon />
                    </a>
                </div>
                <div className="page-language-row">
                    <input type="text" value={page.TabName} onChange={this.onUpdatePages.bind(this, "TabName") } aria-label="Name" />
                    <input type="text" value={page.Title} onChange={this.onUpdatePages.bind(this, "Title") } aria-label="Title" />
                    <textarea value={page.Description} onChange={this.onUpdatePages.bind(this, "Description") } aria-label="Description"/>
                </div>
                <div className="page-language-row">
                    <div className="page-language-row-header">
                        <span className="icon"><SvgIcons.ModuleIcon /></span>
                        <span>{Localization.get("ModulesOnThisPage") }</span>
                        {!this.props.isDefault && <div className="icons-container">
                            <span
                                title={this.state.allModulesLinked ? Localization.get("ClickToDeLocalizeAllModules_tooltip") : Localization.get("ClickToLocalizeAllModules_tooltip")}
                                className={`icon float-left ${(this.state.allModulesLinked ? " blue" : "")}`}
                                onClick={this.linkAllModules.bind(this)  }><SvgIcons.LinkIcon /></span>
                            <div title={this.state.allModulesSelected ? Localization.get("UnCheckAllTranslatedModules_tooltip") : Localization.get("CheckToTranslateAllModules_tooltip")} style={{ float: "left", display: "inline-block"}}>
                                <Checkbox
                                    value={this.state.allModulesSelected}
                                    onChange={this.selectAllModules.bind(this) } /></div>
                        </div>}
                    </div>
                    {moduleComponents }
                    <div className="module-row footer">
                        <div title={page.IsTranslated ? Localization.get("UnCheckTransaledPage_tooltip") : Localization.get("CheckToTransalePage_tooltip")} style={{float: "right", display: "inline-block" }}>
                            <Checkbox
                                value={page.IsTranslated}
                                onChange={this.onUpdatePages.bind(this, "IsTranslated") } /></div>
                        <div>{Localization.get("TranslatedCheckbox") }</div>
                    </div>
                    <div className="module-row footer">
                        <div title={page.IsPublished ? Localization.get("UnCheckPublishedPage_tooltip") : Localization.get("CheckToPublishPage_tooltip")} style={{ float: "right", display: "inline-block" }}>
                            <Checkbox
                                value={page.IsPublished}
                                onChange={this.onUpdatePages.bind(this, "IsPublished") } /></div>
                        <div>{Localization.get("PublishedCheckbox") }</div>
                    </div>
                </div>
            </div>
        );
    }
}

PageLanguage.propTypes = {
    local: PropTypes.object.isRequired,
    page: PropTypes.object.isRequired,
    modules: PropTypes.object.isRequired,
    onUpdatePages: PropTypes.func,
    onUpdateModules: PropTypes.func,
    onDeleteModule: PropTypes.func,
    onRestoreModule: PropTypes.func,
    onDeletePage: PropTypes.func,
    isDefault: PropTypes.bool
};

export default PageLanguage;
