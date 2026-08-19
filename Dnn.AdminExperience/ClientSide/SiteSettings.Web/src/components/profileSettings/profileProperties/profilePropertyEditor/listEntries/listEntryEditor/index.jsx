import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import { SingleLineInputWithError, GridSystem, Switch, Label, Button, InputGroup } from "@dnnsoftware/dnn-react-common";
import resx from "resources";
const logger = console;

class ListEntryEditor extends Component {
    constructor(props) {
        super();
		logger.debug(`>>> Entering constructor(props=${props})`);

        this.state = {
            entryDetail: {
                EntryId: props.entryId,
                PortalId: props.portalId,
                ListName: props.listName,
                Text: props.text,
                Value: props.value,
                EnableSortOrder: props.enableSortOrder
            },
            error: {
                Text: props.text === "" ? true : false,
                Value: props.value === "" ? true : false
            },
            triedToSubmit: false
        };
    }

    onSettingChange(key, event) {
		logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
        let { state } = this;
        let entryDetail = Object.assign({}, state.entryDetail);
		logger.debug(`onSettingChange(key=${key},event=${event}): entryDetail → ${entryDetail}`);
        let value = typeof (event) === "object" ? event.target.value : event;
		logger.debug(`onSettingChange(key=${key},event=${event}): value → ${value}`);
        state.error[key] = value === "" ? true : false;
		logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
        entryDetail[key] = value;

        this.setState({
            entryDetail: entryDetail,
            triedToSubmit: false,
            error: state.error
        });
    }

    onSave() {
		logger.debug('>>> Entering onSave()');
        const { props, state } = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.Text || state.error.Value) {
            return;
        }

        props.onUpdate(state.entryDetail);
		logger.debug('<<< Exiting onSave()');
    }

    onCancel() {
		logger.debug('>>> Entering onCancel()');
        const { props } = this;
        props.Collapse();
		logger.debug('<<< Exiting onCancel()');
    }

     
    render() {
		logger.debug('>>> Entering render()');
        if (this.state.entryDetail !== undefined || this.props.id === "add") {
            const columnOne = <div className="left-column">
                <InputGroup>
                    <Label
                        label={resx.get("ListEntryText")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.Text && this.state.triedToSubmit}
                        errorMessage={resx.get("InvalidEntryText")}
                        value={this.state.entryDetail.Text}
                        onChange={this.onSettingChange.bind(this, "Text")}
                    />
                </InputGroup>
                {this.props.enableSortOrder === null &&
                    <InputGroup>
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("EnableSortOrder.Help")}
                            label={resx.get("EnableSortOrder")}
                        />
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            value={this.state.entryDetail.EnableSortOrder}
                            onChange={this.onSettingChange.bind(this, "EnableSortOrder")}
                        />
                    </InputGroup>
                }
            </div>;
            const columnTwo = <div className="right-column">
                <InputGroup>
                    <Label
                        label={resx.get("ListEntryValue")}
                    />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.Value && this.state.triedToSubmit}
                        errorMessage={resx.get("InvalidEntryValue")}
                        value={this.state.entryDetail.Value}
                        onChange={this.onSettingChange.bind(this, "Value")}
                    />
                </InputGroup>
            </div>;

			logger.debug('<<< Exiting render()');
            return (
                <div className="entry-editor">
                    <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                    <div className="editor-buttons-box">
                        <Button
                            type="neutral"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            type="primary"
                            onClick={this.onSave.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

ListEntryEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    entryDetail: PropTypes.object,
    entryId: PropTypes.number,
    listName: PropTypes.string,
    text: PropTypes.string,
    value: PropTypes.value,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    portalId: PropTypes.number,
    enableSortOrder: PropTypes.bool
};

export default (ListEntryEditor);