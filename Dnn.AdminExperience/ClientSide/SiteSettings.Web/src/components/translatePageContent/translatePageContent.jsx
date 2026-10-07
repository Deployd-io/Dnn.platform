import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import resx from "../../resources";
import { Scrollbars } from "react-custom-scrollbars";
import { InputGroup, Switch, Label, PersonaBarPageBody, Button, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";
import TranslationProgressBars from "../languageSettings/TranslationProgressBars";
import {
    languages as LanguagesActions,
    siteInfo as SiteInfoActions,
    languageEditor as LanguageEditorActions
} from "actions";
import utils from "utils";
const logger = console;


class TranslatePageContent extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            pageList: [],
            basicSettings: null,
            languageBeingEdited: Object.assign({}, props.languageBeingEdited)
        };
        this.init(); 
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    init() {        
        this.getProgressData = this.getProgressData.bind(this);
        this.getPageList();
        this.getBasicSettings();
        this.getProgressData();
		logger.warn('<<< Exiting init()', { "event.action": "init" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;
        if (prevProps.languageBeingEdited !== props.languageBeingEdited) {
            this.setState({languageBeingEdited: Object.assign({}, props.languageBeingEdited)});
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    getPageList() {
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = props.portalId;
        props.dispatch(LanguagesActions.getPageList({ cultureCode, portalId }, (data) => {
            this.setState({ pageList: data });
        }));
		logger.warn('<<< Exiting getPageList()', { "event.action": "getPageList" });
    }

    getBasicSettings() {
        const {props} = this;
        props.dispatch(SiteInfoActions.getPortalSettings(props.portalId, props.cultureCode, (data) => {
            this.setState({
                basicSettings: Object.assign(data.Settings)
            });
        }));
		logger.warn('<<< Exiting getBasicSettings()', { "event.action": "getBasicSettings" });
    }

    getProgressData() {
        const {props} = this;
        props.dispatch(LanguagesActions.getLocalizationProgress((data) => {
            this.setState(data);
            this.getPageList();
            if (data.InProgress && !data.Error) {
                return setTimeout(this.getProgressData, 1000);
            }
            if (data.Error) {
                return;
            }
            this.doneProgress();
        }));
		logger.warn('<<< Exiting getProgressData()', { "event.action": "getProgressData" });
    }

    doneProgress() {
        const {state, props} = this;
        this.getPageList();
        props.dispatch(LanguagesActions.getLanguages(props.portalId, (data) => {
            const languages = data.Languages;
            if (!languages.length) {
                return;
            }
            const language = languages.find(l => l.Code === state.languageBeingEdited.Code);
            this.props.dispatch(LanguageEditorActions.setLanguageBeingEdited(language));
			logger.error(`doneProgress(): language → ${language}`, { "event.action": "doneProgress" });
        }));
    }

    renderPageList() {
        const {pageList} = this.state;
        if (!pageList) {
            return;
        }
        return pageList.map((page, i) => {
            return <div className="page-list-item" key={i}>
                <span>{page.PageName}</span>
                <a className="float-right" onClick={this.goToPageSettings.bind(this, page.PageId) }>{resx.get("EditPageSettings") }</a>
                <a className="float-right" target="_blank" rel="noopener noreferrer" href={page.ViewUrl}>{resx.get("ViewPage") }</a>
            </div>;
        });
		logger.warn('<<< Exiting renderPageList()', { "event.action": "renderPageList" });
    }

    onMarkAllPagesAsTranslated(cultureCode) {
		logger.error(`>>> Entering onMarkAllPagesAsTranslated(cultureCode=${cultureCode})`, { "event.action": "onMarkAllPagesAsTranslated", "logrite.cultureCode": cultureCode });
        const portalId = this.props.portalId;
        this.props.dispatch(LanguagesActions.markAllPagesAsTranslated({ cultureCode, portalId }, () => {
            utils.utilities.notify(resx.get("PagesSuccessfullyTranslated"));
            this.doneProgress();
			logger.warn(`<<< Exiting onMarkAllPagesAsTranslated(cultureCode=${cultureCode})`, { "event.action": "onMarkAllPagesAsTranslated", "logrite.cultureCode": cultureCode });
        }));
    }

    onEraseAllLocalizedPages() {
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = this.props.portalId;
        utils.utilities.confirm(resx.get("EraseTranslatedPagesWarning").replace("{0}", cultureCode), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(LanguagesActions.deleteLanguagePages({ portalId, cultureCode }, () => {
                utils.utilities.notify(resx.get("DeletedAllLocalizedPages"));
                this.doneProgress();
            }));
        });
		logger.warn('<<< Exiting onEraseAllLocalizedPages()', { "event.action": "onEraseAllLocalizedPages" });
    }

    onPublishTranslatedPages(enable = true) {
		logger.error(`>>> Entering onPublishTranslatedPages(enable=${enable})`, { "event.action": "onPublishTranslatedPages", "logrite.enable": enable });
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = props.portalId;
        props.dispatch(LanguagesActions.publishAllPages({ portalId, cultureCode, enable }, () => {
            const message = enable ? resx.get("PublishedAllTranslatedPages") : resx.get("UnPublishedAllTranslatedPages");
			logger.error(`onPublishTranslatedPages(enable=${enable}): message → ${message}`, { "event.action": "onPublishTranslatedPages", "logrite.enable": enable });
            utils.utilities.notify(message);
            this.doneProgress();
			logger.warn(`<<< Exiting onPublishTranslatedPages(enable=${enable})`, { "event.action": "onPublishTranslatedPages", "logrite.enable": enable });
        }));
    }

    addPages() {
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = props.portalId;
        props.dispatch(LanguagesActions.localizeContent({ cultureCode, portalId }, () => {
            this.getProgressData();
        }));
		logger.warn('<<< Exiting addPages()', { "event.action": "addPages" });
    }

    onCancel() {
        this.props.closePersonaBarPage();
		logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
    }

    goToPageSettings(pageId) {
		logger.error(`>>> Entering goToPageSettings(pageId=${pageId})`, { "event.action": "goToPageSettings", "logrite.pageId": pageId });
        let personaBar = window.parent.dnn ? window.parent.dnn.PersonaBar : null;
        if (personaBar) {
            personaBar.openPanel("Dnn.Pages",
                {
                    viewParams:
                    {
                        pageId,
                        viewTab: "localization",
                        referral: utils.identifier,
                        referralText: resx.get("BackToLanguages")
                    }
                }
            );

            let event = document.createEvent("Event");
			logger.error(`goToPageSettings(pageId=${pageId}): event → ${event}`, { "event.action": "goToPageSettings", "logrite.pageId": pageId });
			logger.debug(`goToPageSettings(pageId=${pageId}): event.pageId → ${event.pageId}`, { "event.action": "goToPageSettings", "logrite.pageId": pageId });
			logger.debug(`goToPageSettings(pageId=${pageId}): event.viewTab → ${event.viewTab}`, { "event.action": "goToPageSettings", "logrite.pageId": pageId });
			logger.debug(`goToPageSettings(pageId=${pageId}): event.referral → ${event.referral}`, { "event.action": "goToPageSettings", "logrite.pageId": pageId });
            event.initEvent("viewPageSettings", true, true);

            event.pageId = pageId;
            event.viewTab = "localization";
            event.referral = utils.identifier;
            event.referralText = resx.get("BackToLanguages");

            document.dispatchEvent(event);
			logger.warn(`<<< Exiting goToPageSettings(pageId=${pageId})`, { "event.action": "goToPageSettings", "logrite.pageId": pageId });
        }
    }

    toggleActivateLanguage(languageBeingEdited) {
		logger.error(`>>> Entering toggleActivateLanguage(languageBeingEdited=${languageBeingEdited})`, { "event.action": "toggleActivateLanguage", "logrite.languageBeingEdited": languageBeingEdited });
        this.props.dispatch(LanguagesActions.activateLanguage({
            portalId: this.props.portalId,
            cultureCode: languageBeingEdited.Code,
            enable: languageBeingEdited.Active
        }, () => {
            this.props.dispatch(LanguagesActions.getLanguages(this.props.portalId));
			logger.warn(`<<< Exiting toggleActivateLanguage(languageBeingEdited=${languageBeingEdited})`, { "event.action": "toggleActivateLanguage", "logrite.languageBeingEdited": languageBeingEdited });
        }));
    }

    onToggleActive(active) {
		logger.error(`>>> Entering onToggleActive(active=${active})`, { "event.action": "onToggleActive", "logrite.active": active });
        if (!active) {
            let {languageBeingEdited} = this.state;
            const languageName = this.props.languageDisplayMode.toLowerCase() === "native" ? languageBeingEdited.NativeName : languageBeingEdited.EnglishName;
			logger.error(`onToggleActive(active=${active}): languageName → ${languageName}`, { "event.action": "onToggleActive", "logrite.active": active });
            utils.utilities.confirm(resx.get("DeactivateLanguageWarning").replace("{0}", languageName), resx.get("Yes"), resx.get("No"), () => {
                languageBeingEdited.Active = !languageBeingEdited.Active;
                this.toggleActivateLanguage(languageBeingEdited);
                this.setState({ languageBeingEdited });
            });
        } else {
            let {languageBeingEdited} = this.state;
            languageBeingEdited.Active = !languageBeingEdited.Active;
			logger.debug(`onToggleActive(active=${active}): languageBeingEdited.Active → ${languageBeingEdited.Active}`, { "event.action": "onToggleActive", "logrite.active": active });
            this.toggleActivateLanguage(languageBeingEdited);
            this.setState({ languageBeingEdited });
			logger.warn(`<<< Exiting onToggleActive(active=${active})`, { "event.action": "onToggleActive", "logrite.active": active });
        }
    }

    render() {
        const {props, state} = this;
        const { languageBeingEdited } = props;
        const hasPublishedPages = !!languageBeingEdited.PublishedPages;
        
        const isEnabled = languageBeingEdited.Enabled;
        const pagesNumber = state.pageList ? state.pageList.length : 0;
        const localizablePages = +languageBeingEdited.LocalizablePages;
        const TranslatedPages = +languageBeingEdited.TranslatedPages;
        
        return <PersonaBarPageBody
            className="translate-page-content"
            backToLinkProps={{
                text: resx.get("BackToLanguages"),
                onClick: props.closePersonaBarPage
            }}>

            <div className="language-settings-page-list">
                {languageBeingEdited && <div className="top-block">
                    <div className="language-block">
                        <img className="float-left" src={languageBeingEdited.Icon} alt={languageBeingEdited.NativeName} />
                        <span >{languageBeingEdited.NativeName}</span>
                        {state.basicSettings && <span className="float-right">{state.basicSettings.PortalName}</span>}
                    </div>
                    <InputGroup>
                        <div className="activate-pages-switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("ActivatePages.Help") }
                                label={resx.get("ActivatePages") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.languageBeingEdited.Active}
                                readOnly={!isEnabled}
                                onChange={this.onToggleActive.bind(this) }
                            />
                        </div>
                    </InputGroup>
                    <div className="button-block">
                        <Button
                            type="secondary"
                            disabled={!isEnabled || !localizablePages || !pagesNumber}
                            onClick={this.onMarkAllPagesAsTranslated.bind(this, languageBeingEdited.Code) }>
                            <TextOverflowWrapper text={resx.get("MarkAllPagesAsTranslated") } maxWidth={150}/>
                        </Button>
                        <Button
                            disabled={!isEnabled || !localizablePages}
                            type="danger"
                            onClick={this.onEraseAllLocalizedPages.bind(this) }>
                            <TextOverflowWrapper text={resx.get("EraseAllLocalizedPages") } maxWidth={150}/>
                        </Button>
                        <Button
                            disabled={!languageBeingEdited.Active || !isEnabled || !TranslatedPages}
                            type="primary"
                            className="float-right"
                            onClick={this.onPublishTranslatedPages.bind(this, true) }>
                            <TextOverflowWrapper text={resx.get("PublishTranslatedPages") } maxWidth={150}/>
                        </Button>
                        <Button
                            type="secondary"
                            disabled={!languageBeingEdited.Active || !isEnabled || !hasPublishedPages || !TranslatedPages}
                            className="float-right"
                            onClick={this.onPublishTranslatedPages.bind(this, false) }>
                            <TextOverflowWrapper text={resx.get("UnpublishTranslatedPages") } maxWidth={150}/>
                        </Button>
                    </div>
                </div>}

                <div className="list-header">
                    <span>{resx.get("PagesToTranslate") } <span>{pagesNumber}</span></span>
                    <span className="float-right" onClick={this.addPages.bind(this) }>
                        <em>+</em>{resx.get("AddAllUnlocalizedPages") }
                    </span>
                </div>

                {state.InProgress && <TranslationProgressBars
                    InProgress={this.state.InProgress}
                    PrimaryPercent={this.state.PrimaryPercent}
                    PrimaryTotal={this.state.PrimaryTotal}
                    PrimaryValue={this.state.PrimaryValue}
                    SecondaryPercent={this.state.SecondaryPercent}
                    SecondaryTotal={this.state.SecondaryTotal}
                    SecondaryValue={this.state.SecondaryValue}
                    TimeEstimated={this.state.TimeEstimated}
                    Error={this.state.Error}
                    CurrentOperationText={this.state.CurrentOperationText}/>
                }
                {!!state.pageList && !!state.pageList.length && <div className="page-list">
                    <Scrollbars className="scrollArea content-vertical"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={500}>
                        {this.renderPageList() }
                    </Scrollbars>
                </div>}
            </div>
        </PersonaBarPageBody>;
    }
}

TranslatePageContent.propTypes = {
    dispatch: PropTypes.func.isRequired,
    pageList: PropTypes.array,
    cultureCode: PropTypes.string,
    onSelectChange: PropTypes.func,
    portalId: PropTypes.number,
    closePersonaBarPage: PropTypes.func,
    languageDisplayMode: PropTypes.string,
    languageBeingEdited: PropTypes.object
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        pageList: state.languages.pageList,
        languageBeingEdited: state.languageEditor.languageBeingEdited,
        languageDisplayMode: (state.languages.languageSettings && state.languages.languageSettings.LanguageDisplayMode) || "NATIVE",
    };
}

export default connect(mapStateToProps)(TranslatePageContent);
