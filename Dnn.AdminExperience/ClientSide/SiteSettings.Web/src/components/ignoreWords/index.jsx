import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    search as SearchActions
} from "../../actions";
import IgnoreWordsRow from "./ignoreWordsRow";
import IgnoreWordsEditor from "./ignoreWordsEditor";
import "./style.less";
import { Dropdown, SvgIcons } from "@dnnsoftware/dnn-react-common";
import util from "../../utils";
import resx from "../../resources";
const logger = console;

let tableFields = [];

class IgnoreWordsPanel extends Component {
    constructor() {
        super();
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.state = {
            ignoreWords: undefined,
            openId: "",
            culture: ""
        };
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    loadData() {
		logger.error('>>> Entering loadData()', { "event.action": "loadData" });
        const { props } = this;
        props.dispatch(SearchActions.getIgnoreWords(props.portalId, props.cultureCode, (data) => {
			logger.warn('<<< Exiting loadData()', { "event.action": "loadData" });
            this.setState({
                ignoreWords: Object.assign({}, data),
                culture: data.CultureCode
            });
        }));
    }

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        const { props } = this;

        if (tableFields.length === 0) {
            tableFields.push({ "name": resx.get("IgnoreWords"), "id": "IgnoreWords" });
        }

        props.dispatch(SearchActions.getCultureList(props.portalId));
        this.loadData();
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        const { props } = this;

        if (props.ignoreWords) {
            let portalIdChanged = props.portalId !== undefined && prevProps.portalId !== props.portalId;
            let cultureCodeChanged = props.cultureCode !== undefined && prevProps.cultureCode !== props.cultureCode;

            if (portalIdChanged || cultureCodeChanged) {
                this.loadData();
				logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
            }
        }
    }

    renderHeader() {
		logger.error('>>> Entering renderHeader()', { "event.action": "renderHeader" });
        let tableHeaders = tableFields.map((field) => {
            let className = "words-items header-" + field.id;
            return <div className={className} key={"header-" + field.id}>
                <span>{field.name}&nbsp; </span>
            </div>;
        });
		logger.warn('<<< Exiting renderHeader()', { "event.action": "renderHeader" });
        return <div className="header-row">{tableHeaders}</div>;
    }

