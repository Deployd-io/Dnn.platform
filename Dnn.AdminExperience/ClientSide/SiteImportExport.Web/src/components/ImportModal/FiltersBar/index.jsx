import React, {Component} from "react";
import PropTypes from "prop-types";
import "./style.less";
import Localization from "localization";
const logger = console;

import {Dropdown, GridCell, SearchBox} from "@dnnsoftware/dnn-react-common";

class FiltersBar extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.state = {
            selectedJobFilter: {
                label: Localization.get("SortByDateNewest"),
                value: "newest"
            },
            searchText: ""
        };
		logger.warn(`<<< Exiting constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
    }

    onSelect(option) {
        let { selectedJobFilter } = this.state;
        if (option.value !== selectedJobFilter.value) {
            selectedJobFilter.label = option.label;
            selectedJobFilter.value = option.value;

            this.setState({
                selectedJobFilter
            }, () => { this.props.onFilterChanged(option); });
        }
    }

    BuildFiltersOptions() {
		logger.error('>>> Entering BuildFiltersOptions()', { "event.action": "BuildFiltersOptions" });
        const jobFilters = [
            { "Key": Localization.get("SortByDateNewest"), "Value": "newest" },
            { "Key": Localization.get("SortByDateOldest"), "Value": "oldest" },
            { "Key": Localization.get("SortByName"), "Value": "name" }
        ];
		logger.warn('<<< Exiting BuildFiltersOptions()', { "event.action": "BuildFiltersOptions" });
        return jobFilters.map((jobFilters) => {
            return {label: jobFilters.Key, value: jobFilters.Value};
        });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        let jobFiltersOptions = this.BuildFiltersOptions();
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return <div className="jobs-filter-container">
            <GridCell columnSize={35} >
                {
                    jobFiltersOptions.length > 0 &&
                    <div className="job-filters-filter">
                        <Dropdown style={{ width: "100%" }}
                            withBorder={false}
                            options={jobFiltersOptions}
                            onSelect={this.onSelect.bind(this)}
                            value={this.state.selectedJobFilter.value}
                            prependWith={Localization.get("ShowSortLabel")}
                        />
                        <div className="clear">
                        </div>
                    </div>
                }
            </GridCell>
            <GridCell columnSize={30} >
                <div>&nbsp; </div></GridCell>
            <GridCell columnSize={35} >
                <div className="search-filter">
                    <SearchBox placeholder={Localization.get("SearchPlaceHolder")} onSearch={this.props.onKeywordChanged} maxLength={50} iconStyle={{ right: 0 }} />
                    <div className="clear"></div>
                </div>
            </GridCell>
        </div>;
    }
}
FiltersBar.propTypes = {
    onFilterChanged: PropTypes.func.isRequired,
    onKeywordChanged: PropTypes.func.isRequired
};
export default (FiltersBar);