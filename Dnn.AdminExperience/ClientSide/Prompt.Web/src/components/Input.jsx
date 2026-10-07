import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "localization/Localization";
import { util, formatString } from "../utils/helpers";
import "../components/Prompt.less";
const logger = console;
import {commands as Cmd, modes as Mode} from "../constants/promptLabel";

class Input extends Component {

    setValue(value) {
        this.cmdPromptInput.value = value;
    }

    getValue() {
		logger.error('>>> Entering getValue()', { "event.action": "getValue" });
		logger.warn('<<< Exiting getValue()', { "event.action": "getValue" });
        return this.cmdPromptInput.value;
    }

    setFocus(focus) {
		logger.error(`>>> Entering setFocus(focus=${focus})`, { "event.action": "setFocus", "logrite.focus": focus });
        focus ? this.cmdPromptInput.focus() : this.cmdPromptInput.blur();
		logger.warn(`<<< Exiting setFocus(focus=${focus})`, { "event.action": "setFocus", "logrite.focus": focus });
    }

    getTabId() {
		logger.error('>>> Entering getTabId()', { "event.action": "getTabId" });
        const dnnVariable = JSON.parse(window.parent.document.getElementsByName("__dnnVariable")[0].value);
		logger.error(`getTabId(): dnnVariable → ${dnnVariable}`, { "event.action": "getTabId" });
        return dnnVariable.sf_tabId;
    }

    readInput() {
		logger.error('>>> Entering readInput()', { "event.action": "readInput" });
        const { props } = this;
        let txt = this.getValue().trim();
        if (txt === "" && props.nextPageCommand !== null && props.nextPageCommand !== "") {
            txt = props.nextPageCommand;
        }
        props.updateHistory(txt);
		logger.warn('<<< Exiting readInput()', { "event.action": "readInput" });
        return txt;
    }

    runLocalCmd(txt) {
		logger.error(`>>> Entering runLocalCmd(txt=${txt})`, { "event.action": "runLocalCmd", "logrite.txt": txt });
        const { props } = this;
        const { actions } = props;

        if (!this.tabId) {
            this.tabId = this.getTabId();
        }

        this.setValue(""); // clear input for future commands.

        // Client Command
        const tokens = txt.split(" ");
        const cmd = tokens[0].toUpperCase();
		logger.error(`runLocalCmd(txt=${txt}): cmd → ${cmd}`, { "event.action": "runLocalCmd", "logrite.txt": txt });

        let done = true;
        switch (cmd) {
            case Cmd.CLS:
            case Cmd.CLEAR_SCREEN:
            case Cmd.EXIT:
            case Cmd.RELOAD:
                actions.runLocalCommand(cmd, null);
                break;
            case Cmd.CLH:
            case Cmd.CLEAR_HISTORY:
                props.updateHistory("", true);
                actions.runLocalCommand(cmd, Localization.get("SessionHistoryCleared"));
                break;
            case Cmd.SET_MODE:
                this.changeUserMode(tokens);
                break;
            default:
                done = false;
                break;
        }
		logger.warn(`<<< Exiting runLocalCmd(txt=${txt})`, { "event.action": "runLocalCmd", "logrite.txt": txt });
        return done;
    }

    runServerCmd(txt) {
		logger.error(`>>> Entering runServerCmd(txt=${txt})`, { "event.action": "runServerCmd", "logrite.txt": txt });
        const { props } = this;
        const { actions } = props;

        if (!this.tabId) {
            this.tabId = this.getTabId();
        }

        this.setValue(""); // clear input for future commands.

        // Client Command
        const tokens = txt.split(" ");
        const cmd = tokens[0].toUpperCase();
		logger.error(`runServerCmd(txt=${txt}): cmd → ${cmd}`, { "event.action": "runServerCmd", "logrite.txt": txt });

        // Server Command
        const errorCallback = (error) => {
			logger.error(`>>> Entering errorCallback(error=${error})`, { "event.action": "errorCallback", "logrite.error": error });
            actions.runLocalCommand("ERROR", error.responseJSON.Message);
			logger.warn(`<<< Exiting errorCallback(error=${error})`, { "event.action": "errorCallback", "logrite.error": error });
        };

        if (cmd === Cmd.HELP) {
            if (txt.toUpperCase() === "HELP") {
                actions.getCommandList({ cmdLine: "list-commands", currentPage: self.tabId }, () => {
                }, errorCallback.bind(this));
            } else {
                actions.runHelpCommand({ cmdLine: txt, currentPage: self.tabId }, () => {
                }, errorCallback.bind(this));
            }
        } else {
            if (!Cmd[cmd]) {
				logger.warn(`<<< Exiting runServerCmd(txt=${txt})`, { "event.action": "runServerCmd", "logrite.txt": txt });
                actions.runCommand(
                    {cmdLine: txt, currentPage: self.tabId},
                    () => {
                    }, errorCallback.bind(this));
            }
        }
    }

