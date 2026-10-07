import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../actions";
import { InputGroup, MultiLineInputWithError, PagePicker, GridSystem, Label, Button } from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

let isHost = false;

class DefaultPagesSettingsPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this._mounted = false;
		logger.debug(`constructor(): this._mounted → ${this._mounted}`, { "event.action": "constructor" });
        this.state = {
            defaultPagesSettings: undefined
        };
        isHost = util.settings.isHost;
		logger.debug(`constructor(): this.state → ${this.state}`, { "event.action": "constructor" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this._mounted = true;
        this.loadData();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate() {
		logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
        let { props} = this;
        if (props.defaultPagesSettings) {
            let portalIdChanged = props.portalId !== undefined && props.defaultPagesSettings.PortalId !== props.portalId;
            let cultureCodeChanged = props.cultureCode !== undefined && props.defaultPagesSettings.CultureCode !== props.cultureCode;

            if (portalIdChanged || cultureCodeChanged) {
                this.loadData();
				logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
            }
        }
    }

    componentWillUnmount() {
		logger.error('>>> Entering componentWillUnmount()', { "event.action": "componentWillUnmount" });
        this._mounted = false;
		logger.warn('<<< Exiting componentWillUnmount()', { "event.action": "componentWillUnmount" });
    }

    loadData() {
		logger.error('>>> Entering loadData()', { "event.action": "loadData" });
        const { props } = this;
        props.dispatch(SiteBehaviorActions.getDefaultPagesSettings(props.portalId, props.cultureCode, (data) => {
            if (this._mounted) {
				logger.warn('<<< Exiting loadData()', { "event.action": "loadData" });
                this.setState({
                    defaultPagesSettings: Object.assign({}, data.Settings)
                });
            }
        }));
    }

    onSettingChange(key, event, value) {
		logger.error(`>>> Entering onSettingChange(key=${key},event=${event},value=${value})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event, "logrite.value": value });
        let {state, props} = this;

        let defaultPagesSettings = Object.assign({}, state.defaultPagesSettings);
		logger.error(`onSettingChange(key=${key},event=${event},value=${value}): defaultPagesSettings → ${defaultPagesSettings}`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event, "logrite.value": value });

        if (key === "SplashTabId" || key === "HomeTabId" || key === "LoginTabId" || key === "RegisterTabId" ||
            key === "UserTabId" || key === "SearchTabId" || key === "Custom404TabId" || key === "Custom500TabId" ||
            key === "TermsTabId" || key === "PrivacyTabId") {
            if (defaultPagesSettings[key] !== parseInt(event)) {
                defaultPagesSettings[key] = event;
                defaultPagesSettings[key.replace("Id", "Name")] = value;
            }
            else {
                return;
            }
        }
        else {
            defaultPagesSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            defaultPagesSettings: defaultPagesSettings
        });

        props.dispatch(SiteBehaviorActions.defaultPagesSettingsClientModified(defaultPagesSettings));
		logger.warn(`<<< Exiting onSettingChange(key=${key},event=${event},value=${value})`, { "event.action": "onSettingChange", "logrite.key": key, "logrite.event": event, "logrite.value": value });
    }

    onUpdate(event) {
		logger.error(`>>> Entering onUpdate(event=${event})`, { "event.action": "onUpdate", "logrite.event": event });
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SiteBehaviorActions.updateDefaultPagesSettings(state.defaultPagesSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
			logger.warn(`<<< Exiting onUpdate(event=${event})`, { "event.action": "onUpdate", "logrite.event": event });
        }));
    }

    onCancel() {
		logger.error('>>> Entering onCancel()', { "event.action": "onCancel" });
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.getDefaultPagesSettings(props.portalId, props.cultureCode, (data) => {
				logger.warn('<<< Exiting onCancel()', { "event.action": "onCancel" });
                let defaultPagesSettings = Object.assign({}, data.Settings);
                this.setState({
                    defaultPagesSettings
                });
            }));
        });
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
		logger.error(`render(): noneSpecifiedText → ${noneSpecifiedText}`, { "event.action": "render" });
        const TabParameters = {
            portalId: props.portalId !== undefined ? props.portalId : -2,
            cultureCode: props.cultureCode || "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            roles: "",
            sortOrder: 0
        };
        let TabParameters_1 = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: false });
		logger.error(`render(): TabParameters_1 → ${TabParameters_1}`, { "event.action": "render" });
        let TabParameters_2 = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: true });
		logger.error(`render(): TabParameters_2 → ${TabParameters_2}`, { "event.action": "render" });
        let TabParameters_Login = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: false, validateTab: "Account Login" });
		logger.error(`render(): TabParameters_Login → ${TabParameters_Login}`, { "event.action": "render" });
        let TabParameters_Search = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: false, validateTab: "Search Results" });
		logger.error(`render(): TabParameters_Search → ${TabParameters_Search}`, { "event.action": "render" });
        let TabParameters_Anonymous = Object.assign(Object.assign({}, TabParameters), { disabledNotSelectable: true, roles: "-1" });
        if (state.defaultPagesSettings) {
            const columnOne = <div key="column-one" className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plHomeTabId.Help")}
                        label={resx.get("plHomeTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 7 }}
                        selectedTabId={state.defaultPagesSettings.HomeTabId ? state.defaultPagesSettings.HomeTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "HomeTabId")}
                        defaultLabel={state.defaultPagesSettings.HomeTabName ? state.defaultPagesSettings.HomeTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plLoginTabId.Help")}
                        label={resx.get("plLoginTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 6 }}
                        selectedTabId={state.defaultPagesSettings.LoginTabId ? state.defaultPagesSettings.LoginTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "LoginTabId")}
                        defaultLabel={state.defaultPagesSettings.LoginTabName ? state.defaultPagesSettings.LoginTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_Login}
                    />
                </InputGroup>
                <InputGroup>
                    {/* Setting a style here to workaround this issue: https://github.com/romainberger/react-portal-tooltip/issues/84
                        TODO: remove the style once the underlying issue is resolved */}
                    <Label
                        tooltipMessage={resx.get("plRegisterTabId.Help")}
                        label={resx.get("plRegisterTabId")}                        
                        tooltipStyle={{ float: "", width : "68%"}}                        
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 5 }}
                        selectedTabId={state.defaultPagesSettings.RegisterTabId ? state.defaultPagesSettings.RegisterTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "RegisterTabId")}
                        defaultLabel={state.defaultPagesSettings.RegisterTabName ? state.defaultPagesSettings.RegisterTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plUserTabId.Help")}
                        label={resx.get("plUserTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 4 }}
                        selectedTabId={state.defaultPagesSettings.UserTabId ? state.defaultPagesSettings.UserTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "UserTabId")}
                        defaultLabel={state.defaultPagesSettings.UserTabName ? state.defaultPagesSettings.UserTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("Redirect_AfterLogin.Help") }
                        label={resx.get("Redirect_AfterLogin") } />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 3 }}
                        selectedTabId={state.defaultPagesSettings.RedirectAfterLoginTabId ? state.defaultPagesSettings.RedirectAfterLoginTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "RedirectAfterLoginTabId") }
                        defaultLabel={state.defaultPagesSettings.RedirectAfterLoginTabName ? state.defaultPagesSettings.RedirectAfterLoginTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_2}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("Redirect_AfterLogout.Help") }
                        label={resx.get("Redirect_AfterLogout") } />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 2 }}
                        selectedTabId={state.defaultPagesSettings.RedirectAfterLogoutTabId ? state.defaultPagesSettings.RedirectAfterLogoutTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "RedirectAfterLogoutTabId") }
                        defaultLabel={state.defaultPagesSettings.RedirectAfterLogoutTabName ? state.defaultPagesSettings.RedirectAfterLogoutTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_Anonymous}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("Redirect_AfterRegistration.Help") }
                        label={resx.get("Redirect_AfterRegistration") } />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 1 }}
                        selectedTabId={state.defaultPagesSettings.RedirectAfterRegistrationTabId ? state.defaultPagesSettings.RedirectAfterRegistrationTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "RedirectAfterRegistrationTabId") }
                        defaultLabel={state.defaultPagesSettings.RedirectAfterRegistrationTabName ? state.defaultPagesSettings.RedirectAfterRegistrationTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_2}
                    />
                </InputGroup>
            </div>;
            const columnTwo = <div key="column-two" className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plSplashTabId.Help")}
                        label={resx.get("plSplashTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 7 }}
                        selectedTabId={state.defaultPagesSettings.SplashTabId ? state.defaultPagesSettings.SplashTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "SplashTabId")}
                        defaultLabel={state.defaultPagesSettings.SplashTabName ? state.defaultPagesSettings.SplashTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plSearchTabId.Help")}
                        label={resx.get("plSearchTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 6 }}
                        selectedTabId={state.defaultPagesSettings.SearchTabId ? state.defaultPagesSettings.SearchTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "SearchTabId")}
                        defaultLabel={state.defaultPagesSettings.SearchTabName ? state.defaultPagesSettings.SearchTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_Search}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("pl404TabId.Help")}
                        label={resx.get("pl404TabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 5 }}
                        selectedTabId={state.defaultPagesSettings.Custom404TabId ? state.defaultPagesSettings.Custom404TabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "Custom404TabId")}
                        defaultLabel={state.defaultPagesSettings.Custom404TabName ? state.defaultPagesSettings.Custom404TabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_2}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("pl500TabId.Help")}
                        label={resx.get("pl500TabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 4 }}
                        selectedTabId={state.defaultPagesSettings.Custom500TabId ? state.defaultPagesSettings.Custom500TabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "Custom500TabId")}
                        defaultLabel={state.defaultPagesSettings.Custom500TabName ? state.defaultPagesSettings.Custom500TabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_2}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plTermsTabId.Help")}
                        label={resx.get("plTermsTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 3 }}
                        selectedTabId={state.defaultPagesSettings.TermsTabId ? state.defaultPagesSettings.TermsTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "TermsTabId")}
                        defaultLabel={state.defaultPagesSettings.TermsTabName ? state.defaultPagesSettings.TermsTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plPrivacyTabId.Help")}
                        label={resx.get("plPrivacyTabId")}
                    />
                    <PagePicker
                        serviceFramework={util.utilities.sf}
                        style={{ width: "100%", zIndex: 2 }}
                        selectedTabId={state.defaultPagesSettings.PrivacyTabId ? state.defaultPagesSettings.PrivacyTabId : -1}
                        OnSelect={this.onSettingChange.bind(this, "PrivacyTabId")}
                        defaultLabel={state.defaultPagesSettings.PrivacyTabName ? state.defaultPagesSettings.PrivacyTabName : noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={TabParameters_1}
                    />
                </InputGroup>
            </div>;

            return (
                <div className={styles.defaultPagesSettings}>
                    <GridSystem numberOfColumns={2}>
                        {[columnOne, columnTwo]}
                    </GridSystem>
                    {isHost &&
                        <div className="sectionTitle">{resx.get("PageOutputSettings")}</div>}
                    {isHost &&
                        <InputGroup style={{ paddingTop: "10px" }}>
                            <Label
                                tooltipMessage={resx.get("plPageHeadText.Help")}
                                label={resx.get("plPageHeadText")}
                            />
                            <MultiLineInputWithError
                                value={state.defaultPagesSettings.PageHeadText}
                                onChange={this.onSettingChange.bind(this, "PageHeadText")}
                            />
                        </InputGroup>
                    }

                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.defaultPagesSettingsClientModified}
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.defaultPagesSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

DefaultPagesSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    defaultPagesSettings: PropTypes.object,
    defaultPagesSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        defaultPagesSettings: state.siteBehavior.defaultPagesSettings,
        defaultPagesSettingsClientModified: state.siteBehavior.defaultPagesSettingsClientModified,
    };
}

export default connect(mapStateToProps)(DefaultPagesSettingsPanelBody);
