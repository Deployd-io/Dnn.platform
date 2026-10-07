import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../../actions";
import SiteAliasRow from "./siteAliasRow";
import SiteAliasEditor from "./siteAliasEditor";
import { Collapsible } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import util from "../../../utils";
import resx from "../../../resources";
const logger = console;

class SiteAliasesPanel extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            openId: "",
            tableFields: []
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    loadData() {
		logger.error('>>> Entering loadData()', { "event.action": "loadData" });
        const {props} = this;
        props.dispatch(SiteBehaviorActions.getSiteAliases(props.portalId));
		logger.warn('<<< Exiting loadData()', { "event.action": "loadData" });
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        this.loadData();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const {props} = this;
        if (props !== prevProps) {
            let tableFields = [];
            if (tableFields.length === 0) {
                tableFields.push({ "name": resx.get("Alias.Header"), "id": "Alias" });
                tableFields.push({ "name": resx.get("Browser.Header"), "id": "Browser" });
                tableFields.push({ "name": resx.get("Theme.Header"), "id": "Theme" });
                if (props.siteAliases !== undefined && props.siteAliases. Languages.length > 1) {
                    tableFields.push({ "name": resx.get("Language.Header"), "id": "Language" });
                }
                tableFields.push({ "name": resx.get("Primary.Header"), "id": "Primary" });
            }
            this.setState({tableFields});

            if (!props.siteAliases ||
                props.portalId !== undefined && props.siteAliases.PortalId !== props.portalId) {
                this.loadData();
				logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            }
        }
    }

    renderHeader() {
		logger.error('>>> Entering renderHeader()', { "event.action": "renderHeader" });
        let tableHeaders = this.state.tableFields.map((field) => {
            let className = "alias-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
		logger.warn('<<< Exiting renderHeader()', { "event.action": "renderHeader" });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        this.setState({
            openId: id
        });
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
    }

    onUpdateSiteAlias(aliasDetail) {
        const {props} = this;
        if (aliasDetail.PortalAliasID) {
            props.dispatch(SiteBehaviorActions.updateSiteAlias(aliasDetail, () => {
                util.utilities.notify(resx.get("SiteAliasUpdateSuccess"));
                this.collapse();
                props.dispatch(SiteBehaviorActions.getSiteAliases(props.portalId));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            const alias = Object.assign({}, aliasDetail);
            alias["PortalId"] = props.portalId;
            props.dispatch(SiteBehaviorActions.addSiteAlias(alias, () => {
                util.utilities.notify(resx.get("SiteAliasCreateSuccess"));
                this.collapse();
                props.dispatch(SiteBehaviorActions.getSiteAliases(props.portalId));
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteSiteAlias(aliasId) {
        const {props} = this;
        util.utilities.confirm(resx.get("SiteAliasDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            const siteAliases = Object.assign({}, props.siteAliases);
            siteAliases.PortalAliases = siteAliases.PortalAliases.filter((item) => item.PortalAliasID !== aliasId);
            props.dispatch(SiteBehaviorActions.deleteSiteAlias(aliasId, siteAliases, () => {
                util.utilities.notify(resx.get("SiteAliasDeleteSuccess"));
                this.collapse();
            }, () => {
                util.utilities.notify(resx.get("SiteAliasDeleteError"));
            }));
        });
    }

     
    renderedSiteAliases() {
		logger.error('>>> Entering renderedSiteAliases()', { "event.action": "renderedSiteAliases" });
        let i = 0;
        if (this.props.siteAliases) {
			logger.warn('<<< Exiting renderedSiteAliases()', { "event.action": "renderedSiteAliases" });
            return this.props.siteAliases.PortalAliases.map((item, index) => {
                let id = "row-" + i++;
                return (
                    <SiteAliasRow
                        aliasId={item.PortalAliasID}
                        alias={item.HTTPAlias}
                        browser={item.BrowserType}
                        skin={item.Skin}
                        language={item.CultureCode}
                        isPrimary={!!item.IsPrimary}
                        deletable={item.Deletable}
                        editable={item.Editable}
                        showLanguageColumn={this.props.siteAliases.Languages && this.props.siteAliases.Languages.length > 1}
                        index={index}
                        key={"aliasItem-" + index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        OpenCollapse={this.toggle.bind(this)}
                        Collapse={this.collapse.bind(this)}
                        onDelete={this.onDeleteSiteAlias.bind(this, item.PortalAliasID)}
                        id={id}>
                        <SiteAliasEditor
                            aliasId={item.PortalAliasID}
                            Collapse={this.collapse.bind(this)}
                            onUpdate={this.onUpdateSiteAlias.bind(this)}
                            id={id}
                            openId={this.state.openId} />
                    </SiteAliasRow>
                );
            });
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let opened = (this.state.openId === "add");
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <div className="alias-items">
                    <div className="AddItemRow">
                        <div className="sectionTitle">{resx.get("SiteAliases")}</div>
                        <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                            <div className="add-icon"><SvgIcons.AddIcon />
                            </div> {resx.get("cmdAddAlias")}
                        </div>
                    </div>
                    <div className="alias-items-grid">
                        {this.renderHeader()}
                        <Collapsible isOpened={opened} className="alias-items-collapsible">
                            <SiteAliasRow
                                alias={"-"}
                                browser={"-"}
                                skin={"-"}
                                language={"-"}
                                isPrimary={false}
                                deletable={false}
                                editable={false}
                                showLanguageColumn={this.props.siteAliases && this.props.siteAliases.Languages && this.props.siteAliases.Languages.length > 1}
                                index={"add"}
                                key={"aliasItem-add"}
                                closeOnClick={true}
                                openId={this.state.openId}
                                OpenCollapse={this.toggle.bind(this)}
                                Collapse={this.collapse.bind(this)}
                                onDelete={this.onDeleteSiteAlias.bind(this)}
                                id={"add"}>
                                <SiteAliasEditor
                                    Collapse={this.collapse.bind(this)}
                                    onUpdate={this.onUpdateSiteAlias.bind(this)}
                                    id={"add"}
                                    openId={this.state.openId} />
                            </SiteAliasRow>
                        </Collapsible>
                        {this.renderedSiteAliases()}
                    </div>
                </div>
            </div>
        );
    }
}

SiteAliasesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    siteAliases: PropTypes.object,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        siteAliases: state.siteBehavior.siteAliases,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(SiteAliasesPanel);