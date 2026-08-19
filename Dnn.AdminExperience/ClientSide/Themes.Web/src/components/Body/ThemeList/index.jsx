import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import { Scrollbars } from "react-custom-scrollbars";
import Localization from "localization";
import Theme from "./Theme";
import NoDataIcon from "./../SvgIcon/nodata.svg";
import "./style.less";
const logger = console;

class ThemeList extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {};
		logger.debug(`constructor(): this.state → ${this.state}`);
		logger.debug('<<< Exiting constructor()');
    }


    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
        let globalThemes = props.dataSource.filter(t => t.level === 4);
		logger.debug(`render(): globalThemes → ${globalThemes}`);
        let siteThemes = props.dataSource.filter(t => t.level === 1 || t.level === 2);
		logger.debug('<<< Exiting render()');
        return (
            <div className="theme-list-wrapper">
                {globalThemes.length === 0 && siteThemes.length === 0 &&
                    <div className="empty-state">
                        <div className="noThemes">{Localization.get("NoThemes")}</div>
                        <div className="noThemesMessage">{Localization.get("NoThemesMessage")}</div>
                        <div className="noThemesIcon"><NoDataIcon /></div>
                    </div>
                }
                {globalThemes.length > 0 &&
                    <div>
                        <div className="theme-list-title">{Localization.get("GlobalThemes") }</div>
                        <Scrollbars
                            className="theme-list"
                            autoHeight
                            autoHeightMin={0}
                            autoHeightMax={480}>
                            <GridCell>
                                {globalThemes.map((theme, i) => {
                                    return <GridCell key={i} columnSize={25}><Theme theme={theme} /></GridCell>;
                                })}
                            </GridCell>
                        </Scrollbars>
                    </div>
                }
                {globalThemes.length > 0 && siteThemes.length > 0 &&
                    <hr className="theme-list-separator"></hr>
                }
                {siteThemes.length > 0 &&
                    <div>
                        <div className="theme-list-title">{Localization.get("SiteThemes") }</div>
                        <Scrollbars
                            className="theme-list"
                            autoHeight
                            autoHeightMin={0}
                            autoHeightMax={480}>
                            <GridCell>
                                {siteThemes.map((theme, i) => {
                                    return <GridCell key={i} columnSize={25}><Theme theme={theme} /></GridCell>;
                                })}
                            </GridCell>
                        </Scrollbars>
                    </div>
                }
            </div>
        );
    }
}

ThemeList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    dataSource: PropTypes.array
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeList);