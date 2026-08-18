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
		logger.debug('>>> Entering constructor()');
        this.state = {
            usageFilter: undefined,
            portalId: -1
        };
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
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
				logger.debug(`UNSAFE_componentWillMount(): portalId → ${portalId}`);
            }
            return;
        }
        props.dispatch(ExtensionActions.GetPackageUsageFilter());
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`);
        let {state} = this;

        if (state.portalId === -1) {
            let portalId = props.usageFilter.find(p => p.IsCurrentPortal === true).PortalID;
            this.setState({
                usageFilter: props.usageFilter,
                portalId: portalId
            });
            props.dispatch(ExtensionActions.getPackageUsage(portalId, props.packageId));
			logger.debug(`UNSAFE_componentWillReceiveProps(props=${props}): portalId → ${portalId}`);
        }
		logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`);
    }

    onClose() {
		logger.debug('>>> Entering onClose()');
        const {props} = this;
        props.onClose();
		logger.debug('<<< Exiting onClose()');
    }

    getPortalOptions() {
		logger.debug('>>> Entering getPortalOptions()');
        const {props} = this;
        let options = [];
        if (props.usageFilter !== undefined) {
            options = props.usageFilter.map((portal) => {
                return {
                    label: portal.PortalName, value: portal.PortalID
                };
            });
        }
		logger.debug('<<< Exiting getPortalOptions()');
        return options;
    }

    onSelectPortal(event) {
		logger.debug(`>>> Entering onSelectPortal(event=${event})`);
        let {props} = this;

        this.setState({
            portalId: event.value
        });

        props.dispatch(ExtensionActions.getPackageUsage(event.value, props.packageId));
		logger.debug(`<<< Exiting onSelectPortal(event=${event})`);
    }

    getCurrentUsageFilter() {
		logger.debug('>>> Entering getCurrentUsageFilter()');
        let {props, state} = this;
        if (props.usageFilter && state.portalId !== -1) {
            return props.usageFilter.find(p => p.PortalID === state.portalId).PortalName;
        }
        else return null;
		logger.debug('<<< Exiting getCurrentUsageFilter()');
    }

    getUsageDetailSubject() {
		logger.debug('>>> Entering getUsageDetailSubject()');
        let {props} = this;
        if (props.tabUrls) {
            if (props.tabUrls.length > 0) {
                return resx.get("ModuleUsageDetail").replace("{0}", props.tabUrls.length).replace("{1}", this.getCurrentUsageFilter());
            }
            else return resx.get("NoModuleUsageDetail").replace("{0}", this.getCurrentUsageFilter());
        }
		logger.debug('<<< Exiting getUsageDetailSubject()');
    }

     
    renderUsageDetail() {
		logger.debug('>>> Entering renderUsageDetail()');
        const {props} = this;
        if (props.tabUrls) {
			logger.debug('<<< Exiting renderUsageDetail()');
            return props.tabUrls.map((item, i) => {
                return (
                    <div key={i} className="usage-detail-taburl"><Html html={item.TabUrl} /></div>
                );
            });
        }
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        usageFilter: state.extension.usageFilter,
        tabUrls: state.extension.tabUrls
    };
}

export default connect(mapStateToProps)(InUseModal);