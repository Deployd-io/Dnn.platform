import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";
const logger = console;

class LayoutSelector extends Component {

    onClickLayout(layoutName) {
		logger.error(`>>> Entering onClickLayout(layoutName=${layoutName})`, { "event.action": "onClickLayout", "logrite.layoutName": layoutName });
        const layout = this.props.layouts.find(l => l.name === layoutName);
        this.props.onSelectLayout(layout);
		logger.error(`onClickLayout(layoutName=${layoutName}): layout → ${layout}`, { "event.action": "onClickLayout", "logrite.layoutName": layoutName });
    }

    isSelected(layout) {
		logger.error(`>>> Entering isSelected(layout=${layout})`, { "event.action": "isSelected", "logrite.layout": layout });
        const { selectedLayout } = this.props;
        if (!selectedLayout) {
            return false;
        }
		logger.warn(`<<< Exiting isSelected(layout=${layout})`, { "event.action": "isSelected", "logrite.layout": layout });
        return selectedLayout.name === layout.name;
    }

    getSelectedIndex() {
		logger.error('>>> Entering getSelectedIndex()', { "event.action": "getSelectedIndex" });
        const { selectedLayout, layouts } = this.props;
        if (!selectedLayout) {
            return -1;
        }
		logger.warn('<<< Exiting getSelectedIndex()', { "event.action": "getSelectedIndex" });
        return layouts.findIndex(c => c.name === selectedLayout.name);
    }

    getLayoutCards() {
		logger.error('>>> Entering getLayoutCards()', { "event.action": "getLayoutCards" });
        const { layouts, noThemeSelected } = this.props;
        let {defaultPortalLayout} = this.props;
        defaultPortalLayout = defaultPortalLayout === null ? "" : defaultPortalLayout;
		logger.error(`getLayoutCards(): defaultPortalLayout → ${defaultPortalLayout}`, { "event.action": "getLayoutCards" });

        if (noThemeSelected) {
            return <div className="no-appearance-items">{localization.get("NoThemeSelectedForLayouts") }</div>;
        }
        if (layouts.length === 0) {
            return <div className="no-appearance-items">{localization.get("NoLayouts") }</div>;
        }
        return layouts.map(layout => {
            return <Card
                key={layout.name}
                cardId={layout.name}
                onClick={this.onClickLayout.bind(this) }
                hoverText={localization.get("SetPageLayout") }
                label={layout.name}
                isSiteDefault={defaultPortalLayout.toString().toLowerCase() === layout.path.toString().toLowerCase() }
                image={layout.thumbnail}
                selected={this.isSelected(layout) }
                size="small" />;
        });
		logger.warn('<<< Exiting getLayoutCards()', { "event.action": "getLayoutCards" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const selectedIndex = this.getSelectedIndex();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <Label
                    label={localization.get("Layout") }
                    tooltipMessage={localization.get("PageLayoutTooltip") } />
                <Gallery size="small" scrollToIndex={selectedIndex}>
                    {this.getLayoutCards() }
                </Gallery>
            </div>
        );
    }
}

LayoutSelector.propTypes = {
    noThemeSelected: PropTypes.bool.isRequired,
    selectedLayout: PropTypes.object,
    layouts: PropTypes.array.isRequired,
    onSelectLayout: PropTypes.func.isRequired,
    defaultPortalLayout: PropTypes.string
};

export default LayoutSelector;