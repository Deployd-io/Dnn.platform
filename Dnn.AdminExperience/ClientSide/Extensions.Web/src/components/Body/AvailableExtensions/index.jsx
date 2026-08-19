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
		logger.debug('>>> Entering constructor()');
        this.state = {
            doingOperation: false,
            loading: false
        };
		logger.debug('<<< Exiting constructor()');
    }
    checkIfAvailablePackageTypesEmpty(props) {
		logger.debug(`>>> Entering checkIfAvailablePackageTypesEmpty(props=${props})`);
		logger.debug(`<<< Exiting checkIfAvailablePackageTypesEmpty(props=${props})`);
        return !props.availablePackageTypes || props.availablePackageTypes.length === 0;
    }

    checkIfAvailablePackagesEmpty(props) {
		logger.debug(`>>> Entering checkIfAvailablePackagesEmpty(props=${props})`);
		logger.debug(`<<< Exiting checkIfAvailablePackagesEmpty(props=${props})`);
        return !props.availablePackages || props.availablePackages.length === 0;
    }
    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        const { props } = this;

        if (!this.checkIfAvailablePackageTypesEmpty(props) && this.checkIfAvailablePackagesEmpty(props) && props.selectedAvailablePackageType === "") {
            this.setState({loading: true}, () => {
                props.dispatch(ExtensionActions.getAvailablePackages(props.availablePackageTypes[0].Type, () => {
                    this.setState({loading: false});
					logger.debug('<<< Exiting UNSAFE_componentWillMount()');
                }));
            });
        }
    }
    UNSAFE_componentWillReceiveProps(props) {
		logger.debug(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`);
        if (props.availablePackages && props.availablePackages.length > 0) {
            this.setState({loading: false});
			logger.debug(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`);
        }
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onSelectChange(option) {
		logger.debug(`>>> Entering onSelectChange(option=${option})`);
        this.setState({
            selectValue: option.value
        });
		logger.debug(`<<< Exiting onSelectChange(option=${option})`);
    }
    onFilterSelect(option) {
		logger.debug(`>>> Entering onFilterSelect(option=${option})`);
        const {props} = this;
        this.setState({loading: true}, () => {
            props.dispatch(ExtensionActions.getAvailablePackages(option.value, () => {
                this.setState({loading: false});
				logger.debug(`<<< Exiting onFilterSelect(option=${option})`);
            }));
        });
    }

    onInstall(name, event) {
		logger.debug(`>>> Entering onInstall(name=${name},event=${event})`);
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
					logger.debug(`<<< Exiting onInstall(name=${name},event=${event})`);
                }));
            }));
        }));
    }

    setDoingOperation(doingOperation) {
		logger.debug(`>>> Entering setDoingOperation(doingOperation=${doingOperation})`);
        this.setState({
            doingOperation
        });
		logger.debug(`<<< Exiting setDoingOperation(doingOperation=${doingOperation})`);
    }

    onDeploy(index, _package, event) {
		logger.debug(`>>> Entering onDeploy(index=${index},_package=${_package},event=${event})`);
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
				logger.debug(`<<< Exiting onDeploy(index=${index},_package=${_package},event=${event})`);
            }));
        }));
    }

    renderLoading() {
		logger.debug('>>> Entering renderLoading()');
		logger.debug('<<< Exiting renderLoading()');
        return <div className="loading-extensions">
            <h2>{Localization.get("Loading")}</h2>
            <p>{Localization.get("Loading.Tooltip")}</p>
            <div><FetchingIcon /></div>
        </div>;

    }

    render() {
		logger.debug('>>> Entering render()');
        const {props, state} = this;
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        availablePackageTypes: state.extension.availablePackageTypes,
        availablePackages: state.extension.availablePackages,
        selectedAvailablePackageType: state.extension.selectedAvailablePackageType,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(AvailableExtensions);