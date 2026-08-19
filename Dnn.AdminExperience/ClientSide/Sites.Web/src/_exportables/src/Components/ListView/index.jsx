import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import PortalListItem from "./PortalListItem";
import Localization from "localization";
import { CommonVisiblePanelActions, CommonPortalListActions } from "actions";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import utilities from "utils";
import * as dayjs from "dayjs";
import styles from "./style.module.less";
import localizedFormat from "dayjs/plugin/localizedFormat";
import relativeTime from "dayjs/plugin/relativeTime";
const logger = console;

class ListView extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }
    onDelete(portal, index) {
		logger.debug(`>>> Entering onDelete(portal=${portal},index=${index})`);
        const { props } = this;
        utilities.confirm(Localization.get("deletePortal").replace("{0}", portal.PortalName),
            Localization.get("ConfirmPortalDelete"),
            Localization.get("CancelPortalDelete"),
            () => {
                props.dispatch(CommonPortalListActions.deletePortal(portal.PortalID, index));
            });
			logger.debug(`<<< Exiting onDelete(portal=${portal},index=${index})`);
    }
    backToSites() {
		logger.debug('>>> Entering backToSites()');
        utilities.loadPanel(this.props.sitesModule, {});
		logger.debug('<<< Exiting backToSites()');
    }
    onSetting(portal /*, index*/) {
		logger.debug('>>> Entering onSetting()');
        let event = document.createEvent("Event");
        event.initEvent("portalIdChanged", true, true);
		logger.debug(`onSetting(): event → ${event}`);

        let event2 = document.createEvent("Event");
		logger.debug(`onSetting(): event2 → ${event2}`);
        event2.initEvent("reloadPortalList", true, true);

        let settings = {
            portalId: portal.PortalID,
            referrer: this.props.sitesModule,
            referrerText: Localization.get("BackToSites"),
            backToReferrerFunc: this.backToSites.bind(this),
            isHost: true
        };

        event = Object.assign(event, settings);
        
        let portalsettings = {
            portalId: portal.PortalID,
            cultureCode: this.props.culture
        };
        
        event2 = Object.assign(event2, portalsettings);

        utilities.loadPanel(this.props.siteSettingModule, {
            settings
        });

        document.dispatchEvent(event);
        document.dispatchEvent(event2);
		logger.debug('<<< Exiting onSetting()');
    }
    onPreview(portal /*, index*/) {
		logger.debug('>>> Entering onPreview()');
        if (portal.PortalAliases && portal.PortalAliases.length > 0) {
            window.open(portal.PortalAliases[0].link);
			logger.debug('<<< Exiting onPreview()');
        }
    }
    navigateMap(page) {
		logger.debug(`>>> Entering navigateMap(page=${page})`);
        const { props } = this;
        props.dispatch(CommonVisiblePanelActions.selectPanel(page));
		logger.debug(`<<< Exiting navigateMap(page=${page})`);
    }

    onImportExport(portal, type) {
		logger.debug(`>>> Entering onImportExport(portal=${portal},type=${type})`);
        let event = document.createEvent("Event");

		logger.debug(`onImportExport(portal=${portal},type=${type}): event → ${event}`);
        event.initEvent("siteImportExport", true, true);

        let settings = {
            importExportJob: {
                portalId: portal.PortalID,
                portalName: portal.PortalName,
                jobType: type
            },
            referrer: this.props.sitesModule,
            referrerText: Localization.get("BackToSites"),
            backToReferrerFunc: this.backToSites.bind(this)            
        };

        event = Object.assign(event, settings);

        utilities.loadPanel(this.props.siteImportExportModule, {
            settings
        });

        document.dispatchEvent(event);
		logger.debug(`<<< Exiting onImportExport(portal=${portal},type=${type})`);
    }

    getPortalButtons(portal, index) {
		logger.debug(`>>> Entering getPortalButtons(portal=${portal},index=${index})`);
        let portalButtons = [
            {
                icon: SvgIcons.PreviewIcon,
                onClick: this.onPreview.bind(this, portal, index),
                title: Localization.get("ViewSite")
            },
            {
                icon: SvgIcons.SettingsIcon,
                onClick: this.onSetting.bind(this, portal, index),
                title: Localization.get("SiteSettings")
            },
            {
                icon: SvgIcons.UploadIcon,
                onClick: this.onImportExport.bind(this, portal, "Export", index),
                title: Localization.get("SiteExport")
            },
            {
                icon: SvgIcons.DownloadIcon,
                onClick: this.onImportExport.bind(this, portal, "Import", index),
                title: Localization.get("SiteImport")
            },
            {
                icon: SvgIcons.TreeCopy,
                onClick: this.props.onExportPortal.bind(this, portal),
                title: Localization.get("ExportTemplate")
            }
        ];
        if (portal.allowDelete) {
            portalButtons = portalButtons.concat([
                {
                    icon: SvgIcons.TrashIcon,
                    onClick: this.onDelete.bind(this, portal, index),
                    title: Localization.get("DeleteSite")
                }
            ]);
        }

         
        let i = 0;
		logger.debug(`<<< Exiting getPortalButtons(portal=${portal},index=${index})`);
        return portalButtons.map((_button) => {
            let ButtonIcon = _button.icon;
            let element = <div key={`portalButton_${i}`} title={_button.title} onClick={_button.onClick}><ButtonIcon /></div>;
            i++;
            return element;
        });
    }
    getPortalMapping(portal) {
		logger.debug(`>>> Entering getPortalMapping(portal=${portal})`);
        dayjs.extend(localizedFormat);
        dayjs.extend(relativeTime);
         
        require("dayjs/locale/" + this.props.culture.substring(0,2));

		logger.debug(`<<< Exiting getPortalMapping(portal=${portal})`);
        return [
            {
                label: Localization.get("SiteDetails_SiteID"),
                value: portal.PortalID
            },
            {
                label: Localization.get("SiteDetails_Users"),
                value: portal.Users
            },
            {
                label: Localization.get("SiteDetails_Updated"),
                value: dayjs(portal.LastModifiedOnDate).locale(this.props.culture.substring(0,2)).fromNow()
            },
            {
                label: Localization.get("SiteDetails_Pages"),
                value: portal.Pages
            }
        ].concat((this.props.getPortalMapping && this.props.getPortalMapping(portal)) || []);
    }
    getDetailList() {
		logger.debug('>>> Entering getDetailList()');
        const { props } = this;
		logger.debug('<<< Exiting getDetailList()');
        return props.portals.map((portal, index) => {
            return (portal.PortalAliases && <PortalListItem
                key={"portal-" + portal.PortalID}
                portal={portal}
                portalButtons={this.getPortalButtons(portal, index)}
                portalStatisticInfo={this.getPortalMapping(portal)} />);
        });
    }
    cancelExport(event) {
		logger.debug(`>>> Entering cancelExport(event=${event})`);
        if (event !== undefined)
            event.preventDefault();
        this.setState({
            portalBeingExported: {}
        });
        this.navigateMap(0);
		logger.debug(`<<< Exiting cancelExport(event=${event})`);
    }
    render() {
		logger.debug('>>> Entering render()');
        const portalList = this.getDetailList(), { props } = this;

		logger.debug(`render(): portalList → ${portalList}`);
		logger.debug('<<< Exiting render()');
        return (
            <GridCell className={styles.siteList}>
                <GridCell className={"portal-list-container " + props.className}>
                    {portalList}
                </GridCell>
            </GridCell>
        );
    }
}

ListView.propTypes = {
    dispatch: PropTypes.func.isRequired,
    siteSettingModule: PropTypes.string,
    getPortalMapping: PropTypes.func.isRequired,
    portals: PropTypes.array,
    totalCount: PropTypes.number,
    onEditSite: PropTypes.func,
    onExportPortal: PropTypes.func,
    onSettingClick: PropTypes.func,
    onDeletePortal: PropTypes.func,
    onPreviewPortal: PropTypes.func,
    sitesModule: PropTypes.string,
    culture: PropTypes.string,
    siteImportExportModule: PropTypes.string,
    className: PropTypes.string
};

ListView.defaultProps = {
    siteSettingModule: "Dnn.SiteSettings",
    sitesModule: "Dnn.Sites",
    siteImportExportModule: "Dnn.SiteImportExport",
    culture: "en-US",
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        portals: state.portal.portals,
        totalCount: state.portal.totalCount
    };
}
export default connect(mapStateToProps)(ListView);
