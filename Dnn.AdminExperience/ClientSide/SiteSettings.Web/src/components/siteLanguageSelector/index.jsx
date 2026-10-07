import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteInfo as SiteInfoActions,
    search as SearchActions
} from "../../actions";
import { InputGroup, Dropdown, Flag, Label } from "@dnnsoftware/dnn-react-common";
import resx from "../../resources";
import styles from "./style.module.less";
import util from "../../utils";
const logger = console;

let isHost = false;

class SiteLanguageSelector extends Component {
    constructor() {
        super();
        this.state = {
            cultureCode: undefined,
            portalId: undefined
        };
        isHost = util.settings.isHost;
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    componentDidMount() {
        const {state, props} = this;
        this.setState({
            portalId: props.portalId,
            cultureCode: props.cultureCode
        }, () => {
            if (isHost) {
                props.dispatch(SiteInfoActions.getPortals(() => {
                    props.dispatch(SearchActions.getCultureList(props.portalId));
                }));
            }
            else {
                props.dispatch(SearchActions.getCultureList(state.portalId));
            }
        });

        document.addEventListener("reloadPortalList", (event) => {
            props.dispatch(SiteInfoActions.getPortalSettings(event.portalId, event.cultureCode));
        }, false);
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props, state } = this;
        if (props.portals !== undefined) {
            props.portals.map((item) => {
                if (item.IsCurrentPortal && state.portalId === undefined) {
                    this.setState({
                        portalId: item.PortalID
                    });
                }
            });
        }
        if (props.portalId !== undefined && props.portalId !== prevProps.portalId) {
            this.onSiteChange({ value: props.portalId });         
        }
        if (props.cultureCode !== state.cultureCode) {
            this.onLanguageChange({ value: props.cultureCode });
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }
    }

    onSiteChange(event) {
		logger.error(`>>> Entering onSiteChange(event=${event})`, { "event.action": "onSiteChange", "logrite.event": event });
        let {state, props} = this;

        if (event.value === state.portalId) {
            return;
        }

        props.dispatch(SearchActions.getCultureList(event.value, () => {
            let culture = this.validateCultureCode();
            if (state.cultureCode !== culture) {
                this.setState({
                    portalId: event.value,
                    cultureCode: culture
                });
                this.triggerEvent("portalIdCultureCodeChanged",
                    {
                        portalId: event.value,
                        cultureCode: culture,
                        referrer: props.referrer,
                        referrerText: props.referrerText,
                        backToReferrerFunc: props.backToReferrerFunc
                    });
            }
            else {
                this.setState({
                    portalId: event.value
                });
                this.triggerEvent("portalIdChanged",
                    {
                        portalId: event.value,
                        referrer: props.referrer,
                        referrerText: props.referrerText,
                        backToReferrerFunc: props.backToReferrerFunc
                    });
            }
        }));
		logger.warn(`<<< Exiting onSiteChange(event=${event})`, { "event.action": "onSiteChange", "logrite.event": event });
    }

    onLanguageChange(event) {
		logger.error(`>>> Entering onLanguageChange(event=${event})`, { "event.action": "onLanguageChange", "logrite.event": event });
        let {props} = this;
        this.setState({
            cultureCode: event.value
        });
        this.triggerEvent("cultureCodeChanged",
            {
                cultureCode: event.value,
                referrer: props.referrer,
                referrerText: props.referrerText,
                backToReferrerFunc: props.backToReferrerFunc
            });
			logger.warn(`<<< Exiting onLanguageChange(event=${event})`, { "event.action": "onLanguageChange", "logrite.event": event });
    }

    getSiteOptions() {
        const {props} = this;
        let options = [];
        if (props.portals !== undefined) {
            options = props.portals.map((item) => {
                return {
                    label: item.PortalName, value: item.PortalID
                };
            });
        }
        return options;
    }

    getLanguageOptions() {
        const {props} = this;
        let options = [];
        if (props.languages !== undefined) {
            options = props.languages.map((item, i) => {
                return (
                    <div className={"language-flag" + (item.Code === this.state.cultureCode ? " selected": "")} onClick={this.onLanguageChange.bind(this, { value: item.Code })} key={i}>
                        <Flag culture={item.Code} title={item.Name} />
                    </div>
                );
            });
        }
        return options;
    }

    validateCultureCode() {
        const {state, props} = this;

        if (props.languages.filter(language => {
            return language.Code === state.cultureCode;
        }).length > 0) {
            return state.cultureCode;
        }
        else {
            let culture = (props.languages.filter(language => {
                return language.IsDefault === true;
            }))[0].Code;
            this.setState({
                cultureCode: culture
            });
            return culture;
        }
    }

    triggerEvent(name, parameter) {
		logger.error(`>>> Entering triggerEvent(name=${name},parameter=${parameter})`, { "event.action": "triggerEvent", "logrite.name": name, "logrite.parameter": parameter });
        let event = document.createEvent("Event");
        event.initEvent(name, true, true);
        event = Object.assign(event, parameter);
        document.dispatchEvent(event);
		logger.error(`triggerEvent(name=${name},parameter=${parameter}): event → ${event}`, { "event.action": "triggerEvent", "logrite.name": name, "logrite.parameter": parameter });
    }

    renderSitesList() {
        const {props, state} = this;
        if (props.portals) {
            return (<div className="site-selection">
                <InputGroup>
                    <Label
                        labelType="inline"
                        label={resx.get("SiteSelectionLabel") + ":"}
                    />
                    <Dropdown
                        options={this.getSiteOptions()}
                        value={state.portalId}
                        onSelect={this.onSiteChange.bind(this)}
                    />
                </InputGroup>
            </div>
            );
        }
        else return (<div />);
    }

    renderLanguagesList() {
        const {props} = this;
        if (props.languages) {
            return (<div className="language-selection">
                {this.getLanguageOptions()}
            </div>
            );
        }
        else return (<div />);
    }

     
    render() {
        return <div className={styles.sitesHeader}>
            {(this.props.languages && this.props.languages.length > 1) && this.renderLanguagesList()}
            {(this.props.portals && this.props.portals.length > 1 && isHost) && this.renderSitesList()}
        </div>;
    }
}

SiteLanguageSelector.propTypes = {
    dispatch: PropTypes.func.isRequired,
    portals: PropTypes.array,
    languages: PropTypes.array,
    cultureCode: PropTypes.string,
    portalId: PropTypes.number,
    referrer: PropTypes.string,
    referrerText: PropTypes.string,
    backToReferrerFunc: PropTypes.func
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        portals: state.siteInfo.portals,
        languages: state.search.cultures
    };
}

export default connect(mapStateToProps)(SiteLanguageSelector);