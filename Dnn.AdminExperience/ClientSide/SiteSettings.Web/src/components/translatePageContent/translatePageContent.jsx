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
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            pageList: [],
            basicSettings: null,
            languageBeingEdited: Object.assign({}, props.languageBeingEdited)
        };
        this.init(); 
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    init() {        
		logger.debug('>>> Entering init()');
        this.getProgressData = this.getProgressData.bind(this);
        this.getPageList();
        this.getBasicSettings();
        this.getProgressData();
		logger.debug('<<< Exiting init()');
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        const { props } = this;
        if (prevProps.languageBeingEdited !== props.languageBeingEdited) {
            this.setState({languageBeingEdited: Object.assign({}, props.languageBeingEdited)});
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }
    }

    getPageList() {
		logger.debug('>>> Entering getPageList()');
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = props.portalId;
        props.dispatch(LanguagesActions.getPageList({ cultureCode, portalId }, (data) => {
            this.setState({ pageList: data });
			logger.debug('<<< Exiting getPageList()');
        }));
    }

    getBasicSettings() {
		logger.debug('>>> Entering getBasicSettings()');
        const {props} = this;
        props.dispatch(SiteInfoActions.getPortalSettings(props.portalId, props.cultureCode, (data) => {
			logger.debug('<<< Exiting getBasicSettings()');
            this.setState({
                basicSettings: Object.assign(data.Settings)
            });
        }));
    }

    getProgressData() {
		logger.debug('>>> Entering getProgressData()');
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
			logger.debug('<<< Exiting getProgressData()');
        }));
    }

    doneProgress() {
		logger.debug('>>> Entering doneProgress()');
        const {state, props} = this;
        this.getPageList();
        props.dispatch(LanguagesActions.getLanguages(props.portalId, (data) => {
            const languages = data.Languages;
            if (!languages.length) {
                return;
            }
            const language = languages.find(l => l.Code === state.languageBeingEdited.Code);
            this.props.dispatch(LanguageEditorActions.setLanguageBeingEdited(language));
			logger.debug(`doneProgress(): language → ${language}`);
        }));
		logger.debug('<<< Exiting doneProgress()');
    }

    renderPageList() {
		logger.debug('>>> Entering renderPageList()');
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
		logger.debug('<<< Exiting renderPageList()');
    }

    onMarkAllPagesAsTranslated(cultureCode) {
        const portalId = this.props.portalId;
        this.props.dispatch(LanguagesActions.markAllPagesAsTranslated({ cultureCode, portalId }, () => {
            utils.utilities.notify(resx.get("PagesSuccessfullyTranslated"));
            this.doneProgress();
        }));
		logger.debug(`<<< Exiting onMarkAllPagesAsTranslated(cultureCode=${cultureCode})`);
    }

    onEraseAllLocalizedPages() {
		logger.debug('>>> Entering onEraseAllLocalizedPages()');
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = this.props.portalId;
        utils.utilities.confirm(resx.get("EraseTranslatedPagesWarning").replace("{0}", cultureCode), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(LanguagesActions.deleteLanguagePages({ portalId, cultureCode }, () => {
                utils.utilities.notify(resx.get("DeletedAllLocalizedPages"));
                this.doneProgress();
				logger.debug('<<< Exiting onEraseAllLocalizedPages()');
            }));
        });
    }

    onPublishTranslatedPages(enable = true) {
		logger.debug(`>>> Entering onPublishTranslatedPages(enable=${enable})`);
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = props.portalId;
        props.dispatch(LanguagesActions.publishAllPages({ portalId, cultureCode, enable }, () => {
            const message = enable ? resx.get("PublishedAllTranslatedPages") : resx.get("UnPublishedAllTranslatedPages");
			logger.debug(`onPublishTranslatedPages(enable=${enable}): message → ${message}`);
            utils.utilities.notify(message);
            this.doneProgress();
			logger.debug(`<<< Exiting onPublishTranslatedPages(enable=${enable})`);
        }));
    }

    addPages() {
		logger.debug('>>> Entering addPages()');
        const {props} = this;
        const cultureCode = props.languageBeingEdited.Code;
        const portalId = props.portalId;
        props.dispatch(LanguagesActions.localizeContent({ cultureCode, portalId }, () => {
            this.getProgressData();
			logger.debug('<<< Exiting addPages()');
        }));
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        this.props.closePersonaBarPage();
		logger.debug('<<< Exiting onCancel()');
    }

    goToPageSettings(pageId) {
		logger.debug(`>>> Entering goToPageSettings(pageId=${pageId})`);
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
			logger.debug(`goToPageSettings(pageId=${pageId}): event → ${event}`);
            event.initEvent("viewPageSettings", true, true);

            event.pageId = pageId;
            event.viewTab = "localization";
            event.referral = utils.identifier;
            event.referralText = resx.get("BackToLanguages");

            document.dispatchEvent(event);
			logger.debug(`<<< Exiting goToPageSettings(pageId=${pageId})`);
        }
    }

    toggleActivateLanguage(languageBeingEdited) {
		logger.debug(`>>> Entering toggleActivateLanguage(languageBeingEdited=${languageBeingEdited})`);
        this.props.dispatch(LanguagesActions.activateLanguage({
            portalId: this.props.portalId,
            cultureCode: languageBeingEdited.Code,
            enable: languageBeingEdited.Active
        }, () => {
            this.props.dispatch(LanguagesActions.getLanguages(this.props.portalId));
			logger.debug(`<<< Exiting toggleActivateLanguage(languageBeingEdited=${languageBeingEdited})`);
        }));
    }

    onToggleActive(active) {
		logger.debug(`>>> Entering onToggleActive(active=${active})`);
        if (!active) {
            let {languageBeingEdited} = this.state;
            const languageName = this.props.languageDisplayMode.toLowerCase() === "native" ? languageBeingEdited.NativeName : languageBeingEdited.EnglishName;
			logger.debug(`onToggleActive(active=${active}): languageName → ${languageName}`);
            utils.utilities.confirm(resx.get("DeactivateLanguageWarning").replace("{0}", languageName), resx.get("Yes"), resx.get("No"), () => {
                languageBeingEdited.Active = !languageBeingEdited.Active;
                this.toggleActivateLanguage(languageBeingEdited);
                this.setState({ languageBeingEdited });
            });
        } else {
            let {languageBeingEdited} = this.state;
            languageBeingEdited.Active = !languageBeingEdited.Active;
            this.toggleActivateLanguage(languageBeingEdited);
            this.setState({ languageBeingEdited });
			logger.debug(`<<< Exiting onToggleActive(active=${active})`);
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
        const { languageBeingEdited } = props;
        const hasPublishedPages = !!languageBeingEdited.PublishedPages;
        
        const isEnabled = languageBeingEdited.Enabled;
        const pagesNumber = state.pageList ? state.pageList.length : 0;
        const localizablePages = +languageBeingEdited.LocalizablePages;
        const TranslatedPages = +languageBeingEdited.TranslatedPages;
        
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        pageList: state.languages.pageList,
        languageBeingEdited: state.languageEditor.languageBeingEdited,
        languageDisplayMode: (state.languages.languageSettings && state.languages.languageSettings.LanguageDisplayMode) || "NATIVE",
    };
}

export default connect(mapStateToProps)(TranslatePageContent);