    uncollapse(id) {
        this.setState({
            openId: id
        });
    }

    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: ""
            });
        }
    }

    toggle(openId) {
        if (openId !== "") {
            this.uncollapse(openId);
        }
    }

    onUpdateIgnoreWords(words) {
        const {props} = this;
        if (words.StopWordsId) {
            props.dispatch(SearchActions.updateIgnoreWords(Object.assign({PortalId: props.ignoreWords.PortalId}, words), () => {
                util.utilities.notify(resx.get("IgnoreWordsUpdateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
        else {
            props.dispatch(SearchActions.addIgnoreWords(Object.assign({PortalId: props.ignoreWords.PortalId}, words), () => {
                util.utilities.notify(resx.get("IgnoreWordsCreateSuccess"));
                this.collapse();
            }, (error) => {
                const errorMessage = JSON.parse(error.responseText);
                util.utilities.notifyError(errorMessage.Message);
            }));
        }
    }

    onDeleteIgnoreWords(words) {
        const {props} = this;
        util.utilities.confirm(resx.get("IgnoreWordsDeletedWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SearchActions.deleteIgnoreWords(Object.assign({PortalId: props.ignoreWords.PortalId}, words), () => {
                util.utilities.notify(resx.get("IgnoreWordsDeleteSuccess"));
                this.collapse();
            }, () => {
                util.utilities.notify(resx.get("IgnoreWordsDeleteError"));
            }));
        });
    }

    onSelectCulture(event) {
        let {props} = this;

        this.setState({
            culture: event.value
        });

        props.dispatch(SearchActions.getIgnoreWords(props.portalId, event.value, (data) => {
            this.setState({
                ignoreWords: Object.assign({}, data.IgnoreWords)
            });
        }));
    }

    getCultureOptions() {
		logger.error('>>> Entering getCultureOptions()', { "event.action": "getCultureOptions" });
        const {props} = this;
        let options = [];
        if (props.cultures !== undefined) {
            options = props.cultures.map((item) => {
                return {
                    label: <div style={{ float: "left", display: "flex" }}>
                        <div className="language-flag">
                            <img src={item.Icon} alt={item.Name} />
                        </div>
                        <div className="language-name">{item.Name}</div>
                    </div>, value: item.Code
                };
            });
        }
		logger.warn('<<< Exiting getCultureOptions()', { "event.action": "getCultureOptions" });
        return options;
    }

     
    renderedIgnoreWords() {
		logger.error('>>> Entering renderedIgnoreWords()', { "event.action": "renderedIgnoreWords" });
        const {props} = this;
        if (props.ignoreWords && props.ignoreWords.StopWords) {
			logger.warn('<<< Exiting renderedIgnoreWords()', { "event.action": "renderedIgnoreWords" });
            return (
                <IgnoreWordsRow
                    wordsId={props.ignoreWords.StopWordsId}
                    tags={props.ignoreWords.StopWords}
                    index={0}
                    key={"wordsItem-0"}
                    closeOnClick={true}
                    openId={this.state.openId}
                    OpenCollapse={this.toggle.bind(this)}
                    Collapse={this.collapse.bind(this)}
                    onDelete={this.onDeleteIgnoreWords.bind(this, props.ignoreWords)}
                    id={"row-1"}
                    visible={true}>
                    <IgnoreWordsEditor
                        words={props.ignoreWords}
                        culture={this.state.culture}
                        Collapse={this.collapse.bind(this)}
                        onUpdate={this.onUpdateIgnoreWords.bind(this)}
                        id={"row-1"}
                        openId={this.state.openId} />
                </IgnoreWordsRow>
            );
        }
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props, state} = this;
        let opened = (state.openId === "add");
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>
                <div className="words-group-items">
                    {props.ignoreWords &&
                        <div className="AddItemRow">
                            <div className="sectionTitle">{resx.get("IgnoreWords")}</div>
                            {!props.ignoreWords.StopWords &&
                                <div className={opened ? "AddItemBox-active" : "AddItemBox"} onClick={this.toggle.bind(this, opened ? "" : "add")}>
                                    <div className="add-icon"><SvgIcons.AddIcon />
                                    </div> {resx.get("cmdAddWord")}
                                </div>
                            }
                            {this.props.cultures && this.props.cultures.length > 1 &&
                                <div className="language-filter">
                                    <Dropdown
                                        value={this.state.culture}
                                        style={{ width: "auto" }}
                                        options={this.getCultureOptions()}
                                        withBorder={false}
                                        onSelect={this.onSelectCulture.bind(this)}
                                    />
                                </div>
                            }
                        </div>
                    }
                    <div className="words-items-grid">
                        {this.renderHeader()}
                        <IgnoreWordsRow
                            tags={"-"}
                            index={"add"}
                            key={"wordsItem-add"}
                            closeOnClick={true}
                            openId={state.openId}
                            OpenCollapse={this.toggle.bind(this)}
                            Collapse={this.collapse.bind(this)}
                            onDelete={this.onDeleteIgnoreWords.bind(this)}
                            id={"add"}
                            visible={opened}>
                            <IgnoreWordsEditor
                                Collapse={this.collapse.bind(this)}
                                culture={this.state.culture}
                                onUpdate={this.onUpdateIgnoreWords.bind(this)}
                                id={"add"}
                                openId={state.openId} />
                        </IgnoreWordsRow>
                        {this.renderedIgnoreWords()}
                    </div>
                </div>

            </div >
        );
    }
}

IgnoreWordsPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    ignoreWords: PropTypes.object,
    portalId: PropTypes.number,
    cultures: PropTypes.array,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        ignoreWords: state.search.ignoreWords,
        tabIndex: state.pagination.tabIndex,
        cultures: state.search.cultures,
    };
}

export default connect(mapStateToProps)(IgnoreWordsPanel);