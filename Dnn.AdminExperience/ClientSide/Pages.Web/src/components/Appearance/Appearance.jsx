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
		logger.debug('>>> Entering componentDidMount()');
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
				logger.debug('<<< Exiting componentDidMount()');
            }
        });
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
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
		logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
    }

    autoSelectFirstContainerIfNoOneIsSelected(page, containers) {
		logger.debug(`>>> Entering autoSelectFirstContainerIfNoOneIsSelected(page=${page},containers=${containers})`);
        const { onChangeField, defaultPortalContainer } = this.props;
        const selectedContainerPath = page.containerSrc || defaultPortalContainer;
        const selectedContainer = containers.find(this.findByPath(selectedContainerPath));
        if (!selectedContainer && containers.length !== 0) {
            const container = containers[0];
            onChangeField("containerSrc", this.addAscxExtension(container.path));
			logger.debug(`<<< Exiting autoSelectFirstContainerIfNoOneIsSelected(page=${page},containers=${containers})`);
        }
    }

    getPagePreviewUrl(skinSrc, containerSrc) {
		logger.debug(`>>> Entering getPagePreviewUrl(skinSrc=${skinSrc},containerSrc=${containerSrc})`);
        const { page } = this.props;
        // if is new page take current page loaded behind the persona bar for preview
        const pageUrl = page.tabId !== 0 ? page.absoluteUrl : window.parent.location.href;
        const skinSrcQueryString = this.trimAscxExtension(skinSrc);
		logger.debug(`getPagePreviewUrl(skinSrc=${skinSrc},containerSrc=${containerSrc}): skinSrcQueryString → ${skinSrcQueryString}`);
        const containerSrcQueryString = this.trimAscxExtension(containerSrc);
		logger.debug(`<<< Exiting getPagePreviewUrl(skinSrc=${skinSrc},containerSrc=${containerSrc})`);
        return utils.url.appendQueryString(pageUrl, {
            SkinSrc: skinSrcQueryString,
            ContainerSrc: containerSrcQueryString
        });
    }

    previewPage() {
		logger.debug('>>> Entering previewPage()');
        const { page, defaultPortalLayout, defaultPortalContainer } = this.props;
        const skinSrc = page.skinSrc || defaultPortalLayout;
        const containerSrc = page.containerSrc || defaultPortalContainer;

        if (!skinSrc || !containerSrc) {
            utils.notify(Localization.get("PleaseSelectLayoutContainer"));
            return;
        }

        const previewUrl = this.getPagePreviewUrl(skinSrc, containerSrc);
        window.open(previewUrl);
		logger.debug(`previewPage(): previewUrl → ${previewUrl}`);
		logger.debug('<<< Exiting previewPage()');
    }

    onSelectTheme(theme) {
		logger.debug(`>>> Entering onSelectTheme(theme=${theme})`);
        this.props.onChangeField("themeName", theme.packageName);
        this.props.onChangeField("themeLevel", theme.level);
        const skinSrc = this.addAscxExtension(theme.defaultThemeFile);
		logger.debug(`onSelectTheme(theme=${theme}): skinSrc → ${skinSrc}`);
        this.props.onChangeField("skinSrc", skinSrc);
        this.props.onRetrieveThemeFiles(theme.packageName, theme.level);
		logger.debug(`<<< Exiting onSelectTheme(theme=${theme})`);
    }

    onSelectLayout(layout) {
		logger.debug(`>>> Entering onSelectLayout(layout=${layout})`);
        if (!layout) {
            this.props.onChangeField("skinSrc", null);
        }
        const skinSrc = this.addAscxExtension(layout.path);
        this.props.onChangeField("skinSrc", skinSrc);
		logger.debug(`onSelectLayout(layout=${layout}): skinSrc → ${skinSrc}`);
		logger.debug(`<<< Exiting onSelectLayout(layout=${layout})`);
    }

    onSelectContainer(container) {
		logger.debug(`>>> Entering onSelectContainer(container=${container})`);
        if (!container) {
            this.props.onChangeField("containerSrc", null);
        }
        const containerSrc = this.addAscxExtension(container.path);
        this.props.onChangeField("containerSrc", containerSrc);
		logger.debug(`onSelectContainer(container=${container}): containerSrc → ${containerSrc}`);
		logger.debug(`<<< Exiting onSelectContainer(container=${container})`);
    }

    trimAscxExtension(value) {
		logger.debug(`>>> Entering trimAscxExtension(value=${value})`);
        if (!value) return value;
		logger.debug(`<<< Exiting trimAscxExtension(value=${value})`);
        return value.replace(".ascx", "");
    }

    addAscxExtension(value) {
		logger.debug(`>>> Entering addAscxExtension(value=${value})`);
        if (!value) return value;
		logger.debug(`<<< Exiting addAscxExtension(value=${value})`);
        return value + ".ascx";
    }

    onChangePageStyleSheet(event) {
		logger.debug(`>>> Entering onChangePageStyleSheet(event=${event})`);
        const value = event.target.value;
        this.props.onChangeField("pageStyleSheet", value);
		logger.debug(`<<< Exiting onChangePageStyleSheet(event=${event})`);
    }

    findByPath(componentSrc) {
		logger.debug(`>>> Entering findByPath(componentSrc=${componentSrc})`);
		logger.debug(`<<< Exiting findByPath(componentSrc=${componentSrc})`);
        return (c) => utils.areEqualInvariantCase(this.addAscxExtension(c.path), componentSrc);
    }

    render() {
		logger.debug('>>> Entering render()');
        const { page, themes, layouts, containers, defaultPortalThemeName, defaultPortalThemeLevel, defaultPortalLayout, defaultPortalContainer } = this.props;
        const selectedThemeName = page.themeName || defaultPortalThemeName;
		logger.debug(`render(): containers → ${containers}`);
        const selectedThemeLevel = page.themeLevel || defaultPortalThemeLevel;
        const selectedTheme = themes.find(t => t.packageName === selectedThemeName && t.level === selectedThemeLevel);
        const noThemeSelected = !selectedTheme;
        const selectedLayoutPath = page.skinSrc || defaultPortalLayout;
        const selectedLayout = layouts.find(this.findByPath(selectedLayoutPath));
        const selectedContainerPath = page.containerSrc || defaultPortalContainer;
		logger.debug(`render(): selectedTheme → ${selectedTheme}`);
		logger.debug(`render(): selectedLayout → ${selectedLayout}`);
        const selectedContainer = containers.find(this.findByPath(selectedContainerPath));

		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
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
	logger.debug(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`);
	logger.debug(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`);
    return bindActionCreators({
        onRetrieveThemes: ThemeActions.retrieveThemes,
        onRetrieveThemeFiles: ThemeActions.retrieveThemeFiles
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(Appearance);
