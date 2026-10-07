import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";
const logger = console;

class ThemeSelector extends Component {

    onThemeClick(theme) {
		logger.error(`>>> Entering onThemeClick(theme=${theme})`, { "event.action": "onThemeClick", "logrite.theme": theme });
        this.props.onSelectTheme(theme);
		logger.warn(`<<< Exiting onThemeClick(theme=${theme})`, { "event.action": "onThemeClick", "logrite.theme": theme });
    }

    isSelected(theme) {
		logger.error(`>>> Entering isSelected(theme=${theme})`, { "event.action": "isSelected", "logrite.theme": theme });
        const { selectedTheme } = this.props;
        if (!selectedTheme) {
            return false;
        }

		logger.warn(`<<< Exiting isSelected(theme=${theme})`, { "event.action": "isSelected", "logrite.theme": theme });
        return selectedTheme.packageName === theme.packageName && selectedTheme.level === theme.level;
    }

    getSelectedIndex() {
		logger.error('>>> Entering getSelectedIndex()', { "event.action": "getSelectedIndex" });
        const { selectedTheme, themes } = this.props;
        if (!selectedTheme) {
            return -1;
        }
		logger.warn('<<< Exiting getSelectedIndex()', { "event.action": "getSelectedIndex" });
        return themes.findIndex(c => c.packageName === selectedTheme.packageName);
    }

    getThemeCards() {
		logger.error('>>> Entering getThemeCards()', { "event.action": "getThemeCards" });
        const { themes } = this.props;
        let {defaultPortalThemeName, defaultPortalThemeLevel} = this.props;
        defaultPortalThemeName = defaultPortalThemeName === null ? "" : defaultPortalThemeName;
		logger.error(`getThemeCards(): defaultPortalThemeName → ${defaultPortalThemeName}`, { "event.action": "getThemeCards" });
        defaultPortalThemeLevel = defaultPortalThemeLevel === null ? "" : defaultPortalThemeLevel;
		logger.error(`getThemeCards(): defaultPortalThemeLevel → ${defaultPortalThemeLevel}`, { "event.action": "getThemeCards" });

        if (themes.length === 0) {
            return <div className="no-appearance-items">{localization.get("NoThemes") }</div>;
        }
        return themes.map(theme => {
            return <Card
                key={theme.packageName}
                cardId={theme.packageName}
                onClick={this.onThemeClick.bind(this, theme) }
                hoverText={localization.get("SetPageTheme") }
                label={theme.packageName}
                isSiteDefault={(defaultPortalThemeName.toString().toLowerCase() === theme.packageName.toString().toLowerCase()) && defaultPortalThemeLevel === theme.level }
                selected={this.isSelected(theme) }
                image={theme.thumbnail} />;
        });
		logger.warn('<<< Exiting getThemeCards()', { "event.action": "getThemeCards" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const selectedIndex = this.getSelectedIndex();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <Label
                    label={localization.get("PageTheme") }
                    tooltipMessage={localization.get("PageThemeTooltip") } />
                <Gallery scrollToIndex={selectedIndex}>
                    {this.getThemeCards() }
                </Gallery>
            </div>
        );
    }
}

ThemeSelector.propTypes = {
    selectedTheme: PropTypes.object,
    themes: PropTypes.array.isRequired,
    onSelectTheme: PropTypes.func.isRequired,
    defaultPortalThemeName: PropTypes.string,
    defaultPortalThemeLevel: PropTypes.number
};

export default ThemeSelector;