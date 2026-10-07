import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    seo as SeoActions
} from "../../actions";
import ProviderRow from "./providerRow";
import ProviderEditor from "./providerEditor";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.module.less";
const logger = console;

let tableFields = [];

class ExtensionUrlProvidersPanelBody extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            openId: ""
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;

        tableFields = [];
        tableFields.push({ "name": resx.get("Name.Header"), "id": "Name" });
        tableFields.push({ "name": resx.get("Enabled.Header"), "id": "Enabled" });

        props.dispatch(SeoActions.getExtensionUrlProviders());
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    onUpdateProviderStatus(providerId, isActive) {
        const {props} = this;

        props.dispatch(SeoActions.updateExtensionUrlProviderStatus({ ProviderId: providerId, IsActive: isActive }, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    uncollapse(id) {
		logger.error(`>>> Entering uncollapse(id=${id})`, { "event.action": "uncollapse", "logrite.id": id });
        setTimeout(() => {
            this.setState({
                openId: id
            });
        }, this.timeout);
		logger.warn(`<<< Exiting uncollapse(id=${id})`, { "event.action": "uncollapse", "logrite.id": id });
    }

    collapse() {
		logger.error('>>> Entering collapse()', { "event.action": "collapse" });
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
			logger.warn('<<< Exiting collapse()', { "event.action": "collapse" });
        }
    }

    toggle(openId) {
		logger.error(`>>> Entering toggle(openId=${openId})`, { "event.action": "toggle", "logrite.openId": openId });
        if (openId !== "") {
            this.uncollapse(openId);
        } else {
            this.collapse();
			logger.warn(`<<< Exiting toggle(openId=${openId})`, { "event.action": "toggle", "logrite.openId": openId });
        }
    }

    renderProvidersHeader() {
		logger.error('>>> Entering renderProvidersHeader()', { "event.action": "renderProvidersHeader" });
        let tableHeaders = tableFields.map((field) => {
            let className = "provider-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}</span>
            </div>;
        });
		logger.warn('<<< Exiting renderProvidersHeader()', { "event.action": "renderProvidersHeader" });
        return <div className="header-row">{tableHeaders}</div>;
    }

    renderedProviders() {
        if (this.props.providers) {
            if (this.props.providers.length > 0) {
                return this.props.providers.map((item, index) => {
                    return (
                        <ProviderRow
                            providerId={item.ExtensionUrlProviderId}
                            name={item.ProviderName}
                            enabled={item.IsActive}
                            index={index}
                            key={"provider-" + index}
                            closeOnClick={true}
                            openId={this.state.openId}
                            onUpdateStatus={this.onUpdateProviderStatus.bind(this)}
                            OpenCollapse={this.toggle.bind(this)}
                            Collapse={this.collapse.bind(this)}>
                            <ProviderEditor
                                settingUrl={item.SettingUrl}
                                Collapse={this.collapse.bind(this)}
                                openId={this.state.openId} />
                        </ProviderRow>
                    );
                });
            }
            else {
                return <GridCell className="no-extension-url-providers">{resx.get("NoExtensionUrlProviders")}</GridCell>;
            }
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.extensionUrlProviders}>
                <div className="columnTitle">{resx.get("ExtensionUrlProviders")}</div>
                <div className="provider-items-grid">
                    {this.renderProvidersHeader()}
                    {this.renderedProviders()}
                </div>
            </div>
        );
    }
}

ExtensionUrlProvidersPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    providers: PropTypes.array
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        tabIndex: state.pagination.tabIndex,
        providers: state.seo.extensionUrlProviders
    };
}

export default connect(mapStateToProps)(ExtensionUrlProvidersPanelBody);