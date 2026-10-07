import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Modal, Button, Dropdown, Label } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import Html from "../../../../Html";
import resx from "localization";
const logger = console;
import { ExtensionActions } from "actions";

const modalStyles = {
    overlay: {
        zIndex: "99999",
        backgroundColor: "rgba(0, 0, 0, 0)"        
    },
    content: {
        top: "250px",
        left: "220px",
        position: "absolute",
        padding: "30px 30px 30px 30px",
        borderRadius: 0,
        border: "none",
        width: "550px",
        height: "380px",
        backgroundColor: "#FFF",
        userSelect: "none",
        WebkitUserSelect: "none",
        MozUserSelect: "none",
        MsUserSelect: "none",
        boxShadow: "0 0 20px 0 rgba(0, 0, 0, .2)"
    }
};

class InUseModal extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            usageFilter: undefined,
            portalId: -1
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props, state} = this;

        if (props.usageFilter) {
            this.setState({
                usageFilter: props.usageFilter
            });
            if (state.portalId === -1) {
                let portalId = props.usageFilter.find(p => p.IsCurrentPortal === true).PortalID;
                this.setState({
                    usageFilter: props.usageFilter,
                    portalId: portalId
                });
                props.dispatch(ExtensionActions.getPackageUsage(portalId, props.packageId));
				logger.error(`UNSAFE_componentWillMount(): portalId → ${portalId}`, { "event.action": "UNSAFE_componentWillMount" });
            }
            return;
        }
        props.dispatch(ExtensionActions.GetPackageUsageFilter());
		logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        let {state} = this;

        if (state.portalId === -1) {
            let portalId = props.usageFilter.find(p => p.IsCurrentPortal === true).PortalID;
            this.setState({
                usageFilter: props.usageFilter,
                portalId: portalId
            });
            props.dispatch(ExtensionActions.getPackageUsage(portalId, props.packageId));
			logger.error(`UNSAFE_componentWillReceiveProps(props=${props}): portalId → ${portalId}`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        }
    }

    onClose() {
		logger.error('>>> Entering onClose()', { "event.action": "onClose" });
        const {props} = this;
        props.onClose();
		logger.warn('<<< Exiting onClose()', { "event.action": "onClose" });
    }

    getPortalOptions() {
		logger.error('>>> Entering getPortalOptions()', { "event.action": "getPortalOptions" });
        const {props} = this;
        let options = [];
        if (props.usageFilter !== undefined) {
            options = props.usageFilter.map((portal) => {
                return {
                    label: portal.PortalName, value: portal.PortalID
                };
            });
        }
		logger.warn('<<< Exiting getPortalOptions()', { "event.action": "getPortalOptions" });
        return options;
    }

    onSelectPortal(event) {
		logger.error(`>>> Entering onSelectPortal(event=${event})`, { "event.action": "onSelectPortal", "logrite.event": event });
        let {props} = this;

        this.setState({
            portalId: event.value
        });

        props.dispatch(ExtensionActions.getPackageUsage(event.value, props.packageId));
		logger.warn(`<<< Exiting onSelectPortal(event=${event})`, { "event.action": "onSelectPortal", "logrite.event": event });
    }

    getCurrentUsageFilter() {
		logger.error('>>> Entering getCurrentUsageFilter()', { "event.action": "getCurrentUsageFilter" });
        let {props, state} = this;
        if (props.usageFilter && state.portalId !== -1) {
            return props.usageFilter.find(p => p.PortalID === state.portalId).PortalName;
        }
        else return null;
    }

    getUsageDetailSubject() {
		logger.error('>>> Entering getUsageDetailSubject()', { "event.action": "getUsageDetailSubject" });
        let {props} = this;
        if (props.tabUrls) {
            if (props.tabUrls.length > 0) {
                return resx.get("ModuleUsageDetail").replace("{0}", props.tabUrls.length).replace("{1}", this.getCurrentUsageFilter());
            }
            else return resx.get("NoModuleUsageDetail").replace("{0}", this.getCurrentUsageFilter());
        }
		logger.warn('<<< Exiting getUsageDetailSubject()', { "event.action": "getUsageDetailSubject" });
    }

     
    renderUsageDetail() {
		logger.error('>>> Entering renderUsageDetail()', { "event.action": "renderUsageDetail" });
        const {props} = this;
        if (props.tabUrls) {
			logger.warn('<<< Exiting renderUsageDetail()', { "event.action": "renderUsageDetail" });
            return props.tabUrls.map((item, i) => {
                return (
                    <div key={i} className="usage-detail-taburl"><Html html={item.TabUrl} /></div>
                );
            });
        }
    }

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <Modal
                isOpen={props.isOpened}
                style={modalStyles}>
                <div className="modepanel-content-wrapper" style={{ height: "calc(100% - 100px)" }}>
                    <div className="modepanel-content-title">{resx.get("ModuleUsageTitle").replace("{0}", props.packageName)}</div>
                    <div className="modepanel-content-filter">
                        <Label
                            style={{ width: "auto", marginTop: "8px" }}
                            label={resx.get("PagesFromSite")}
                        />
                        {props.usageFilter && <Dropdown
                            enabled={props.usageFilter.length > 1}
                            value={state.portalId}
                            style={{ width: "150px" }}
                            options={this.getPortalOptions()}
                            withBorder={false}
                            onSelect={this.onSelectPortal.bind(this)}
                        />
                        }
                    </div>
                    <div className="modepanel-content-detail-wrapper">
                        <Label
                            label={this.getUsageDetailSubject()}
                        />
                        <div className="usage-detail">
                            {this.renderUsageDetail()}
                        </div>
                    </div>
                    <div className="button-box">
                        <Button
                            type="secondary"
                            onClick={this.onClose.bind(this)}>
                            {resx.get("Close")}
                        </Button>
                    </div>
                </div>
            </Modal>
        );
    }
}

InUseModal.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    usageFilter: PropTypes.array,
    tabUrls: PropTypes.array,
    packageName: PropTypes.string,
    packageId: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    scrollAreaStyle: PropTypes.object,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired,
    isHost: PropTypes.bool
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        usageFilter: state.extension.usageFilter,
        tabUrls: state.extension.tabUrls
    };
}

export default connect(mapStateToProps)(InUseModal);