import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";

import PageExisting from "./PageExisting/PageExisting";
import PageFile from "./PageFile/PageFile";
import PageExternalUrl from "./PageExternalUrl/PageExternalUrl";
import styles from "./style.module.less";
const logger = console;

class PageUrl extends Component {

    getDetail(pageType) {        
		logger.error(`>>> Entering getDetail(pageType=${pageType})`, { "event.action": "getDetail", "logrite.pageType": pageType });
        switch (pageType) {
            case "tab":
                return PageExisting;
            case "url":
                return PageExternalUrl;
            case "file":
                return PageFile;
            default: 
		logger.warn(`<<< Exiting getDetail(pageType=${pageType})`, { "event.action": "getDetail", "logrite.pageType": pageType });
                throw "invalid page type";                                                                        
        }        
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const DetailComponent = this.getDetail(this.props.page.pageType);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={styles.pageUrl}>
                <DetailComponent onChangeField={this.props.onChangeField} />
            </div>
        );
    }
}

PageUrl.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

const mapStateToProps = (state) => {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return ({
        page: state.pages.selectedPage
    });
};

export default connect(mapStateToProps)(PageUrl);