import React, { Component } from "react";
import PropTypes from "prop-types";
import {
    Label,
    InputGroup,
    SingleLineInputWithError,
} from "@dnnsoftware/dnn-react-common";
import GlobalIcon from "./GlobalIcon";
const logger = console;

export default class EditPwdBlock extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            canChange: props.value === "",
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }
    render() {
		logger.debug('>>> Entering render()');
        const { props, state } = this;

		logger.debug('<<< Exiting render()');
        return (
            <InputGroup>
                <Label
                    className="title"
                    tooltipMessage={props.tooltip}
                    label={props.label}
                    style={{ width: "auto" }}
                />
                {props.isGlobal && <GlobalIcon />}
                {state.canChange ? (
                    <SingleLineInputWithError
                        value={props.value}
                        type="password"
                        onChange={props.onChange}
                        error={!!props.error}
                        errorMessage={props.error}
                    />
                ) : (
                    <div className="edit-pwd-button">
                        <div>
                            <a
                                href="#"
                                onClick={(e) => {
                                    e.preventDefault();
                                    props.onClear();
                                    this.setState({ canChange: true });
                                }}
                            >
                [ {props.changeButtonText} ]
                            </a>
                        </div>
                    </div>
                )}
            </InputGroup>
        );
    }
}

EditPwdBlock.propTypes = {
    label: PropTypes.string,
    changeButtonText: PropTypes.string,
    tooltip: PropTypes.string,
    value: PropTypes.string,
    isGlobal: PropTypes.bool.isRequired,
    onChange: PropTypes.func,
    onClear: PropTypes.func,
    error: PropTypes.string,
};

EditPwdBlock.defaultProps = {
    isGlobal: false,
};
