import React, {Component} from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";
import { Switch, Label } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../../localization";
const logger = console;


class PageUrlCommons extends Component {

    onChangeField(key) {
		logger.debug(`>>> Entering onChangeField(key=${key})`);
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
		logger.debug(`<<< Exiting onChangeField(key=${key})`);
    }

    onChangeValue(key, value) {
		logger.debug(`>>> Entering onChangeValue(key=${key},value=${value})`);
        const {onChangeField} = this.props;
        onChangeField(key, value);
		logger.debug(`<<< Exiting onChangeValue(key=${key},value=${value})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {page, display} = this.props;
        const gridClass = "page-url-commons-grid " + display;
		logger.debug(`render(): gridClass → ${gridClass}`);

		logger.debug('<<< Exiting render()');
        return (
            <div className={styles.pageUrlCommons}>
                <div className={gridClass}>
                    <div className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={Localization.get("PermanentRedirectTooltip") }
                            label={Localization.get("PermanentRedirect") }
                        />
                        <Switch
                            labelHidden={false}
                            onText={Localization.get("On") }
                            offText={Localization.get("Off") }
                            value={page.permanentRedirect}
                            onChange={this.onChangeValue.bind(this, "permanentRedirect") } />
                    </div>
                    <div className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={Localization.get("OpenLinkInNewWindowTooltip") }
                            label={Localization.get("OpenLinkInNewWindow") }
                        />
                        <Switch
                            labelHidden={false}
                            onText={Localization.get("On") }
                            offText={Localization.get("Off") }
                            value={page.linkNewWindow}
                            onChange={this.onChangeValue.bind(this, "linkNewWindow") } />
                    </div>
                </div>
            </div>
        );
    }
}

PageUrlCommons.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    display: PropTypes.oneOf(["horizontal", "vertical"]).isRequired
};

PageUrlCommons.defaultProps = {
    display: "horizontal"
};

export default PageUrlCommons;