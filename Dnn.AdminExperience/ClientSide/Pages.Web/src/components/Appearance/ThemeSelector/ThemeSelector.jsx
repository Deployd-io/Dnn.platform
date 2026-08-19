import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";
const logger = console;

class ThemeSelector extends Component {

    onThemeClick(theme) {
		logger.debug(`>>> Entering onThemeClick(theme=${theme})`);
        this.props.onSelectTheme(theme);
		logger.debug(`<<< Exiting onThemeClick(theme=${theme})`);
    }

    isSelected(theme) {
		logger.debug(`>>> Entering isSelected(theme=${theme})`);
        const { selectedTheme } = this.props;
        if (!selectedTheme) {
		logger.debug(`<<< Exiting isSelected(theme=${theme})`);
            return false;
        }

        return selectedTheme.packageName === theme.packageName && selectedTheme.level === theme.level;
    }

    getSelectedIndex() {
		logger.debug('>>> Entering getSelectedIndex()');
        const { selectedTheme, themes } = this.props;
        if (!selectedTheme) {
		logger.debug('<<< Exiting getSelectedIndex()');
            return -1;
        }
        return themes.findIndex(c => c.packageName === selectedTheme.packageName);
    }

    getThemeCards() {
		logger.debug('>>> Entering getThemeCards()');
        const { themes } = this.props;
        let {defaultPortalThemeName, defaultPortalThemeLevel} = this.props;
        defaultPortalThemeName = defaultPortalThemeName === null ? "" : defaultPortalThemeName;
        defaultPortalThemeLevel = defaultPortalThemeLevel === null ? "" : defaultPortalThemeLevel;

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
		logger.debug('<<< Exiting getThemeCards()');
    }

    render() {
		logger.debug('>>> Entering render()');
        const selectedIndex = this.getSelectedIndex();
		logger.debug('<<< Exiting render()');
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