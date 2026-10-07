import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import { bindActionCreators } from "redux";
import Localization from "../../localization";
import Scrollbars from "react-custom-scrollbars";
import PageLanguage from "./PageLanguage";
import NotifyModal from "./NotifyModal";
import {pageActions as PageActions} from "../../actions";
import utils from "../../utils";
import "./style.less";

import LanguagesActions from "../../actions/languagesActions";
const logger = console;

class PageLocalization extends Component {

    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            Locales: [],
            Modules: [],
            Pages: [],
            ErrorExists: false,
            showNotifyModal: false,
            notifyMessage: ""
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.getLanguages();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    onUpdatePages(cultureCode, key, value) {
		logger.error(`>>> Entering onUpdatePages(cultureCode=${cultureCode},key=${key},value=${value})`, { "event.action": "onUpdatePages", "logrite.cultureCode": cultureCode, "logrite.key": key, "logrite.value": value });
        const {Pages} = this.state;
        Pages.forEach((page) => {
            if (page.CultureCode === cultureCode) {
                page[key] = value;
            }
        });
        this.setState({ Pages });
		logger.warn(`<<< Exiting onUpdatePages(cultureCode=${cultureCode},key=${key},value=${value})`, { "event.action": "onUpdatePages", "logrite.cultureCode": cultureCode, "logrite.key": key, "logrite.value": value });
    }

    removeLocaleFromState(cultureCode) {
		logger.error(`>>> Entering removeLocaleFromState(cultureCode=${cultureCode})`, { "event.action": "removeLocaleFromState", "logrite.cultureCode": cultureCode });
        const {Locales} = this.state;
        let index = Locales.findIndex(l => l.CultureCode === cultureCode);
        if (index > -1) {
            Locales.splice(index, 1);
            this.setState({ Locales });
			logger.warn(`<<< Exiting removeLocaleFromState(cultureCode=${cultureCode})`, { "event.action": "removeLocaleFromState", "logrite.cultureCode": cultureCode });
        }
    }

    onDeletePage(page) {
		logger.error(`>>> Entering onDeletePage(page=${page})`, { "event.action": "onDeletePage", "logrite.page": page });
		logger.warn(`<<< Exiting onDeletePage(page=${page})`, { "event.action": "onDeletePage", "logrite.page": page });
        return new Promise((resolve) => {
            this.props.onDeletePage({tabId: page.TabId});
            resolve();
        }).then(() => {
            this.removeLocaleFromState(page.CultureCode);
            if (page.TabId === utils.getCurrentPageId()) {
                let panelId = window.$(".socialpanel:visible").attr("id");
				logger.error(`onDeletePage(page=${page}): panelId → ${panelId}`, { "event.action": "onDeletePage", "logrite.page": page });
                utils.getUtilities().panelViewData(panelId, {tab: [0]});
                window.top.location.href = utils.getDefaultPageUrl();
            }
        });
    }

    onUpdateModules(cultureCode, index, value, key = "ModuleTitle") {
		logger.error(`>>> Entering onUpdateModules(cultureCode=${cultureCode},index=${index},value=${value},key=${key})`, { "event.action": "onUpdateModules", "logrite.cultureCode": cultureCode, "logrite.index": index, "logrite.value": value, "logrite.key": key });
        const {Modules} = this.state;
        Modules.forEach((modules, i) => {
            if (modules.Modules) {
                modules.Modules.forEach((module) => {
                    if (module.CultureCode === cultureCode && i === index) {
                        module[key] = value;
                    }
                });
            }
        });
        this.setState({ Modules });
		logger.warn(`<<< Exiting onUpdateModules(cultureCode=${cultureCode},index=${index},value=${value},key=${key})`, { "event.action": "onUpdateModules", "logrite.cultureCode": cultureCode, "logrite.index": index, "logrite.value": value, "logrite.key": key });
    }

