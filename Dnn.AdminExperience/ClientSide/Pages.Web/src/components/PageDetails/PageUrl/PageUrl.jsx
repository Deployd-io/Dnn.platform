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
		logger.debug(`>>> Entering getDetail(pageType=${pageType})`);
        switch (pageType) {
            case "tab":
                return PageExisting;
            case "url":
                return PageExternalUrl;
            case "file":
                return PageFile;
            default: 
		logger.debug(`<<< Exiting getDetail(pageType=${pageType})`);
                throw "invalid page type";                                                                        
        }        
    }

    render() {
		logger.debug('>>> Entering render()');
        const DetailComponent = this.getDetail(this.props.page.pageType);
		logger.debug('<<< Exiting render()');
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
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return ({
        page: state.pages.selectedPage
    });
};

export default connect(mapStateToProps)(PageUrl);