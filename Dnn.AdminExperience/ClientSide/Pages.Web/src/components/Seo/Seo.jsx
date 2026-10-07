import React, {Component} from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
import { GridSystem, GridCell, Label, Dropdown, MultiLineInputWithError, Switch } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import PageUrls from "./PageUrls/PageUrls";
const logger = console;

class Seo extends Component {

    onChangeField(key, event) {
		logger.error(`>>> Entering onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
		logger.warn(`<<< Exiting onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
    }

    onSitemapPrioritySelected(option) {
		logger.error(`>>> Entering onSitemapPrioritySelected(option=${option})`, { "event.action": "onSitemapPrioritySelected", "logrite.option": option });
        const {onChangeField} = this.props;
        onChangeField("sitemapPriority", option.value);
		logger.warn(`<<< Exiting onSitemapPrioritySelected(option=${option})`, { "event.action": "onSitemapPrioritySelected", "logrite.option": option });
    }

    getSitemapPriorityOptions() {
		logger.error('>>> Entering getSitemapPriorityOptions()', { "event.action": "getSitemapPriorityOptions" });
        const options = [];
        for (let i = 0; i <= 1; i += 0.1) {
            const v = i.toFixed(1) / 1;
			logger.error(`getSitemapPriorityOptions(): v → ${v}`, { "event.action": "getSitemapPriorityOptions" });
            options.push({ value: v, label: v });
        }

		logger.warn('<<< Exiting getSitemapPriorityOptions()', { "event.action": "getSitemapPriorityOptions" });
        return options;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {page, onChangeField} = this.props;

        const editing = page.tabId !== 0;

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.seoContainer}>
                {editing &&
                    <PageUrls pageUrls={page.pageUrls}
                        siteAliases={page.siteAliases}
                        pageHasParent={page.hasParent}
                        primaryAliasId={page.primaryAliasId} />}

                <GridSystem>
                    <GridCell className="left-column">
                        <MultiLineInputWithError
                            label={Localization.get("PageHeaderTags") }
                            tooltipMessage={Localization.get("PageHeaderTags_tooltip") }
                            value={page.pageHeadText}
                            onChange={this.onChangeField.bind(this, "pageHeadText") } />
                    </GridCell>
                    <GridCell className="right-column">
                        <GridCell>
                            <Label
                                labelType="block"
                                tooltipMessage={Localization.get("SitemapPriority_tooltip") }
                                label={Localization.get("SitemapPriority") } />
                            <Dropdown options={this.getSitemapPriorityOptions() }
                                value={page.sitemapPriority}
                                onSelect={this.onSitemapPrioritySelected.bind(this) }
                                withBorder={true} />
                        </GridCell>
                        <GridCell className="new-section">
                            <Label
                                labelType="inline"
                                tooltipMessage={Localization.get("AllowIndexing_tooltip") }
                                label={Localization.get("AllowIndexing") }
                            />
                            <Switch
                                labelHidden={false}
                                onText={Localization.get("On") }
                                offText={Localization.get("Off") }
                                value={page.allowIndex}
                                onChange={onChangeField.bind(this, "allowIndex") } />
                        </GridCell>
                    </GridCell>
                </GridSystem>
            </div>
        );
    }
}

Seo.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};


export default Seo;