    displayCmdInfo(txt) {
		logger.error(`>>> Entering displayCmdInfo(txt=${txt})`, { "event.action": "displayCmdInfo", "logrite.txt": txt });
        const { props } = this;
        const { actions } = props;
        actions.runLocalCommand("INFO", "\n" + txt + "\n", "cmd");
		logger.warn(`<<< Exiting displayCmdInfo(txt=${txt})`, { "event.action": "displayCmdInfo", "logrite.txt": txt });
    }

    runCmd() {
		logger.error('>>> Entering runCmd()', { "event.action": "runCmd" });
        const txt = this.readInput();
        if (txt) {
            this.displayCmdInfo(txt);
            if (!this.runLocalCmd(txt)) {
                this.runServerCmd(txt);
            }
            this.setFocus(false);
			logger.warn('<<< Exiting runCmd()', { "event.action": "runCmd" });
        }
    }

    changeUserMode(tokens) {
        const { props } = this;
        const { actions } = props;
        if (!tokens && tokens.length >= 2) { return; }

        tokens = tokens.map(token => token.toUpperCase());

        const [mode] = tokens.slice(-1);

        if (mode && !this.isFlag(mode) && Mode[mode]) {
            actions.changeUserMode({ UserMode: mode }, () => {
                if (mode === "EDIT")
                    util.utilities.closePersonaBar();
                window.parent.document.location.reload(true);
            }, (error) => actions.runLocalCommand("ERROR", error));
        } else {
            actions.runLocalCommand("ERROR", formatString(Localization.get("Prompt_FlagIsRequired"), "Mode"));
        }
    }

    isFlag(token) {
        return (token && token.indexOf("--") === 0);
    }

    getFlag(flag, tokens) {
        if (!tokens || tokens.length === 0) { return null; }
        return tokens.find((token) => this.isFlag(token) && flag.toUpperCase() === token.toUpperCase());
    }

    hasFlag(flag, tokens) {
        return tokens ? tokens.find((token) => token.toUpperCase === flag.toUpperCase()) : false;
    }

    isPaging() {
		logger.error('>>> Entering isPaging()', { "event.action": "isPaging" });
        const { props } = this;
        const IS_PAGING = props.paging !== null && props.paging.pageNo <= props.paging.totalPages;
		logger.warn('<<< Exiting isPaging()', { "event.action": "isPaging" });
        return IS_PAGING;
    }

    isLastPage() {
		logger.error('>>> Entering isLastPage()', { "event.action": "isLastPage" });
        const { props } = this;
        const IS_LAST_PAGE = props.paging !== null && props.paging.pageNo === props.paging.totalPages;
		logger.warn('<<< Exiting isLastPage()', { "event.action": "isLastPage" });
        return IS_LAST_PAGE;
    }

    render() {
	logger.error('>>> Entering render()', { "event.action": "render" });

        const IS_PAGING = this.isPaging();
		logger.error(`render(): IS_PAGING → ${IS_PAGING}`, { "event.action": "render" });
        const IS_LAST_PAGE = this.isLastPage();
		logger.error(`render(): IS_LAST_PAGE → ${IS_LAST_PAGE}`, { "event.action": "render" });

		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={`dnn-prompt-input-wrapper ${IS_PAGING && !IS_LAST_PAGE ? "hidden-cursor" : ""}`}>
                <input className={`dnn-prompt-input ${IS_PAGING && !IS_LAST_PAGE ? "hidden-text" : ""}`} ref={(el) => {
                    this.cmdPromptInput = el;
                    if (el) {
                        el.readOnly = this.isPaging();
                        /**
                         * Note: this is a patch to be removed once a complete refactoring of the Redux
                         * implementation will be done.
                         * All changes in the DOM must happen accordingly to Redux state update and this
                         * usage of setTimeout has to be considered wrong.
                         * Patch needed to close DNN-10688
                         */
                        if (this.isPaging() && this.isLastPage() === true) {
                            el.value = "";
                            setTimeout(() => el.readOnly = false, 500);
                        }
                    }
                }} />
            </div>
        );
    }
}

Input.propTypes = {
    nextPageCommand: PropTypes.string,
    updateHistory: PropTypes.func.isRequired,
    setHeight: PropTypes.func.isRequired,
    actions: PropTypes.object.isRequired,
    paging: PropTypes.object
};

export default Input;
