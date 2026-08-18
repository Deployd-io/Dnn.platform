import React, { Component } from "react";
import PropTypes from "prop-types";
import Output from "components/Output";
import Input from "components/Input";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Localization from "localization/Localization";
import "components/Prompt.less";
import * as PromptActionsCreators from "actions/prompt";
const logger = console;
import { PersonaBarPage } from "@dnnsoftware/dnn-react-common";

export class App extends Component {

    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.isPaging = false;
        this.history = [];
		logger.debug(`constructor(props=${props}): this.isPaging → ${this.isPaging}`);
        this.cmdOffset = 0; // reverse offset into history
        const { dispatch } = props;
		logger.debug(`constructor(props=${props}): this.history → ${this.history}`);
        this.actions = bindActionCreators(PromptActionsCreators, dispatch);
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    componentDidUpdate() {
		logger.debug('>>> Entering componentDidUpdate()');
        this.setFocus(true);
        this.scrollToBottom();
		logger.debug('<<< Exiting componentDidUpdate()');
    }
    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        this.showGreeting();
		logger.debug('<<< Exiting componentDidMount()');
    }

    onClickHandler(e) {
		logger.debug(`>>> Entering onClickHandler(e=${e})`);
        if (e.target.classList.contains("dnn-prompt-cmd-insert")) {
            this.setValue(e.target.dataset.cmd.replace(/'/g, "\""));
            this.setFocus(true);
        } else {
            this.setFocus(true);
			logger.debug(`<<< Exiting onClickHandler(e=${e})`);
        }
    }

    scrollToBottom() {
		logger.debug('>>> Entering scrollToBottom()');
        if (this.cmdPrompt) {
            this.cmdPrompt.scrollTop = this.cmdPrompt.scrollHeight;
			logger.debug('<<< Exiting scrollToBottom()');
        }
    }

    showGreeting() {
		logger.debug('>>> Entering showGreeting()');
        this.actions.runLocalCommand("INFO", Localization.get("PromptGreeting"), "cmd");
		logger.debug('<<< Exiting showGreeting()');
    }
    setValue(value) {
        this.cmdPromptInputControl.setValue(value);
    }
    setFocus(focus) {
		logger.debug(`>>> Entering setFocus(focus=${focus})`);
        this.cmdPromptInputControl.setFocus(focus);
		logger.debug(`<<< Exiting setFocus(focus=${focus})`);
    }

    runCmd() {
		logger.debug('>>> Entering runCmd()');
        this.cmdPromptInputControl.runCmd();
		logger.debug('<<< Exiting runCmd()');
    }
    updateHistory(value, isClear) {
		logger.debug(`>>> Entering updateHistory(value=${value},isClear=${isClear})`);
        this.cmdOffset = 0; // reset history index
        if (isClear) {
            this.history = [];
        } else {
            //Remove command from history if already exists. 
            if (this.history.some(item => item === value)) {
                this.history = this.history.filter(item => {
                    return item !== value;
                });
            }
            //Push the command as the last run.
            this.history.push(value);
			logger.debug(`<<< Exiting updateHistory(value=${value},isClear=${isClear})`);
        }
    }
    paging(isPaging) {
		logger.debug(`>>> Entering paging(isPaging=${isPaging})`);
        this.isPaging = isPaging;
		logger.debug(`<<< Exiting paging(isPaging=${isPaging})`);
    }
    setHeight(height) {
		logger.debug(`>>> Entering setHeight(height=${height})`);
        if (this.cmdPrompt !== null && this.cmdPrompt !== undefined && height !== undefined && height !== "") {
            height = height.replace("%", "");
            if (parseInt(height) > 0 && parseInt(height) <= 100)
				logger.debug(`<<< Exiting setHeight(height=${height})`);
                this.cmdPrompt.style.height = height + "%";
        }
    }

    endPaging() {
		logger.debug('>>> Entering endPaging()');
        this.actions.endPaging();
        this.setValue("");
        this.setFocus(true);
		logger.debug('<<< Exiting endPaging()');
    }

    keyDownHandler(e) {
		logger.debug(`>>> Entering keyDownHandler(e=${e})`);
        const { props } = this;

        const lastPage = props.paging !== null && props.paging.pageNo === props.paging.totalPages;

        //CTRL + Key
        if (e.ctrlKey) {
            if (e.keyCode === 192) {
                if (this.wrapper[0].offsetLeft <= 0) {
                    this.util.loadPanel("Dnn.Prompt", {
                        moduleName: "Dnn.Prompt",
                        folderName: "",
                        identifier: "Dnn.Prompt",
                        path: "Prompt"
                    });
                } else {
                    this.util.closePersonaBar();
                }
                return;
            }
            if (e.keyCode === 88) {
                this.endPaging();
                return;
            }
        }

        if (props.isBusy) return;
        if (
            document.activeElement.className.indexOf("dnn-prompt-input") > -1
            && document.activeElement.offsetParent !== null
            && document.activeElement.offsetParent.className.indexOf("dnn-prompt-input-wrapper") > -1
            && document.activeElement.tagName === "INPUT"
            && document.activeElement.type === "text") {
            switch (e.keyCode) {
                case 13: // enter key
                    if (this.isPaging) {
                        this.setValue("");
                        this.runCmd();
                        this.setFocus(false);
                        return;
                    }
                    return this.runCmd();
                case 38: // Up arrow
                    if ((this.history.length + this.cmdOffset > 0)) {
                        this.cmdOffset--;
                        this.setValue(this.history[this.history.length + this.cmdOffset]);
                        e.preventDefault();
                    }
                    break;
                case 40: // Down arrow
                    if ((this.cmdOffset < -1)) {
                        this.cmdOffset++;
                        this.setValue(this.history[this.history.length + this.cmdOffset]);
                        e.preventDefault();
                    }
                    break;
            }
        }

        if (this.isPaging && !e.ctrlKey) {
            this.setValue("");
            this.setFocus(false);
            this.runCmd();
            if (lastPage) {
                this.endPaging();
            }
			logger.debug(`<<< Exiting keyDownHandler(e=${e})`);
            return;
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props } = this;
		logger.debug('<<< Exiting render()');
        return (
            <PersonaBarPage isOpen={true} fullWidth={true}>
                <div className="wrapper">
                    <div
                        className="dnn-prompt"
                        style={{ display: "block" }}
                        onKeyDown={this.keyDownHandler.bind(this)}
                        onClick={this.onClickHandler.bind(this)}
                        ref={(el) => this.cmdPrompt = el}>
                        <Output
                            {...props}
                            className="Output"
                            scrollToBottom={this.scrollToBottom.bind(this)}
                            IsPaging={this.paging.bind(this)}></Output>
                        <br />
                        {!props.isBusy && <Input
                            ref={(el) => this.cmdPromptInputControl = el}
                            {...props}
                            actions={this.actions}
                            updateHistory={this.updateHistory.bind(this)}
                            paging={props.paging}
                            setHeight={this.setHeight.bind(this)} />}
                    </div>
                </div>
            </PersonaBarPage>
        );
    }
}
App.propTypes = {
    output: PropTypes.string,
    data: PropTypes.array,
    isHtml: PropTypes.bool,
    reload: PropTypes.bool,
    isError: PropTypes.bool,
    clearOutput: PropTypes.bool,
    fieldOrder: PropTypes.array,
    commandList: PropTypes.array,
    style: PropTypes.string,
    isBusy: PropTypes.bool,
    isHelp: PropTypes.bool,
    name: PropTypes.string,
    nextPageCommand: PropTypes.string,
    description: PropTypes.string,
    options: PropTypes.array,
    resultHtml: PropTypes.string,
    error: PropTypes.string,
    dispatch: PropTypes.func,
    paging: PropTypes.object,
};

function mapStateToProps(state) {
	logger.debug(`>>> Entering mapStateToProps(state=${state})`);
	logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
    return {
        output: state.output,
        data: state.data,
        paging: state.pagingInfo,
        isBusy: state.isBusy,
        isHtml: state.isHtml,
        reload: state.reload,
        style: state.style,
        fieldOrder: state.fieldOrder,
        commandList: state.commandList,
        isError: state.isError,
        clearOutput: state.clearOutput,
        isHelp: state.isHelp,
        name: state.name,
        description: state.description,
        options: state.options,
        resultHtml: state.resultHtml,
        error: state.error,
        nextPageCommand: state.nextPageCommand,
    };
}

export default connect(mapStateToProps)(App);