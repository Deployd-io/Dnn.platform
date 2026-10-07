import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions, PaginationActions, VisiblePanelActions } from "actions";
import Localization from "localization";
import ExtensionList from "./ExtensionList";
import { GridCell, DropdownWithError } from "@dnnsoftware/dnn-react-common";
import FetchingIcon from "../../../img/fetching.svg";
import "./style.less";
const logger = console;

class InstalledExtensions extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {
            loading: false
        };
    }

    checkIfPackageTypesEmpty(props) {
		logger.error(`>>> Entering checkIfPackageTypesEmpty(props=${props})`, { "event.action": "checkIfPackageTypesEmpty", "logrite.props": props });
		logger.warn(`<<< Exiting checkIfPackageTypesEmpty(props=${props})`, { "event.action": "checkIfPackageTypesEmpty", "logrite.props": props });
        return !props.installedPackageTypes || props.installedPackageTypes.length === 0;
    }

    checkIfInstalledPackagesEmpty(props) {
		logger.error(`>>> Entering checkIfInstalledPackagesEmpty(props=${props})`, { "event.action": "checkIfInstalledPackagesEmpty", "logrite.props": props });
		logger.warn(`<<< Exiting checkIfInstalledPackagesEmpty(props=${props})`, { "event.action": "checkIfInstalledPackagesEmpty", "logrite.props": props });
        return !props.installedPackages || props.installedPackages.length === 0;
    }

    UNSAFE_componentWillMount() {
		logger.error('>>> Entering UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        const {props} = this;
        if (this.checkIfPackageTypesEmpty(props)) {
            props.dispatch(ExtensionActions.getPackageTypes());
			logger.warn('<<< Exiting UNSAFE_componentWillMount()', { "event.action": "UNSAFE_componentWillMount" });
        }
    }

    UNSAFE_componentWillReceiveProps(props) {
		logger.error(`>>> Entering UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
        if (!this.checkIfPackageTypesEmpty(props) && this.checkIfInstalledPackagesEmpty(props) && props.selectedInstalledPackageType === "") {
            this.setState({loading: true}, () => {
                props.dispatch(ExtensionActions.getInstalledPackages(props.installedPackageTypes[0].Type, ()=> {
                    this.setState({loading: false});
					logger.warn(`<<< Exiting UNSAFE_componentWillReceiveProps(props=${props})`, { "event.action": "UNSAFE_componentWillReceiveProps", "logrite.props": props });
                }));
            });
        }
    }

    handleSelect(index/*, last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onFilterSelect(option) {
		logger.error(`>>> Entering onFilterSelect(option=${option})`, { "event.action": "onFilterSelect", "logrite.option": option });
        const {props} = this;
        this.setState({loading: true}, () => {
            props.dispatch(ExtensionActions.getInstalledPackages(option.value, () => {
                this.setState({loading: false});
				logger.warn(`<<< Exiting onFilterSelect(option=${option})`, { "event.action": "onFilterSelect", "logrite.option": option });
            }));
        });
    }

    onDelete(_package, index) {
		logger.error(`>>> Entering onDelete(_package=${_package},index=${index})`, { "event.action": "onDelete", "logrite._package": _package, "logrite.index": index });
        const {props} = this;
        props.dispatch(ExtensionActions.setPackageBeingDeleted(Object.assign({ packageType: props.selectedInstalledPackageType }, _package), index, () => {
            props.dispatch(VisiblePanelActions.selectPanel(6));
			logger.warn(`<<< Exiting onDelete(_package=${_package},index=${index})`, { "event.action": "onDelete", "logrite._package": _package, "logrite.index": index });
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
                    <DropdownWithError className="filter-dropdown" onSelect={this.onFilterSelect.bind(this)} options={props.installedPackageTypes && props.installedPackageTypes.map((_package) => {
                        return {
                            label: _package.DisplayName,
                            value: _package.Type
                        };
                    })}
                    label={Localization.get("Showing.Label")}
                    value={props.selectedInstalledPackageType}
                    labelType="inline" />
                </GridCell>
                {state.loading && this.renderLoading()}
                {(props.installedPackages && props.installedPackages.length > 0 && !state.loading) &&
                    <ExtensionList
                        packages={props.installedPackages}
                        isHost={props.isHost}
                        onEdit={props.onEdit.bind(this)}
                        onDelete={this.onDelete.bind(this)} />
                }
            </GridCell>
        );
    }
}

InstalledExtensions.propTypes = {
    dispatch: PropTypes.func.isRequired,
    installedPackages: PropTypes.array,
    installedPackageTypes: PropTypes.array,
    isHost: PropTypes.bool,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        installedPackageTypes: state.extension.installedPackageTypes,
        installedPackages: state.extension.installedPackages,
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(InstalledExtensions);