import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { GridSystem, GridCell, Label, Switch, Dropdown, Button, RadioButtons, SingleLineInputWithError } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import { pageActions as PageActions } from "../../actions";
import styles from "./style.module.less";
import utils from "../../utils";
const logger = console;

class More extends Component {
    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.props.onFetchCacheProviderList();
        if (this.props.page.cacheProvider && !this.props.cachedPageCount) {
            this.props.onGetCachedPageCount(this.props.page.cacheProvider);
			logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
        }
    }

    onChangeField(key, event) {
		logger.error(`>>> Entering onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
        const { onChangeField } = this.props;
        onChangeField(key, event.target.value);
		logger.warn(`<<< Exiting onChangeField(key=${key},event=${event})`, { "event.action": "onChangeField", "logrite.key": key, "logrite.event": event });
    }

    onCacheProviderSelected(option) {
		logger.error(`>>> Entering onCacheProviderSelected(option=${option})`, { "event.action": "onCacheProviderSelected", "logrite.option": option });
        this.props.onChangeField("cacheProvider", option.value);
        if (!this.props.page.cacheProvider && option.value) {
            this.props.onChangeField("cacheIncludeExclude", true);
            this.props.onChangeField("cacheDuration", "");
            this.props.onChangeField("cacheMaxVaryByCount", "");
        }
        if (option.value) {
            this.props.onGetCachedPageCount(option.value);
			logger.warn(`<<< Exiting onCacheProviderSelected(option=${option})`, { "event.action": "onCacheProviderSelected", "logrite.option": option });
        }
        this.setState({
            cacheProvider: option.value
        });
    }

    onChangeIncludeExclude(value) {
		logger.error(`>>> Entering onChangeIncludeExclude(value=${value})`, { "event.action": "onChangeIncludeExclude", "logrite.value": value });
        const include = value === "true";
        this.props.onChangeField("cacheIncludeExclude", include);
		logger.error(`onChangeIncludeExclude(value=${value}): include → ${include}`, { "event.action": "onChangeIncludeExclude", "logrite.value": value });
    }

    onClearCache() {
		logger.error('>>> Entering onClearCache()', { "event.action": "onClearCache" });
        this.props.onClearCache(this.props.page.cacheProvider);
		logger.warn('<<< Exiting onClearCache()', { "event.action": "onClearCache" });
    }

    getWorkflowOptions() {
		logger.error('>>> Entering getWorkflowOptions()', { "event.action": "getWorkflowOptions" });
		logger.warn('<<< Exiting getWorkflowOptions()', { "event.action": "getWorkflowOptions" });
        return this.props.workflowList.length ? this.props.workflowList.map((item => { return { value: item.workflowId, label: item.workflowName }; })) : null;
    }

    onWorkflowDropDownChange(option) {
		logger.error(`>>> Entering onWorkflowDropDownChange(option=${option})`, { "event.action": "onWorkflowDropDownChange", "logrite.option": option });
        this.props.onChangeField("workflowId", option.value);
        this.props.page.workflowId = option.value;
        this.setState({
            page: {...this.props.page},
        });
		logger.warn(`<<< Exiting onWorkflowDropDownChange(option=${option})`, { "event.action": "onWorkflowDropDownChange", "logrite.option": option });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const { page, errors, onChangeField, cacheProviderList } = this.props;
        const cacheProviderOptions = cacheProviderList &&
            [{ value: null, label: Localization.get("None") },
                ...cacheProviderList.map(x => ({ value: x, label: x }))];
        const includeExcludeOptions = [
            { value: true, label: Localization.get("Include") },
            { value: false, label: Localization.get("Exclude") }];

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.moreContainer}>
                <div className="title">
                    {Localization.get("Security")}
                </div>
                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={Localization.get("SecureConnection_tooltip")}
                            label={Localization.get("SecureConnection")}
                        />
                        <Switch
                            labelHidden={false}
                            onText={Localization.get("On")}
                            offText={Localization.get("Off")}
                            value={page.isSecure}
                            onChange={onChangeField.bind(this, "isSecure")} />
                    </GridCell>
                    <GridCell className="right-column">
                        <Label
                            labelType="inline"
                            tooltipMessage={Localization.get("DisableLink_tooltip")}
                            label={Localization.get("DisableLink")}
                        />
                        <Switch
                            labelHidden={false}
                            onText={Localization.get("On")}
                            offText={Localization.get("Off")}
                            value={page.disableLink}
                            onChange={onChangeField.bind(this, "disableLink")} />
                    </GridCell>
                </GridSystem>

                <div className="title sectionTitle">
                    {Localization.get("CacheSettings")}
                </div>

                <GridSystem>
                    <GridCell className="left-column">
                        <Label
                            labelType="block"
                            tooltipMessage={Localization.get("OutputCacheProvider_tooltip")}
                            label={Localization.get("OutputCacheProvider")}
                        />
                        {cacheProviderOptions &&
                            <Dropdown options={cacheProviderOptions}
                                value={page.cacheProvider}
                                onSelect={this.onCacheProviderSelected.bind(this)}
                                withBorder={true} />}

                        {page.cacheProvider &&
                            <SingleLineInputWithError
                                error={!!errors.cacheDuration}
                                errorMessage={errors.cacheDuration}
                                label={Localization.get("CacheDuration")}
                                tooltipMessage={Localization.get("CacheDuration_tooltip")}
                                value={page.cacheDuration}
                                onChange={this.onChangeField.bind(this, "cacheDuration")} />}
                        {page.cacheProvider &&
                            <div className="clear-cache">
                                <Label
                                    labelType="block"
                                    label={Localization.get("lblCachedItemCount").replace("{0}", this.props.cachedPageCount)}
                                />
                                <Button
                                    disabled={this.props.cachedPageCount === 0}
                                    type="danger"
                                    onClick={this.onClearCache.bind(this)}>
                                    {Localization.get("ClearPageCache")}
                                </Button>
                            </div>}
                    </GridCell>

                    {page.cacheProvider &&
                        <GridCell className="right-column">
                            <Label
                                labelType="inline"
                                tooltipMessage={Localization.get("IncludeExcludeParams_tooltip")}
                                label={Localization.get("IncludeExcludeParams")} />
                            <RadioButtons
                                options={includeExcludeOptions}
                                value={page.cacheIncludeExclude}
                                onChange={this.onChangeIncludeExclude.bind(this)} />

                            {!page.cacheIncludeExclude &&
                                <SingleLineInputWithError
                                    label={Localization.get("IncludeParamsInCacheValidation")}
                                    tooltipMessage={Localization.get("IncludeParamsInCacheValidation_tooltip")}
                                    value={page.cacheIncludeVaryBy}
                                    onChange={this.onChangeField.bind(this, "cacheIncludeVaryBy")} />}

                            {page.cacheIncludeExclude &&
                                <SingleLineInputWithError
                                    label={Localization.get("ExcludeParamsInCacheValidation")}
                                    tooltipMessage={Localization.get("ExcludeParamsInCacheValidation_tooltip")}
                                    value={page.cacheExcludeVaryBy}
                                    onChange={this.onChangeField.bind(this, "cacheExcludeVaryBy")} />}

                            <SingleLineInputWithError
                                error={!!errors.cacheMaxVaryByCount}
                                errorMessage={errors.cacheMaxVaryByCount}
                                label={Localization.get("VaryByLimit")}
                                tooltipMessage={Localization.get("VaryByLimit_tooltip")}
                                value={page.cacheMaxVaryByCount}
                                onChange={this.onChangeField.bind(this, "cacheMaxVaryByCount")} />

                        </GridCell>
                    }
                </GridSystem>

                {utils.isPlatform() && <>
                    <div className="title sectionTitle">{Localization.get("WorkflowSettings")}</div>
                    <GridSystem>
                        <GridCell className="left-column">
                            <Label
                                labelType="inline"
                                tooltipMessage={Localization.get("EnabledVersioning_tooltip")}
                                label={Localization.get("EnabledVersioning")} />
                            <Switch
                                onText={Localization.get("On")}
                                offText={Localization.get("Off")}
                                value={page.enabledVersioning}
                                onChange={onChangeField.bind(this, "enabledVersioning")} />

                        </GridCell>
                        {page.enabledVersioning &&
                            <GridCell className="right-column">

                                <Label
                                    labelType="inline"
                                    tooltipMessage={Localization.get("WorkflowEnabled_tooltip")}
                                    label={Localization.get("WorkflowEnabled")} />
                                <Switch
                                    onText={Localization.get("On")}
                                    offText={Localization.get("Off")}
                                    value={page.workflowEnabled}
                                    onChange={onChangeField.bind(this, "workflowEnabled")} />
                                {page.workflowEnabled &&
                                    <Dropdown
                                        className="more-dropdown"
                                        options={this.getWorkflowOptions()}
                                        label={page.workflowId ? this.getWorkflowOptions().find(x => x.value === page.workflowId).label : Localization.get("Workflow")}
                                        value={page.workflowId !== "" && page.workflowId}
                                        onSelect={this.onWorkflowDropDownChange.bind(this)}                               
                                        withBorder={true} />}
                            </GridCell>
                        }
                    </GridSystem></>
                }
            </div>
        );
    }
}

More.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired,
    cacheProviderList: PropTypes.array,
    onFetchCacheProviderList: PropTypes.func.isRequired,
    onGetCachedPageCount: PropTypes.func.isRequired,
    cachedPageCount: PropTypes.number,
    onClearCache: PropTypes.func.isRequired,
    errors: PropTypes.object.isRequired,
    workflowList: PropTypes.array,
    getWorkflowsList: PropTypes.func.isRequired,
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        cacheProviderList: state.pages.cacheProviderList,
        cachedPageCount: state.pages.cachedPageCount,
        workflowList: state.pages.workflowList,
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return bindActionCreators({
        onFetchCacheProviderList: PageActions.fetchCacheProviderList,
        onGetCachedPageCount: PageActions.getCachedPageCount,
        onClearCache: PageActions.clearCache
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(More);