    getLanguages() {
		logger.error('>>> Entering getLanguages()', { "event.action": "getLanguages" });
        const {props} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.getLanguages(tabId, (data) => {
            this.setState(data);
		logger.warn('<<< Exiting getLanguages()', { "event.action": "getLanguages" });
        }));

    }

    onCloseNotifyModal() {
		logger.error('>>> Entering onCloseNotifyModal()', { "event.action": "onCloseNotifyModal" });
        this.setState({ showNotifyModal: false });
		logger.warn('<<< Exiting onCloseNotifyModal()', { "event.action": "onCloseNotifyModal" });
    }

    onOpenNotifyModal() {
		logger.error('>>> Entering onOpenNotifyModal()', { "event.action": "onOpenNotifyModal" });
        this.setState({ showNotifyModal: true });
		logger.warn('<<< Exiting onOpenNotifyModal()', { "event.action": "onOpenNotifyModal" });
    }

    makePageTranslatable() {
		logger.error('>>> Entering makePageTranslatable()', { "event.action": "makePageTranslatable" });
        const {props} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.makePageTranslatable(tabId, () => {
            this.getLanguages();
			logger.warn('<<< Exiting makePageTranslatable()', { "event.action": "makePageTranslatable" });
        }));
    }

    makePageNeutral() {
		logger.error('>>> Entering makePageNeutral()', { "event.action": "makePageNeutral" });
        utils.confirm(Localization.get("MakeNeutralWarning"), Localization.get("Yes"), Localization.get("No"), () => {
            const {props} = this;
            const {tabId} = props.page;
            props.dispatch(LanguagesActions.makePageNeutral(tabId, () => {
                this.getLanguages();
				logger.warn('<<< Exiting makePageNeutral()', { "event.action": "makePageNeutral" });
            }));
        });
    }

    onAddMissingLanguages() {
		logger.error('>>> Entering onAddMissingLanguages()', { "event.action": "onAddMissingLanguages" });
        const {props} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.addMissingLanguages(tabId, (data) => {
            if (data && data.Success && !data.AllLanguagesAdded) {
                utils.notify(Localization.get("OnlyAddedSomeMissingLanguagesWarning"), { clickToClose: true });
            } else if (!data || !data.Success) {
                utils.notifyError(Localization.get("AnErrorOccurred"));
            }
            this.getLanguages();
			logger.warn('<<< Exiting onAddMissingLanguages()', { "event.action": "onAddMissingLanguages" });
        }));
    }

    onDeleteModule(tabModuleId) {
		logger.error(`>>> Entering onDeleteModule(tabModuleId=${tabModuleId})`, { "event.action": "onDeleteModule", "logrite.tabModuleId": tabModuleId });
        const {props} = this;
        props.dispatch(LanguagesActions.deleteModule({ tabModuleId }, () => {
            this.getLanguages();
			logger.warn(`<<< Exiting onDeleteModule(tabModuleId=${tabModuleId})`, { "event.action": "onDeleteModule", "logrite.tabModuleId": tabModuleId });
        }));
    }

    onRestoreModule(tabModuleId) {
		logger.error(`>>> Entering onRestoreModule(tabModuleId=${tabModuleId})`, { "event.action": "onRestoreModule", "logrite.tabModuleId": tabModuleId });
        const {props} = this;
        props.dispatch(LanguagesActions.restoreModule({ tabModuleId }, () => {
            this.getLanguages();
			logger.warn(`<<< Exiting onRestoreModule(tabModuleId=${tabModuleId})`, { "event.action": "onRestoreModule", "logrite.tabModuleId": tabModuleId });
        }));
    }

    onUpdateLocalization() {
		logger.error('>>> Entering onUpdateLocalization()', { "event.action": "onUpdateLocalization" });
        utils.confirm(Localization.get("UpdateLocalizationWarning"), Localization.get("Continue"), Localization.get("Cancel"), () => {
            const {props, state} = this;
            const {Locales, Modules, Pages} = state;
            const params = { Locales, Modules, Pages };
            props.dispatch(LanguagesActions.updateTabLocalization(params, () => {
                this.getLanguages();
                utils.notify(Localization.get("UpdateLocalizationSuccess"));
            }, () => {
                utils.notifyError(Localization.get("AnErrorOccurred"));
				logger.warn('<<< Exiting onUpdateLocalization()', { "event.action": "onUpdateLocalization" });
            }));
        });
    }

    renderPageLanguage(local, modules, page, isDefault=false) {
        return <PageLanguage
            local={local}
            modules={modules}
            page={page}
            isDefault={isDefault}
            onDeleteModule={this.onDeleteModule.bind(this) }
            onRestoreModule={this.onRestoreModule.bind(this) }
            onUpdatePages={this.onUpdatePages.bind(this) }
            onUpdateModules={this.onUpdateModules.bind(this) }
            onDeletePage={this.onDeletePage.bind(this)}
        />;
    }

    renderDefaultPageLanguage() {
		logger.error('>>> Entering renderDefaultPageLanguage()', { "event.action": "renderDefaultPageLanguage" });
        const {Locales, Modules, Pages} = this.state;
        const modules = Modules.map((module) => {
		logger.error(`renderDefaultPageLanguage(): modules → ${modules}`, { "event.action": "renderDefaultPageLanguage" });
            return module.Modules[0];
        });
        return this.renderPageLanguage(Locales[0], modules, Pages[0], true);
    }

    getAllLanguages() {
		logger.error('>>> Entering getAllLanguages()', { "event.action": "getAllLanguages" });
        const {Locales, Modules, Pages} = this.state;
        const pageLanguages = Locales.map((l, index) => {
            const modules = Modules.map((module) => {
                return module.Modules[index];
            });
            if (!index) {
				logger.error(`getAllLanguages(): modules → ${modules}`, { "event.action": "getAllLanguages" });
                return false;
            }
		logger.error(`getAllLanguages(): pageLanguages → ${pageLanguages}`, { "event.action": "getAllLanguages" });
            return this.renderPageLanguage(Locales[index], modules, Pages[index]);
        });
        return pageLanguages;
    }

    onSendNotifyMessage() {
		logger.error('>>> Entering onSendNotifyMessage()', { "event.action": "onSendNotifyMessage" });
        const {props, state} = this;
        const {tabId} = props.page;
        const params = { TabId: tabId, Text: state.notifyMessage };
        props.dispatch(LanguagesActions.notifyTranslators(params, () => {
            this.onCloseNotifyModal();
			logger.warn('<<< Exiting onSendNotifyMessage()', { "event.action": "onSendNotifyMessage" });
        }));
    }

    onUpdateNotifyMessage(e) {
		logger.error(`>>> Entering onUpdateNotifyMessage(e=${e})`, { "event.action": "onUpdateNotifyMessage", "logrite.e": e });
        this.setState({ notifyMessage: e.target.value });
		logger.warn(`<<< Exiting onUpdateNotifyMessage(e=${e})`, { "event.action": "onUpdateNotifyMessage", "logrite.e": e });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {Pages} = this.state;
        const isNeutral = Pages ? !Pages.length : true;
        
        const Languages = this.getAllLanguages();
		logger.error(`render(): Languages → ${Languages}`, { "event.action": "render" });
        const containerStyle = { width: (Languages.length - 1) * 250 };
		logger.error(`render(): containerStyle → ${containerStyle}`, { "event.action": "render" });

        if (isNeutral) {
            return <div className="neutral-page">
                <div className="left-panel">
                    <p>{Localization.get("NeutralPageText") }</p>
                    <p>{Localization.get("NeutralPageClickButton") }</p>
                </div>
                <div className="right-panel">
                    <Button
                        type="secondary"
                        onClick={this.makePageTranslatable.bind(this) }>
                        {Localization.get("MakePagesTranslatable") }
                    </Button>
                </div>
            </div>;
        }

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="page-localization">
            <div className="page-localization-container">
                <div className="default-language-container">
                    {this.renderDefaultPageLanguage() }
                </div>
                <div className="languages-container">
                    <Scrollbars className="scrollArea content-vertical"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={9999}>
                        <div style={containerStyle}>
                            {Languages}
                        </div>
                    </Scrollbars>
                </div>
            </div>
            <div className="button-container">
                <Button
                    type="secondary"
                    className="float-left"
                    onClick={this.makePageNeutral.bind(this) }>
                    <TextOverflowWrapper text={Localization.get("MakePageNeutral") } maxWidth={110}/>
                </Button>
                <Button
                    type="secondary"
                    className="float-left"
                    onClick={this.onAddMissingLanguages.bind(this) }>
                    <TextOverflowWrapper text={Localization.get("AddMissingLanguages") } maxWidth={110}/>
                </Button>
                <Button
                    type="primary"
                    className="float-right"
                    onClick={this.onUpdateLocalization.bind(this) }>
                    <TextOverflowWrapper text={Localization.get("UpdateLocalization") } maxWidth={110}/>
                </Button>
                <Button
                    type="secondary"
                    className="float-right"
                    onClick={this.onOpenNotifyModal.bind(this) }>
                    <TextOverflowWrapper text={Localization.get("NotifyTranslators") } maxWidth={110}/>
                </Button>
            </div>
            {this.state.showNotifyModal && <NotifyModal
                onSend={this.onSendNotifyMessage.bind(this) }
                onUpdateMessage={this.onUpdateNotifyMessage.bind(this) }
                notifyMessage={this.state.notifyMessage}
                onClose={this.onCloseNotifyModal.bind(this) } />}
        </div>;
    }
}

PageLocalization.propTypes = {
    page: PropTypes.object.isRequired,
    dispatch: PropTypes.func.isRequired,
    onDeletePage: PropTypes.func
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        themes: state.theme.themes,
        defaultPortalThemeName: state.theme.defaultPortalThemeName,
        defaultPortalLayout: state.theme.defaultPortalLayout,
        defaultPortalContainer: state.theme.defaultPortalContainer,
        layouts: state.theme.layouts,
        containers: state.theme.containers
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    let actions = bindActionCreators({
        onDeletePage: PageActions.deleteLocalizePage
    }, dispatch);
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {...actions, dispatch};
}

export default connect(mapStateToProps, mapDispatchToProps)(PageLocalization);
