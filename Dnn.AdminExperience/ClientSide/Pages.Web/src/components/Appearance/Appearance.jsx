import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { GridCell, SingleLineInputWithError, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import utils from "../../utils";
import ThemeSelector from "./ThemeSelector/ThemeSelector";
import LayoutSelector from "./LayoutSelector/LayoutSelector";
import ContainerSelector from "./ContainerSelector/ContainerSelector";
import ThemeActions from "../../actions/themeActions";
import style from "./style.module.less";
const logger = console;

class Appearance extends Component {

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const { page, onRetrieveThemes, onRetrieveThemeFiles } = this.props;

        onRetrieveThemes().then(data => {
            if (!data || data.success === false) {
                return;
            }
            const { defaultPortalThemeName, defaultPortalThemeLevel } = this.props;
            const selectedThemeName = page.themeName || defaultPortalThemeName;
            const selectedThemeLevel = page.themeLevel || defaultPortalThemeLevel;
            if (selectedThemeName) {
                onRetrieveThemeFiles(selectedThemeName, selectedThemeLevel);
				logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
            }
        });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { page, containers, defaultPortalThemeName, onRetrieveThemeFiles } = prevProps;

        if (this.props.containers && this.props.containers !== containers) {
            this.autoSelectFirstContainerIfNoOneIsSelected(this.props.page, this.props.containers);
        }

        if (page.themeName) {
            return;
        }

        if (this.props.defaultPortalThemeName && this.props.defaultPortalThemeName !== defaultPortalThemeName && this.props.defaultPortalThemeName && this.props.defaultPortalThemeName !== null) {
            onRetrieveThemeFiles(this.props.defaultPortalThemeName,this.props.defaultPortalThemeLevel);
        }
		logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
    }

    autoSelectFirstContainerIfNoOneIsSelected(page, containers) {
		logger.error(`>>> Entering autoSelectFirstContainerIfNoOneIsSelected(page=${page},containers=${containers})`, { "event.action": "autoSelectFirstContainerIfNoOneIsSelected", "logrite.page": page, "logrite.containers": containers });
        const { onChangeField, defaultPortalContainer } = this.props;
        const selectedContainerPath = page.containerSrc || defaultPortalContainer;
        const selectedContainer = containers.find(this.findByPath(selectedContainerPath));
        if (!selectedContainer && containers.length !== 0) {
            const container = containers[0];
            onChangeField("containerSrc", this.addAscxExtension(container.path));
			logger.warn(`<<< Exiting autoSelectFirstContainerIfNoOneIsSelected(page=${page},containers=${containers})`, { "event.action": "autoSelectFirstContainerIfNoOneIsSelected", "logrite.page": page, "logrite.containers": containers });
        }
    }

    getPagePreviewUrl(skinSrc, containerSrc) {
		logger.error(`>>> Entering getPagePreviewUrl(skinSrc=${skinSrc},containerSrc=${containerSrc})`, { "event.action": "getPagePreviewUrl", "logrite.skinSrc": skinSrc, "logrite.containerSrc": containerSrc });
        const { page } = this.props;
        // if is new page take current page loaded behind the persona bar for preview
        const pageUrl = page.tabId !== 0 ? page.absoluteUrl : window.parent.location.href;
        const skinSrcQueryString = this.trimAscxExtension(skinSrc);
		logger.error(`getPagePreviewUrl(skinSrc=${skinSrc},containerSrc=${containerSrc}): skinSrcQueryString → ${skinSrcQueryString}`, { "event.action": "getPagePreviewUrl", "logrite.skinSrc": skinSrc, "logrite.containerSrc": containerSrc });
        const containerSrcQueryString = this.trimAscxExtension(containerSrc);
		logger.warn(`<<< Exiting getPagePreviewUrl(skinSrc=${skinSrc},containerSrc=${containerSrc})`, { "event.action": "getPagePreviewUrl", "logrite.skinSrc": skinSrc, "logrite.containerSrc": containerSrc });
        return utils.url.appendQueryString(pageUrl, {
            SkinSrc: skinSrcQueryString,
            ContainerSrc: containerSrcQueryString
        });
    }

    previewPage() {
		logger.error('>>> Entering previewPage()', { "event.action": "previewPage" });
        const { page, defaultPortalLayout, defaultPortalContainer } = this.props;
        const skinSrc = page.skinSrc || defaultPortalLayout;
        const containerSrc = page.containerSrc || defaultPortalContainer;

        if (!skinSrc || !containerSrc) {
            utils.notify(Localization.get("PleaseSelectLayoutContainer"));
            return;
        }

        const previewUrl = this.getPagePreviewUrl(skinSrc, containerSrc);
        window.open(previewUrl);
		logger.error(`previewPage(): previewUrl → ${previewUrl}`, { "event.action": "previewPage" });
    }

    onSelectTheme(theme) {
		logger.error(`>>> Entering onSelectTheme(theme=${theme})`, { "event.action": "onSelectTheme", "logrite.theme": theme });
        this.props.onChangeField("themeName", theme.packageName);
        this.props.onChangeField("themeLevel", theme.level);
        const skinSrc = this.addAscxExtension(theme.defaultThemeFile);
		logger.error(`onSelectTheme(theme=${theme}): skinSrc → ${skinSrc}`, { "event.action": "onSelectTheme", "logrite.theme": theme });
        this.props.onChangeField("skinSrc", skinSrc);
        this.props.onRetrieveThemeFiles(theme.packageName, theme.level);
		logger.warn(`<<< Exiting onSelectTheme(theme=${theme})`, { "event.action": "onSelectTheme", "logrite.theme": theme });
    }

    onSelectLayout(layout) {
		logger.error(`>>> Entering onSelectLayout(layout=${layout})`, { "event.action": "onSelectLayout", "logrite.layout": layout });
        if (!layout) {
            this.props.onChangeField("skinSrc", null);
        }
        const skinSrc = this.addAscxExtension(layout.path);
        this.props.onChangeField("skinSrc", skinSrc);
		logger.error(`onSelectLayout(layout=${layout}): skinSrc → ${skinSrc}`, { "event.action": "onSelectLayout", "logrite.layout": layout });
    }

    onSelectContainer(container) {
		logger.error(`>>> Entering onSelectContainer(container=${container})`, { "event.action": "onSelectContainer", "logrite.container": container });
        if (!container) {
            this.props.onChangeField("containerSrc", null);
        }
        const containerSrc = this.addAscxExtension(container.path);
        this.props.onChangeField("containerSrc", containerSrc);
		logger.error(`onSelectContainer(container=${container}): containerSrc → ${containerSrc}`, { "event.action": "onSelectContainer", "logrite.container": container });
    }

    trimAscxExtension(value) {
		logger.error(`>>> Entering trimAscxExtension(value=${value})`, { "event.action": "trimAscxExtension", "logrite.value": value });
        if (!value) return value;
		logger.warn(`<<< Exiting trimAscxExtension(value=${value})`, { "event.action": "trimAscxExtension", "logrite.value": value });
        return value.replace(".ascx", "");
    }

    addAscxExtension(value) {
		logger.error(`>>> Entering addAscxExtension(value=${value})`, { "event.action": "addAscxExtension", "logrite.value": value });
        if (!value) return value;
		logger.warn(`<<< Exiting addAscxExtension(value=${value})`, { "event.action": "addAscxExtension", "logrite.value": value });
        return value + ".ascx";
    }

    onChangePageStyleSheet(event) {
		logger.error(`>>> Entering onChangePageStyleSheet(event=${event})`, { "event.action": "onChangePageStyleSheet", "logrite.event": event });
        const value = event.target.value;
        this.props.onChangeField("pageStyleSheet", value);
		logger.warn(`<<< Exiting onChangePageStyleSheet(event=${event})`, { "event.action": "onChangePageStyleSheet", "logrite.event": event });
    }

    findByPath(componentSrc) {
		logger.error(`>>> Entering findByPath(componentSrc=${componentSrc})`, { "event.action": "findByPath", "logrite.componentSrc": componentSrc });
		logger.warn(`<<< Exiting findByPath(componentSrc=${componentSrc})`, { "event.action": "findByPath", "logrite.componentSrc": componentSrc });
        return (c) => utils.areEqualInvariantCase(this.addAscxExtension(c.path), componentSrc);
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { page, themes, layouts, containers, defaultPortalThemeName, defaultPortalThemeLevel, defaultPortalLayout, defaultPortalContainer } = this.props;
        const selectedThemeName = page.themeName || defaultPortalThemeName;
        const selectedThemeLevel = page.themeLevel || defaultPortalThemeLevel;
        const selectedTheme = themes.find(t => t.packageName === selectedThemeName && t.level === selectedThemeLevel);
		logger.error(`render(): selectedTheme → ${selectedTheme}`, { "event.action": "render" });
        const noThemeSelected = !selectedTheme;
        const selectedLayoutPath = page.skinSrc || defaultPortalLayout;
        const selectedLayout = layouts.find(this.findByPath(selectedLayoutPath));
		logger.error(`render(): selectedLayout → ${selectedLayout}`, { "event.action": "render" });
        const selectedContainerPath = page.containerSrc || defaultPortalContainer;
        const selectedContainer = containers.find(this.findByPath(selectedContainerPath));
		logger.error(`render(): selectedContainer → ${selectedContainer}`, { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={style.moduleContainer}>
                <GridCell>
                    <ThemeSelector
                        themes={themes}
                        defaultPortalThemeName={defaultPortalThemeName}
                        defaultPortalThemeLevel={defaultPortalThemeLevel}
                        selectedTheme={selectedTheme}
                        onSelectTheme={this.onSelectTheme.bind(this) } />
                </GridCell>
                <GridCell>
                    <LayoutSelector
                        noThemeSelected={noThemeSelected}
                        layouts={layouts}
                        defaultPortalLayout={this.trimAscxExtension(defaultPortalLayout) }
                        selectedLayout={selectedLayout}
                        onSelectLayout={this.onSelectLayout.bind(this) } />
                </GridCell>
                <GridCell>
                    <ContainerSelector
                        noThemeSelected={noThemeSelected}
                        containers={containers}
                        defaultPortalContainer={this.trimAscxExtension(defaultPortalContainer) }
                        selectedContainer={selectedContainer}
                        onSelectContainer={this.onSelectContainer.bind(this) } />
                </GridCell>
                <GridCell>
                    <GridCell columnSize={50}>
                        <SingleLineInputWithError style={{ width: "100%" }}
                            label={Localization.get("PageStyleSheet") }
                            tooltipMessage={Localization.get("PageStyleSheetTooltip") }
                            value={page.pageStyleSheet}
                            onChange={this.onChangePageStyleSheet.bind(this) } />
                    </GridCell>
                    <GridCell columnSize={50}>
                        <Button type="secondary" onClick={this.previewPage.bind(this) } style={{ marginTop: "25px", float: "right" }}>
                            {Localization.get("PreviewThemeLayoutAndContainer") }
                        </Button>
                    </GridCell>
                </GridCell>
            </div>
        );
    }
}

Appearance.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    onRetrieveThemes: PropTypes.func.isRequired,
    onRetrieveThemeFiles: PropTypes.func.isRequired,
    themes: PropTypes.array.isRequired,
    layouts: PropTypes.array.isRequired,
    containers: PropTypes.array.isRequired,
    defaultPortalLayout: PropTypes.string,
    defaultPortalContainer: PropTypes.string,
    defaultPortalThemeName: PropTypes.string,
    defaultPortalThemeLevel: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        themes: state.theme.themes,
        defaultPortalThemeName: state.theme.defaultPortalThemeName,
        defaultPortalThemeLevel: state.theme.defaultPortalThemeLevel,
        defaultPortalLayout: state.theme.defaultPortalLayout,
        defaultPortalContainer: state.theme.defaultPortalContainer,
        layouts: state.theme.layouts,
        containers: state.theme.containers
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return bindActionCreators({
        onRetrieveThemes: ThemeActions.retrieveThemes,
        onRetrieveThemeFiles: ThemeActions.retrieveThemeFiles
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(Appearance);
