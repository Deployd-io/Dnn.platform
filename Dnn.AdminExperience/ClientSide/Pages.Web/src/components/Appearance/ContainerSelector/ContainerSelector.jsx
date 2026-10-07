import React, {Component} from "react";
import PropTypes from "prop-types";
import { Label } from "@dnnsoftware/dnn-react-common";
import localization from "../../../localization";
import Card from "../Card/Card";
import Gallery from "../Gallery/Gallery";
const logger = console;

class ContainerSelector extends Component {

    onContainerClick(containerName) {
		logger.error(`>>> Entering onContainerClick(containerName=${containerName})`, { "event.action": "onContainerClick", "logrite.containerName": containerName });
        const container = this.props.containers.find(c => c.name === containerName);
        this.props.onSelectContainer(container);
		logger.error(`onContainerClick(containerName=${containerName}): container → ${container}`, { "event.action": "onContainerClick", "logrite.containerName": containerName });
    }

    isSelected(container) {
		logger.error(`>>> Entering isSelected(container=${container})`, { "event.action": "isSelected", "logrite.container": container });
        const { selectedContainer } = this.props;
        if (!selectedContainer) {
            return false;
        }
		logger.warn(`<<< Exiting isSelected(container=${container})`, { "event.action": "isSelected", "logrite.container": container });
        return selectedContainer.name === container.name;
    }

    getSelectedIndex() {
		logger.error('>>> Entering getSelectedIndex()', { "event.action": "getSelectedIndex" });
        const { selectedContainer, containers } = this.props;
        if (!selectedContainer) {
            return -1;
        }
		logger.warn('<<< Exiting getSelectedIndex()', { "event.action": "getSelectedIndex" });
        return containers.findIndex(c => c.name === selectedContainer.name);
    }

    getContainerCards() {
		logger.error('>>> Entering getContainerCards()', { "event.action": "getContainerCards" });
        const { containers, noThemeSelected } = this.props;
        let {defaultPortalContainer} = this.props;
        defaultPortalContainer = defaultPortalContainer === null ? "" : defaultPortalContainer;
		logger.error(`getContainerCards(): defaultPortalContainer → ${defaultPortalContainer}`, { "event.action": "getContainerCards" });

        if (noThemeSelected) {
            return <div className="no-appearance-items">{localization.get("NoThemeSelectedForContainers") }</div>;
        }
        if (containers.length === 0) {
            return <div className="no-appearance-items">{localization.get("NoContainers") }</div>;
        }
        return containers.map(container => {
            return <Card
                key={container.name}
                cardId={container.name}
                onClick={this.onContainerClick.bind(this) }
                hoverText={localization.get("SetPageContainer") }
                label={container.name}
                isSiteDefault={defaultPortalContainer.toString().toLowerCase() === container.path.toString().toLowerCase() }
                image={container.thumbnail}
                selected={this.isSelected(container) }
                size="small" />;
        });
		logger.warn('<<< Exiting getContainerCards()', { "event.action": "getContainerCards" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const selectedIndex = this.getSelectedIndex();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <Label
                    label={localization.get("PageContainer") }
                    tooltipMessage={localization.get("PageContainerTooltip") } />
                <Gallery size="small" scrollToIndex={selectedIndex}>
                    {this.getContainerCards() }
                </Gallery>
            </div>
        );
    }
}

ContainerSelector.propTypes = {
    noThemeSelected: PropTypes.bool.isRequired,
    selectedContainer: PropTypes.object,
    containers: PropTypes.array.isRequired,
    onSelectContainer: PropTypes.func.isRequired,
    defaultPortalContainer: PropTypes.string
};

export default ContainerSelector;