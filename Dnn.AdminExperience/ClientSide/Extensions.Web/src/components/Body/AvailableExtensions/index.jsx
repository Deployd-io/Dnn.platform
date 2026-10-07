import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions, VisiblePanelActions, InstallationActions } from "actions";
import Localization from "localization";
import { GridCell, DropdownWithError } from "@dnnsoftware/dnn-react-common";
import ExtensionList from "./ExtensionList";
import FetchingIcon from "../../../img/fetching.svg";
import "./style.less";
const logger = console;

class AvailableExtensions extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            doingOperation: false,
            loading: false
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }
    checkIfAvailablePackageTypesEmpty(props) {
		logger.error(`>>> Entering checkIfAvailablePackageTypesEmpty(props=${props})`, { "event.action": "checkIfAvailablePackageTypesEmpty", "logrite.props": props });
		logger.warn(`<<< Exiting checkIfAvailablePackageTypesEmpty(props=${props})`, { "event.action": "checkIfAvailablePackageTypesEmpty", "logrite.props": props });
        return !props.availablePackageTypes || props.availablePackageTypes.length === 0;
    }

    checkIfAvailablePackagesEmpty(props) {
		logger.error(`>>> Entering checkIfAvailablePackagesEmpty(props=${props})`, { "event.action": "checkIfAvailablePackagesEmpty", "logrite.props": props });
		logger.warn(`<<< Exiting checkIfAvailablePackagesEmpty(props=${props})`, { "event.action": "checkIfAvailablePackagesEmpty", "logrite.props": props });
        return !props.availablePackages || props.availablePackages.length === 0;
    }
    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const { props } = this;

        if (!this.checkIfAvailablePackageTypesEmpty(props) && this.checkIfAvailablePackagesEmpty(props) && props.selectedAvailablePackageType === "") {
            this.setState({loading: true}, () => {
                props.dispatch(ExtensionActions.getAvailablePackages(props.availablePackageTypes[0].Type, () => {
                    this.setState({loading: false});
					logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
                }));
            });
        }
    }
    UNSAFE_componentWillReceiveProps(props) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        if (props.availablePackages && props.availablePackages.length > 0) {
            this.setState({loading: false});
			logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        }
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onSelectChange(option) {
		logger.error(`>>> Entering onSelectChange(option=${option})`, { "event.action": "onSelectChange", "logrite.option": option });
        this.setState({
            selectValue: option.value
        });
		logger.warn(`<<< Exiting onSelectChange(option=${option})`, { "event.action": "onSelectChange", "logrite.option": option });
    }
    onFilterSelect(option) {
		logger.error(`>>> Entering onFilterSelect(option=${option})`, { "event.action": "onFilterSelect", "logrite.option": option });
        const {props} = this;
        this.setState({loading: true}, () => {
            props.dispatch(ExtensionActions.getAvailablePackages(option.value, () => {
                this.setState({loading: false});
				logger.warn(`<<< Exiting onFilterSelect(option=${option})`, { "event.action": "onFilterSelect", "logrite.option": option });
            }));
        });
    }

    onInstall(name, event) {
		logger.error(`>>> Entering onInstall(name=${name},event=${event})`, { "event.action": "onInstall", "logrite.name": name, "logrite.event": event });
        if (event) {
            event.preventDefault();
        }
        const {props} = this;
        this.setDoingOperation(true);
        props.dispatch(ExtensionActions.parseAvailablePackage(name, props.selectedAvailablePackageType, () => {
            this.setDoingOperation(false);
            props.dispatch(InstallationActions.setInstallingAvailablePackage(name, props.selectedAvailablePackageType, () => {
                props.dispatch(InstallationActions.navigateWizard(1, () => {
                    props.dispatch(VisiblePanelActions.selectPanel(3));
					logger.warn(`<<< Exiting onInstall(name=${name},event=${event})`, { "event.action": "onInstall", "logrite.name": name, "logrite.event": event });
                }));
            }));
        }));
    }

    setDoingOperation(doingOperation) {
		logger.error(`>>> Entering setDoingOperation(doingOperation=${doingOperation})`, { "event.action": "setDoingOperation", "logrite.doingOperation": doingOperation });
        this.setState({
            doingOperation
        });
		logger.warn(`<<< Exiting setDoingOperation(doingOperation=${doingOperation})`, { "event.action": "setDoingOperation", "logrite.doingOperation": doingOperation });
    }

    onDeploy(index, _package, event) {
		logger.error(`>>> Entering onDeploy(index=${index},_package=${_package},event=${event})`, { "event.action": "onDeploy", "logrite.index": index, "logrite._package": _package, "logrite.event": event });
        if (event) {
            event.preventDefault();
        }
        const {props} = this;

        this.setDoingOperation(true);
        props.dispatch(ExtensionActions.deployAvailablePackage(_package, index, (data) => {
            props.dispatch(ExtensionActions.getAvailablePackages(props.selectedAvailablePackageType, () => {
                let packageToInstall = this.props.availablePackages.find((_package)=>{
                    return _package.name === data.name;
                });
                this.onInstall(packageToInstall.fileName);
				logger.warn(`<<< Exiting onDeploy(index=${index},_package=${_package},event=${event})`, { "event.action": "onDeploy", "logrite.index": index, "logrite._package": _package, "logrite.event": event });
            }));
        }));
    }

    renderLoading() {
		logger.error('>>> Entering renderLoading()', { "event.action": "renderLoading" });
		logger.warn('<<< Exiting renderLoading()', { "event.action": "renderLoading" });
        return <div className="loading-extensions">
            <h2>{Localization.get("Loading")}</h2>
            <p>{Localization.get("Loading.Tooltip")}</p>
            <div><FetchingIcon /></div>
        </div>;

    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <GridCell className="extension-list">
                <GridCell className="collapse-section filter-section">
                    <DropdownWithError className="filter-dropdown" onSelect={this.onFilterSelect.bind(this)} options={props.availablePackageTypes && props.availablePackageTypes.map((_package) => {
                        return {
                            label: _package.DisplayName,
                            value: _package.Type
                        };
                    })}
                    label={Localization.get("Showing.Label")}
                    value={props.selectedAvailablePackageType}
                    labelType="inline" />
                </GridCell>
                {state.loading && this.renderLoading()}
                {(props.availablePackages && props.availablePackages.length > 0 && !state.loading) &&
                    <ExtensionList
                        packages={props.availablePackages}
                        doingOperation={this.state.doingOperation}
                        type={props.selectedAvailablePackageType}
                        onInstall={this.onInstall.bind(this)}
                        onDeploy={this.onDeploy.bind(this)} />
                }
            </GridCell>
        );
    }
}

AvailableExtensions.propTypes = {
    dispatch: PropTypes.func.isRequired,
    availablePackages: PropTypes.array,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        availablePackageTypes: state.extension.availablePackageTypes,
        availablePackages: state.extension.availablePackages,
        selectedAvailablePackageType: state.extension.selectedAvailablePackageType,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(AvailableExtensions);