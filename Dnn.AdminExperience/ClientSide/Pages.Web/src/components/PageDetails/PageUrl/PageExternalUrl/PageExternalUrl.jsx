import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.module.less";
import { SingleLineInputWithError, InputGroup } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../../localization";
import PageUrlCommons from "../PageUrlCommons/PageUrlCommons";
const logger = console;

class PageExternalUrl extends Component {

    onChangeField(key, event) {
		logger.debug(`>>> Entering onChangeField(key=${key},event=${event})`);
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
		logger.debug(`<<< Exiting onChangeField(key=${key},event=${event})`);
    }

    render() {
		logger.debug('>>> Entering render()');
        const {page} = this.props;
        
		logger.debug('<<< Exiting render()');
        return (
            <div className={styles.pageExternalUrl}>
                <InputGroup>
                    <SingleLineInputWithError
                        className="external-url-input"
                        label={Localization.get("ExternalUrl")}
                        tooltipMessage={Localization.get("ExternalUrlTooltip")}    
                        value={page.externalRedirection}
                        onChange={this.onChangeField.bind(this, "externalRedirection")}
                        maxLength={255} />
                </InputGroup>
                <PageUrlCommons {...this.props} />
                <div style={{clear: "both"}}></div>
            </div>
        );
    }
}

PageExternalUrl.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

const mapStateToProps = (state) => {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return ({page : state.pages.selectedPage});
};

export default connect(mapStateToProps)(PageExternalUrl);