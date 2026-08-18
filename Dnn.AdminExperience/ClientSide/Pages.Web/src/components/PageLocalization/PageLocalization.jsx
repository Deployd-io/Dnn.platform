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
		logger.debug('>>> Entering constructor()');
        this.state = {
            Locales: [],
            Modules: [],
            Pages: [],
            ErrorExists: false,
            showNotifyModal: false,
            notifyMessage: ""
        };
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.getLanguages();
		logger.debug('<<< Exiting componentDidMount()');
    }

    onUpdatePages(cultureCode, key, value) {
		logger.debug(`>>> Entering onUpdatePages(cultureCode=${cultureCode},key=${key},value=${value})`);
        const {Pages} = this.state;
        Pages.forEach((page) => {
            if (page.CultureCode === cultureCode) {
                page[key] = value;
            }
        });
        this.setState({ Pages });
		logger.debug(`<<< Exiting onUpdatePages(cultureCode=${cultureCode},key=${key},value=${value})`);
    }

    removeLocaleFromState(cultureCode) {
		logger.debug(`>>> Entering removeLocaleFromState(cultureCode=${cultureCode})`);
        const {Locales} = this.state;
        let index = Locales.findIndex(l => l.CultureCode === cultureCode);
        if (index > -1) {
            Locales.splice(index, 1);
            this.setState({ Locales });
			logger.debug(`<<< Exiting removeLocaleFromState(cultureCode=${cultureCode})`);
        }
    }

    onDeletePage(page) {
		logger.debug(`>>> Entering onDeletePage(page=${page})`);
		logger.debug(`<<< Exiting onDeletePage(page=${page})`);
        return new Promise((resolve) => {
            this.props.onDeletePage({tabId: page.TabId});
            resolve();
        }).then(() => {
            this.removeLocaleFromState(page.CultureCode);
            if (page.TabId === utils.getCurrentPageId()) {
                let panelId = window.$(".socialpanel:visible").attr("id");
				logger.debug(`onDeletePage(page=${page}): panelId → ${panelId}`);
                utils.getUtilities().panelViewData(panelId, {tab: [0]});
                window.top.location.href = utils.getDefaultPageUrl();
            }
        });
    }

    onUpdateModules(cultureCode, index, value, key = "ModuleTitle") {
		logger.debug(`>>> Entering onUpdateModules(cultureCode=${cultureCode},index=${index},value=${value},key=${key})`);
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
		logger.debug(`<<< Exiting onUpdateModules(cultureCode=${cultureCode},index=${index},value=${value},key=${key})`);
    }

    getLanguages() {
		logger.debug('>>> Entering getLanguages()');
        const {props} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.getLanguages(tabId, (data) => {
            this.setState(data);
		logger.debug('<<< Exiting getLanguages()');
        }));

    }

    onCloseNotifyModal() {
		logger.debug('>>> Entering onCloseNotifyModal()');
        this.setState({ showNotifyModal: false });
		logger.debug('<<< Exiting onCloseNotifyModal()');
    }

    onOpenNotifyModal() {
		logger.debug('>>> Entering onOpenNotifyModal()');
        this.setState({ showNotifyModal: true });
		logger.debug('<<< Exiting onOpenNotifyModal()');
    }

    makePageTranslatable() {
		logger.debug('>>> Entering makePageTranslatable()');
        const {props} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.makePageTranslatable(tabId, () => {
            this.getLanguages();
			logger.debug('<<< Exiting makePageTranslatable()');
        }));
    }

    makePageNeutral() {
		logger.debug('>>> Entering makePageNeutral()');
        utils.confirm(Localization.get("MakeNeutralWarning"), Localization.get("Yes"), Localization.get("No"), () => {
            const {props} = this;
            const {tabId} = props.page;
            props.dispatch(LanguagesActions.makePageNeutral(tabId, () => {
                this.getLanguages();
				logger.debug('<<< Exiting makePageNeutral()');
            }));
        });
    }

    onAddMissingLanguages() {
		logger.debug('>>> Entering onAddMissingLanguages()');
        const {props} = this;
        const {tabId} = props.page;
        props.dispatch(LanguagesActions.addMissingLanguages(tabId, (data) => {
            if (data && data.Success && !data.AllLanguagesAdded) {
                utils.notify(Localization.get("OnlyAddedSomeMissingLanguagesWarning"), { clickToClose: true });
            } else if (!data || !data.Success) {
                utils.notifyError(Localization.get("AnErrorOccurred"));
            }
            this.getLanguages();
			logger.debug('<<< Exiting onAddMissingLanguages()');
        }));
    }

    onDeleteModule(tabModuleId) {
		logger.debug(`>>> Entering onDeleteModule(tabModuleId=${tabModuleId})`);
        const {props} = this;
        props.dispatch(LanguagesActions.deleteModule({ tabModuleId }, () => {
            this.getLanguages();
			logger.debug(`<<< Exiting onDeleteModule(tabModuleId=${tabModuleId})`);
        }));
    }

    onRestoreModule(tabModuleId) {
		logger.debug(`>>> Entering onRestoreModule(tabModuleId=${tabModuleId})`);
        const {props} = this;
        props.dispatch(LanguagesActions.restoreModule({ tabModuleId }, () => {
            this.getLanguages();
			logger.debug(`<<< Exiting onRestoreModule(tabModuleId=${tabModuleId})`);
        }));
    }

    onUpdateLocalization() {
		logger.debug('>>> Entering onUpdateLocalization()');
        utils.confirm(Localization.get("UpdateLocalizationWarning"), Localization.get("Continue"), Localization.get("Cancel"), () => {
            const {props, state} = this;
            const {Locales, Modules, Pages} = state;
            const params = { Locales, Modules, Pages };
            props.dispatch(LanguagesActions.updateTabLocalization(params, () => {
                this.getLanguages();
                utils.notify(Localization.get("UpdateLocalizationSuccess"));
            }, () => {
                utils.notifyError(Localization.get("AnErrorOccurred"));
				logger.debug('<<< Exiting onUpdateLocalization()');
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
		logger.debug('>>> Entering renderDefaultPageLanguage()');
        const {Locales, Modules, Pages} = this.state;
        const modules = Modules.map((module) => {
		logger.debug(`renderDefaultPageLanguage(): modules → ${modules}`);
		logger.debug('<<< Exiting renderDefaultPageLanguage()');
            return module.Modules[0];
        });
        return this.renderPageLanguage(Locales[0], modules, Pages[0], true);
    }

    getAllLanguages() {
		logger.debug('>>> Entering getAllLanguages()');
        const {Locales, Modules, Pages} = this.state;
        const pageLanguages = Locales.map((l, index) => {
            const modules = Modules.map((module) => {
                return module.Modules[index];
            });
            if (!index) {
				logger.debug(`getAllLanguages(): modules → ${modules}`);
                return false;
            }
		logger.debug(`getAllLanguages(): pageLanguages → ${pageLanguages}`);
		logger.debug('<<< Exiting getAllLanguages()');
            return this.renderPageLanguage(Locales[index], modules, Pages[index]);
        });
        return pageLanguages;
    }

    onSendNotifyMessage() {
		logger.debug('>>> Entering onSendNotifyMessage()');
        const {props, state} = this;
        const {tabId} = props.page;
        const params = { TabId: tabId, Text: state.notifyMessage };
        props.dispatch(LanguagesActions.notifyTranslators(params, () => {
            this.onCloseNotifyModal();
			logger.debug('<<< Exiting onSendNotifyMessage()');
        }));
    }

    onUpdateNotifyMessage(e) {
		logger.debug(`>>> Entering onUpdateNotifyMessage(e=${e})`);
        this.setState({ notifyMessage: e.target.value });
		logger.debug(`<<< Exiting onUpdateNotifyMessage(e=${e})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {Pages} = this.state;
        const isNeutral = Pages ? !Pages.length : true;
        
        const Languages = this.getAllLanguages();
		logger.debug(`render(): Languages → ${Languages}`);
        const containerStyle = { width: (Languages.length - 1) * 250 };
		logger.debug(`render(): containerStyle → ${containerStyle}`);

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

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
    let actions = bindActionCreators({
        onDeletePage: PageActions.deleteLocalizePage
    }, dispatch);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
    return {...actions, dispatch};
}

export default connect(mapStateToProps, mapDispatchToProps)(PageLocalization);
