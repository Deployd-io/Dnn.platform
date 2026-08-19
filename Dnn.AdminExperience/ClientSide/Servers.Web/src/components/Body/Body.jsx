import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    DnnTabs as Tabs,
    PersonaBarPageBody,
} from "@dnnsoftware/dnn-react-common";
import { pagination as PaginationActions } from "../../actions";
import "./style.less";
import Localization from "../../localization";
import ApplicationTab from "../Tabs/Application";
import UpgradesTab from "../Tabs/Upgrades";
import WebTab from "../Tabs/Web";
import DatabaseTab from "../Tabs/Database/Database";
import ServersTab from "../Tabs/Servers/Servers";
import SmtpServerTab from "../Tabs/SmtpServer";
import PerformanceTab from "../Tabs/Performance/Index";
import LogsTab from "../Tabs/Logs";
import application from "../../globals/application";
import utils from "../../utils";
const logger = console;

class Body extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.handleSelect = this.handleSelect.bind(this);
		logger.debug('<<< Exiting constructor()');
    }

    handleSelect(index) {
        const { props } = this;
        props.dispatch(PaginationActions.loadTab(index));
    }

    insertElementsInArray(
        array,
        elements,
        propertyNameHasIndex,
        propertyNameHasValue
    ) {
        for (let i = 0; i < elements.length; i++) {
            let index = this.getInteger(elements[i][propertyNameHasIndex]);

            if (index || index === 0) {
                index = Math.min(array.length, Math.max(0, index));
                array.splice(index, 0, elements[i][propertyNameHasValue]);
            }
        }
    }

    getInteger(value) {
		logger.debug(`>>> Entering getInteger(value=${value})`);
        if (value === 0) {
            return 0;
        }
        if (value) {
		logger.debug(`<<< Exiting getInteger(value=${value})`);
            return parseInt(value.toString());
        }
        return value;
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
        const isHost = utils.isHostUser();
		logger.debug(`render(): isHost → ${isHost}`);
        let registeredTabs = application.getRegisteredServerTabs();
		logger.debug(`render(): registeredTabs → ${registeredTabs}`);
        if (!isHost) {
            registeredTabs = registeredTabs.filter(function (tab) {
				logger.debug('>>> Entering filter()');
                return !tab.isHostOnlyVisible;
            });
        }

        const systemInfoTabHeaders = isHost
            ? [
                Localization.get("tabApplicationTitle"),
                Localization.get("tabWebTitle"),
                Localization.get("tabDatabaseTitle"),
                Localization.get("tabServersTitle"),
                Localization.get("tabUpgrades"),
            ]
            : [Localization.get("tabApplicationTitle")];
        const systemInfoTabBody = isHost
            ? [
                <ApplicationTab key="first" />,
                <WebTab key="second" />,
                <DatabaseTab key="third" />,
                <ServersTab key="fourth" />,
                <UpgradesTab key="fifth" applicationInfo={props.applicationInfo} />,
            ]
            : [<ApplicationTab key="first" />];
        const serverSettingsTabHeaders = isHost
            ? [
                Localization.get("tabSmtpServerTitle"),
                Localization.get("tabPerformanceTitle"),
                Localization.get("tabLogsTitle"),
            ]
            : [Localization.get("tabSmtpServerTitle")];
        const serverSettingsTabBody = isHost
            ? [
                <SmtpServerTab key="first" />,
                <PerformanceTab key="second" />,
                <LogsTab key="third" />,
            ]
            : [<SmtpServerTab key="first" />];
        const mainTabHeaders = [
            Localization.get("tabSystemInfoTitle"),
            Localization.get("tabServerSettingsTitle"),
        ];
        const mainTabBody = [
            <Tabs tabHeaders={systemInfoTabHeaders} type="secondary" key="first">
                {systemInfoTabBody}
            </Tabs>,
            <Tabs tabHeaders={serverSettingsTabHeaders} type="secondary" key="second">
                {serverSettingsTabBody}
            </Tabs>,
        ];

        this.insertElementsInArray(
            mainTabHeaders,
            registeredTabs.filter(function (tab) {
                return !tab.parentIndex && tab.parentIndex !== 0;
            }),
            "order",
            "title"
        );
        this.insertElementsInArray(
            mainTabBody,
            registeredTabs.filter(function (tab) {
                return !tab.parentIndex && tab.parentIndex !== 0;
            }),
            "order",
            "component"
        );

        this.insertElementsInArray(
            systemInfoTabHeaders,
            registeredTabs.filter(function (tab) {
                return tab.parentIndex === 0;
            }),
            "order",
            "title"
        );
        this.insertElementsInArray(
            systemInfoTabBody,
            registeredTabs.filter(function (tab) {
                return tab.parentIndex === 0;
            }),
            "order",
            "component"
        );

        this.insertElementsInArray(
            serverSettingsTabHeaders,
            registeredTabs.filter(function (tab) {
                return tab.parentIndex === 1;
            }),
            "order",
            "title"
        );
        this.insertElementsInArray(
            serverSettingsTabBody,
            registeredTabs.filter(function (tab) {
		logger.debug('<<< Exiting render()');
                return tab.parentIndex === 1;
            }),
            "order",
            "component"
        );

        return (
            <PersonaBarPageBody>
                <Tabs
                    className="dnn-servers-tab-panel"
                    onSelect={this.handleSelect}
                    selectedIndex={props.tabIndex}
                    tabHeaders={mainTabHeaders}
                >
                    {mainTabBody}
                </Tabs>
            </PersonaBarPageBody>
        );
    }
}

Body.propTypes = {
    applicationInfo: PropTypes.object.isRequired,
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        applicationInfo: state.applicationTab.applicationInfo,
        tabIndex: state.pagination.tabIndex,
    };
}

export default connect(mapStateToProps)(Body);
