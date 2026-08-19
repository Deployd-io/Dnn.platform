import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import {
  Dropdown,
  InputGroup,
  Switch,
  Label,
  Button,
  Tooltip,
  MultiLineInputWithError,
  GridCell,
  GridSystem,
  Checkbox,
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import "./style.less";
const logger = console;

class CspSettingsPanelBody extends Component {
  constructor() {
    super();
	logger.debug('>>> Entering constructor()');
    this.state = {
      cspSettings: undefined,
      error: {
        CspHeader: false,
        CspHeaderErrors: [],
        CspReportingHeader: false,
        CspReportingHeaderErrors: [],
      },
      cspLockConfirm: false,
    };
	logger.debug('<<< Exiting constructor()');
  }

  UNSAFE_componentWillMount() {
	logger.debug('>>> Entering UNSAFE_componentWillMount()');
    const { props } = this;
    if (props.cspSettings) {
      this.setState({
        cspSettings: props.cspSettings,
        error: {
          CspHeader: false,
          CspHeaderErrors: [],
          CspReportingHeader: false,
          CspReportingHeaderErrors: [],
        },
        cspLockConfirm: false,
      });
      return;
    }
    this.getSettings();
	logger.debug('<<< Exiting UNSAFE_componentWillMount()');
  }

  getCspHeaderModeOptions() {
	logger.debug('>>> Entering getCspHeaderModeOptions()');
    let cspHeaderModeOptions = [];
    cspHeaderModeOptions.push({
      value: 0,
      label: resx.get("CspHeaderModeOff"),
    });
    cspHeaderModeOptions.push({
      value: 1,
      label: resx.get("CspHeaderModeReportOnly"),
    });
    cspHeaderModeOptions.push({ value: 2, label: resx.get("CspHeaderModeOn") });

	logger.debug('<<< Exiting getCspHeaderModeOptions()');
    return cspHeaderModeOptions;
  }

  onCspHeaderValueChange(key, event) {
	logger.debug(`>>> Entering onCspHeaderValueChange(key=${key},event=${event})`);
    this.onSettingChange(key, event);
	logger.debug(`<<< Exiting onCspHeaderValueChange(key=${key},event=${event})`);
  }

  onCspReportingHeaderValueChange(key, event) {
	logger.debug(`>>> Entering onCspReportingHeaderValueChange(key=${key},event=${event})`);
    this.onSettingChange(key, event);
	logger.debug(`<<< Exiting onCspReportingHeaderValueChange(key=${key},event=${event})`);
  }

  onSettingChange(key, event) {
	logger.debug(`>>> Entering onSettingChange(key=${key},event=${event})`);
    const { state, props } = this;
    let cspSettings = Object.assign({}, state.cspSettings);
	logger.debug(`onSettingChange(key=${key},event=${event}): cspSettings → ${cspSettings}`);
    cspSettings[key] = typeof event === "object" ? event.target.value : event;
    let error = Object.assign({}, state.error);
	logger.debug(`onSettingChange(key=${key},event=${event}): error → ${error}`);
    error[key] = false;
    error[key + "Errors"] = [];
    this.setState(
      {
        cspSettings: cspSettings,
        error: error,
      },
      () => {}
    );
    props.dispatch(SecurityActions.cspSettingsClientModified(cspSettings));
	logger.debug(`<<< Exiting onSettingChange(key=${key},event=${event})`);
  }

  onUpdate(event) {
	logger.debug(`>>> Entering onUpdate(event=${event})`);
    event.preventDefault();
    const { props, state } = this;

    props.dispatch(
      SecurityActions.updateCspSettings(
        state.cspSettings,
        (data) => {
          if (data.Success) {
            util.utilities.notify(resx.get("CspSettingsUpdateSuccess"));
            this.getSettings();
          } else {
            util.utilities.notifyError(data.Message);
            this.setState({
              cspSettings: state.cspSettings,
              error: data.Error,
            });
          }
        },
        () => {
          util.utilities.notifyError(resx.get("CspSettingsError"));
		logger.debug(`<<< Exiting onUpdate(event=${event})`);
        }
      )
    );
  }

  onCancel() {
	logger.debug('>>> Entering onCancel()');
    util.utilities.confirm(
      resx.get("CspSettingsRestoreWarning"),
      resx.get("Yes"),
      resx.get("No"),
      () => {
        this.getSettings();
		logger.debug('<<< Exiting onCancel()');
      }
    );
  }

  getSettings() {
	logger.debug('>>> Entering getSettings()');
    const { props } = this;
    props.dispatch(
      SecurityActions.getCspSettings((data) => {
        let cspSettings = Object.assign({}, data.Results.Settings);
        this.setState({
          cspSettings: cspSettings,
          error: {
            CspHeader: false,
            CspHeaderErrors: [],
            CspReportingHeader: false,
            CspReportingHeaderErrors: [],
          },
        });
      })
    );
	logger.debug('<<< Exiting getSettings()');
  }

  render() {
	logger.debug('>>> Entering render()');
    const { state } = this;

    if (state.cspSettings) {
	logger.debug('<<< Exiting render()');
      return (
        <div id="cspSettings-container">
          <div className="warningBox">
            <div className="warningText">{resx.get("CspSettings.Help")}</div>
          </div>
          {state.cspSettings.CspDisabled && (
            <div className="redWarningBox">
              <div className="warningText">{resx.get("CspDisabled.Help")}</div>
            </div>
          )}
          <InputGroup>
            <Label
              tooltipMessage={resx.get("plCspHeaderMode.Help")}
              label={resx.get("plCspHeaderMode")}
            />
            <Dropdown
              options={this.getCspHeaderModeOptions()}
              value={state.cspSettings.CspHeaderMode}
              onSelect={(newVal) => {
                this.onSettingChange("CspHeaderMode", newVal.value);
              }}
            />
          </InputGroup>
          <GridSystem numberOfColumns={2}>
            <GridCell>
              <InputGroup>
                <div>
                  <Label
                    labelType="inline"
                    tooltipMessage={resx.get("plCspHeaderFixed.Help")}
                    label={resx.get("plCspHeaderFixed")}
                  />
                  <Switch
                    onText={resx.get("SwitchOn")}
                    offText={resx.get("SwitchOff")}
                    value={state.cspSettings.CspHeaderFixed}
                    onChange={this.onSettingChange.bind(this, "CspHeaderFixed")}
                  />
                </div>
              </InputGroup>
            </GridCell>
            <GridCell>
              {state.cspSettings.CspHeaderFixed && (
                <div className="csp-settings-locked-confirm">
                  <div>{resx.get("CspLockConfirm")}</div>
                  <Checkbox
                    value={state.cspLockConfirm}
                    onChange={() => this.setState({ cspLockConfirm: !state.cspLockConfirm })}
                  />
                </div>
              )}
            </GridCell>
          </GridSystem>
          <div className="cspSettings-row_switch"></div>
          <InputGroup>
            <Label
              tooltipMessage={resx.get("plCspHeader.Help")}
              label={resx.get("plCspHeader")}
            />
            <MultiLineInputWithError
              error={state.error["CspHeader"]}
              value={state.cspSettings.CspHeader}
              errorMessage={state.error["CspHeaderErrors"].join(", ")}
              onChange={this.onCspHeaderValueChange.bind(this, "CspHeader")}
            />
          </InputGroup>

          <InputGroup>
            <Label
              tooltipMessage={resx.get("plCspReportingHeader.Help")}
              label={resx.get("plCspReportingHeader")}
            />
            <MultiLineInputWithError
              error={state.error["CspReportingHeader"]}
              errorMessage={state.error["CspReportingHeaderErrors"].join(", ")}
              value={state.cspSettings.CspReportingHeader}
              onChange={this.onCspReportingHeaderValueChange.bind(
                this,
                "CspReportingHeader"
              )}
            />
          </InputGroup>
          <div className="buttons-box">
            <Button
              disabled={!this.props.cspSettingsClientModified}
              type="neutral"
              onClick={this.onCancel.bind(this)}
            >
              {resx.get("Cancel")}
            </Button>
            <Button
              disabled={!this.props.cspSettingsClientModified || (state.cspSettings.CspHeaderFixed && !state.cspLockConfirm)}
              type="primary"
              onClick={this.onUpdate.bind(this)}
            >
              {resx.get("Save")}
            </Button>
          </div>
        </div>
      );
    } else return <div />;
  }
}

CspSettingsPanelBody.propTypes = {
  dispatch: PropTypes.func.isRequired,
  tabIndex: PropTypes.number,
  cspSettings: PropTypes.object,
  cspSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
logger.debug(`>>> Entering mapStateToProps(state=${state})`);
logger.debug(`<<< Exiting mapStateToProps(state=${state})`);
  return {
    tabIndex: state.pagination.tabIndex,
    cspSettings: state.security.cspSettings,
    cspSettingsClientModified: state.security.cspSettingsClientModified,
  };
}

export default connect(mapStateToProps)(CspSettingsPanelBody);
