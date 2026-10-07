import PropTypes from "prop-types";
import React, { Component } from "react";
import "./style.less";
import Localization from "localization";
const logger = console;
import { Dropdown, SearchBox, GridCell } from "@dnnsoftware/dnn-react-common";

class FiltersBar extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            selectedUserFilter: {
                label: Localization.get("Authorized"),
                value: 0
            },
            searchText: ""
        };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }
    onSelect(option) {
        let { label} = option;
        let { value} = option;
        let {selectedUserFilter} = this.state;
        if (value !== selectedUserFilter.value) {
            selectedUserFilter.label = label;
            selectedUserFilter.value = value;

            if (selectedUserFilter.value === 0 || selectedUserFilter.value === 5) {
                this.setState({
                    selectedUserFilter: { label: "", value: -1 },
                    searchText: ""
                }, () => {
                    this.setState({
                        selectedUserFilter,
                        searchText: ""
                    }, () => { this.props.onChange(option, this.state.searchText); });
                });
            }
            else {
                this.setState({
                    selectedUserFilter,
                    searchText: ""
                }, () => { this.props.onChange(option, this.state.searchText); });
            }
        }
    }

    onKeywordChanged(text) {
		logger.error(`>>> Entering onKeywordChanged(text=${text})`, { "event.action": "onKeywordChanged", "logrite.text": text });
        this.setState({
            searchText: text
        }, () => {
            this.props.onChange(this.state.selectedUserFilter, text);
        });
		logger.warn(`<<< Exiting onKeywordChanged(text=${text})`, { "event.action": "onKeywordChanged", "logrite.text": text });
    }

    BuildUserFiltersOptions() {
		logger.error('>>> Entering BuildUserFiltersOptions()', { "event.action": "BuildUserFiltersOptions" });
        let {userFilters} = this.props;
        let userFiltersOptions = [];
        userFiltersOptions = userFilters.map((userFilter) => {
		logger.warn('<<< Exiting BuildUserFiltersOptions()', { "event.action": "BuildUserFiltersOptions" });
            return { label: userFilter.Key, value: userFilter.Value };
        });
        return userFiltersOptions;
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let userFiltersOptions = this.BuildUserFiltersOptions();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (<div className="users-filter-container">
            <GridCell columnSize={35} >
                {userFiltersOptions &&
                    userFiltersOptions.length > 0 &&
                    <div className="user-filters-filter">
                        <Dropdown style={{ width: "100%" }}
                            withBorder={false}
                            options={userFiltersOptions}
                            label={this.state.selectedUserFilter.label}
                            onSelect={this.onSelect.bind(this)}
                            prependWith={Localization.get("ShowLabel")}
                        />
                        <div className="clear">
                        </div>
                    </div>}
            </GridCell>
            <GridCell columnSize={30} >
                <div>&nbsp; </div></GridCell>
            <GridCell columnSize={35} >
                <div className="search-filter">
                    <SearchBox placeholder={Localization.get("SearchPlaceHolder")} onSearch={this.onKeywordChanged.bind(this)} maxLength={50} iconStyle={{ right: 0 }} />
                    <div className="clear"></div>
                </div>
            </GridCell>
        </div>);
    }
}
FiltersBar.propTypes = {
    dispatch: PropTypes.func,
    onChange: PropTypes.func.isRequired,
    userFilters: PropTypes.array.isRequired
};
export default (FiltersBar);