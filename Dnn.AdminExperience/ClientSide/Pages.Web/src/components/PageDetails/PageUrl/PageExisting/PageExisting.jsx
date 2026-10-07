import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.module.less";
import { InputGroup, PagePicker, Label } from "@dnnsoftware/dnn-react-common";
import utils from "../../../../utils";
import Localization from "../../../../localization";
import PageUrlCommons from "../PageUrlCommons/PageUrlCommons";
const logger = console;

const PageToTestParameters = {
    portalId: -2,
    cultureCode: "",
    isMultiLanguage: false,
    excludeAdminTabs: false,
    disabledNotSelectable: false,
    roles: "",
    sortOrder: 0
};

class PageExisting extends Component {

    onChangePage(value) {
		logger.error(`>>> Entering onChangePage(value=${value})`, { "event.action": "onChangePage", "logrite.value": value });
        const {onChangeField} = this.props;
        onChangeField("existingTabRedirection", parseInt(value) === -1 ? "" : value);
		logger.warn(`<<< Exiting onChangePage(value=${value})`, { "event.action": "onChangePage", "logrite.value": value });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {page} = this.props;
        const serviceFramework = utils.getServiceFramework();
		logger.error(`render(): serviceFramework → ${serviceFramework}`, { "event.action": "render" });
        const noneSpecifiedText = "<" + Localization.get("NoneSpecified") + ">";
		logger.error(`render(): noneSpecifiedText → ${noneSpecifiedText}`, { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.pageExisting}>
                <InputGroup>
                    <Label
                        tooltipMessage={Localization.get("ExistingPageTooltip") }
                        label={Localization.get("ExistingPage") } />
                    <PagePicker
                        serviceFramework={serviceFramework}
                        style={{ width: "100%", zIndex: 2 }}
                        OnSelect={this.onChangePage.bind(this) }
                        defaultLabel={noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={PageToTestParameters}
                        selectedTabId={page.existingTabRedirection || -1} />
                </InputGroup>
                <PageUrlCommons {...this.props} />
                <div style={{ clear: "both" }}></div>
            </div>
        );
    }
}

PageExisting.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

const mapStateToProps = (state) => {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return ({page : state.pages.selectedPage});
};

export default connect(mapStateToProps)(PageExisting);