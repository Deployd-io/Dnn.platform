import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Localization from "localization";
import { GridCell, SearchBox, Dropdown } from "@dnnsoftware/dnn-react-common";
import utils from "utils";
import RestoreTheme from "./RestoreTheme";
import "./style.less";
const logger = console;

let canEdit = false;
class MiddleActions extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            selectedThemeFilter: {
                label: Localization.get("ThemeLevelAll"),
                value: 7
            }
        };
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
		logger.debug('<<< Exiting constructor()');
    }

    onKeywordChanged(value) {
		logger.debug(`>>> Entering onKeywordChanged(value=${value})`);
        const { props } = this;

        props.onSearch.call(this, value);
		logger.debug(`<<< Exiting onKeywordChanged(value=${value})`);
    }

    buildFiltersOptions() {
		logger.debug('>>> Entering buildFiltersOptions()');
        const levelFilters = [
            { "Key": Localization.get("ThemeLevelAll"), "Value": 7 },
            { "Key": Localization.get("ThemeLevelGlobal"), "Value": 4 },
            { "Key": Localization.get("ThemeLevelSite"), "Value": 2 }
        ];
		logger.debug('<<< Exiting buildFiltersOptions()');
        return levelFilters.map((levelFilter) => {
            return { label: levelFilter.Key, value: levelFilter.Value };
        });
    }

    onSelect(option) {
        let { selectedThemeFilter } = this.state;
        if (option.value !== selectedThemeFilter.value) {
            selectedThemeFilter.label = option.label;
            selectedThemeFilter.value = option.value;

            this.setState({
                selectedThemeFilter
            }, () => { this.props.onFilterChanged(option.value); });
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        let themeFilterOptions = this.buildFiltersOptions();
		logger.debug('<<< Exiting render()');
        return (
            <GridCell className="middle-actions">
                <GridCell columnSize={40}>
                    {canEdit &&
                        <RestoreTheme />
                    }
                </GridCell>
                <GridCell columnSize={30}>
                    <div className="theme-level-filter">
                        <Dropdown style={{ width: "100%" }}
                            withBorder={false}
                            options={themeFilterOptions}
                            onSelect={this.onSelect.bind(this)}
                            value={this.state.selectedThemeFilter.value}
                            prependWith={Localization.get("ShowFilterLabel")}
                        />
                        <div className="clear">
                        </div>
                    </div>
                </GridCell>
                <GridCell columnSize={30}>
                    <div className="search-filter">
                        {
                            <SearchBox placeholder={Localization.get("SearchPlaceHolder")} onSearch={this.onKeywordChanged.bind(this)} maxLength={50} iconStyle={{ right: 0 }} />
                        }
                        <div className="clear"></div>
                    </div>
                </GridCell>
            </GridCell>
        );
    }
}

MiddleActions.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onSearch: PropTypes.func.isRequired,
    onFilterChanged: PropTypes.func.isRequired
};

function mapStateToProps() {
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}

export default connect(mapStateToProps)(MiddleActions);