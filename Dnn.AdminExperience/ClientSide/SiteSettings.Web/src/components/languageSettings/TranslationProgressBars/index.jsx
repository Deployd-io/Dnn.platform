import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import resx from "../../../resources";
import ProgressBar from "./progressBar";
import "./style.less";
const logger = console;

class TranslationProgressBars extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.currentTime = Date.now();
		logger.debug('<<< Exiting constructor()');
    }

    normalizeTime(time) {
        if (time < 10) {
            return "0" + time;
        }
        return time;
    }

    getTime() {
		logger.debug('>>> Entering getTime()');
        const time = Date.now() - this.currentTime;
        const seconds = Math.floor(time / 1000);
		logger.debug(`getTime(): time → ${time}`);
		logger.debug(`getTime(): seconds → ${seconds}`);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
		logger.debug(`getTime(): minutes → ${minutes}`);
		logger.debug(`getTime(): hours → ${hours}`);
		logger.debug('<<< Exiting getTime()');
        return `${this.normalizeTime(hours)}:${this.normalizeTime(minutes % 60)}:${this.normalizeTime(seconds % 60)}s`;
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        const totalProgressText =
            resx.get("TotalProgress").replace("[number]", props.PrimaryPercent) + " - " +
            resx.get("TotalLanguages").replace("[number]", props.PrimaryTotal);
        const progressText = resx.get("Progress").replace("[number]", props.SecondaryPercent);
		logger.debug(`render(): progressText → ${progressText}`);

		logger.debug('<<< Exiting render()');
        return <div className={"translation-progress-bars" + (props.Error ? " error" : "") }>
            <div className="text">
                <span>{resx.get("TranslationProgressBarText").replace("[number]", props.SecondaryTotal) }</span>
            </div>
            <ProgressBar text={totalProgressText} percentageValue={props.PrimaryPercent}/>
            <ProgressBar text={progressText} rightText={props.CurrentOperationText} percentageValue={props.SecondaryPercent}/>
            <div className="text time">
                {resx.get("ElapsedTime") }
                <span>{this.getTime() }</span>
            </div>
        </div>;
    }
}

TranslationProgressBars.propTypes = {
    dispatch: PropTypes.func.isRequired,
    closePersonaBarPage: PropTypes.func,
    languages: PropTypes.array,
    languageSettings: PropTypes.object,
    InProgress: PropTypes.bool,
    PrimaryPercent: PropTypes.number,
    PrimaryTotal: PropTypes.number,
    PrimaryValue: PropTypes.number,
    SecondaryPercent: PropTypes.number,
    SecondaryTotal: PropTypes.number,
    SecondaryValue: PropTypes.number,
    TimeEstimated: PropTypes.number,
    CurrentOperationText: PropTypes.array,
    Error: PropTypes.strings
};

function mapStateToProps(state) {
logger.debug(`>>> Entering mapStateToProps(state=${state})`);

	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        languages: state.languages.languageList,
        languageSettings: state.languages.languageSettings
    };
}

export default connect(mapStateToProps)(TranslationProgressBars);