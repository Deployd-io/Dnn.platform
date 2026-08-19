import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Collapsible as Collapse, IconButton, Button } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import resx from "../../../resources";
import utils from "../../../utils";
const logger = console;
import { security as SecurityActions } from "../../../actions";

class ApiTokenRow extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            collapsed: true,
            collapsedClass: true,
            repainting: false
        };
        this.timeout = 10;
		logger.debug(`<<< Exiting constructor(props=${props})`);
        // this.handleClick = this.handleClick.bind(this);
    }

    UNSAFE_componentWillReceiveProps() {
		logger.debug('>>> Entering UNSAFE_componentWillReceiveProps()');
        this.setState({});
		logger.debug('<<< Exiting UNSAFE_componentWillReceiveProps()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        document.addEventListener("click", this.handleClick);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    handleClick(event) {
		logger.debug(`>>> Entering handleClick(event=${event})`);
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

        if (!this.node.contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)) {

            this.timeout = 475;
            this.collapse();
        } else {

            this.timeout = 0;
			logger.debug(`<<< Exiting handleClick(event=${event})`);
        }
    }

    uncollapse() {
		logger.debug('>>> Entering uncollapse()');
        setTimeout(() => {
            this.setState({
                collapsed: false
            });
        }, this.timeout);
		logger.debug('<<< Exiting uncollapse()');
    }

    collapse() {
		logger.debug('>>> Entering collapse()');
        this.setState({
            collapsed: true
        });
		logger.debug('<<< Exiting collapse()');
    }

    toggle() {
		logger.debug('>>> Entering toggle()');
        if (this.state.collapsed) {
            this.uncollapse();
        } else {
            this.collapse();
			logger.debug('<<< Exiting toggle()');
        }
    }

    render() {
		logger.debug('>>> Entering render()');
        const { props, state } = this;
        let status;
        let statusClass = "inactive";
		logger.debug(`render(): statusClass → ${statusClass}`);
        if (utils.dateInPast(new Date(props.apiToken.ExpiresOn), new Date())) {
            status = resx.get("Expired");
        } else {
            if (props.apiToken.IsRevoked) {
                status = resx.get("Revoked");
            } else {
                status = resx.get("Active");
                statusClass = "active";
            }
        }

		logger.debug('<<< Exiting render()');
        return (
            <div ref={node => this.node = node} className={"collapsible-logitemdetail " + state.collapsed + (props.className ? (" " + props.className) : "")}>
                <div className={"collapsible-logitemdetail-header " + state.collapsed}>
                    <div className="term-header">
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "17%" }}>
                            <div className="term-label-wrapper">
                                <span>{props.apiToken.TokenName}&nbsp;</span>
                            </div>
                        </div>
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "18%" }}>
                            <div className="term-label-wrapper">
                                <span>{props.apiToken.PortalName}&nbsp;</span>
                            </div>
                        </div>
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "8%" }}>
                            <div className="term-label-wrapper">
                                <span>{props.scopes.filter((item) => item.value === this.props.apiToken.Scope)[0].label}</span>
                            </div>
                        </div>
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "10%" }}>
                            <div className="term-label-wrapper">
                                <span>{utils.formatDate(props.apiToken.CreatedOnDate)}&nbsp;</span>
                            </div>
                        </div>
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "20%" }}>
                            <div className="term-label-wrapper">
                                <span>{props.apiToken.CreatedByUser}&nbsp;</span>
                            </div>
                        </div>
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "10%" }}>
                            <div className="term-label-wrapper">
                                <span>{utils.formatDate(props.apiToken.ExpiresOn)}&nbsp;</span>
                            </div>
                        </div>
                        <div className="term-label" onClick={this.toggle.bind(this)} style={{ width: "12%" }}>
                            <div className="term-label-wrapper">
                                <span className={statusClass}>{status}&nbsp;</span>
                            </div>
                        </div>
                        <div className="term-label" style={{ width: "5%" }}>
                            <div className="term-label-wrapper">
                                <IconButton type="edit"
                                    className={"edit-icon " + (this.state.collapsed)}
                                    onClick={this.toggle.bind(this)}
                                    title={resx.get("Edit")} />
                            </div>
                        </div>
                    </div>
                </div>
                <Collapse className="logitem-collapsible" isOpened={!this.state.collapsed}>{!state.collapsed && props.children}
                    {!state.collapsed && <div className="buttons-box">
                        <Button
                            type="danger"
                            onClick={() => {
                                utils.utilities.confirm(resx.get("DeleteApiKey.Confirm"), resx.get("Yes"), resx.get("No"), () => {
                                    this.props.dispatch(SecurityActions.revokeOrDeleteApiToken(this.props.apiToken.ApiTokenId, true, () => {
                                        this.collapse();
                                        this.props.onClose();
                                    }));
                                });
                            }}>
                            {resx.get("DeleteApiKey")}
                        </Button>
                        {statusClass === "active" && (
                            <Button
                                type="secondary"
                                onClick={() => {
                                    utils.utilities.confirm(resx.get("Revoke.Confirm"), resx.get("Yes"), resx.get("No"), () => {
                                        this.props.dispatch(SecurityActions.revokeOrDeleteApiToken(this.props.apiToken.ApiTokenId, false, () => {
                                            this.props.onClose();
                                        }));
                                    });
                                }}>
                                {resx.get("Revoke")}
                            </Button>
                        )}
                    </div>}
                </Collapse>
            </div>
        );
    }
}

ApiTokenRow.propTypes = {
    dispatch: PropTypes.func.isRequired,
    apiToken: PropTypes.object.isRequired,
    scopes: PropTypes.array.isRequired,
    onClose: PropTypes.func.isRequired,
    className: PropTypes.string,
    children: PropTypes.node,
};

function mapStateToProps() {
	logger.debug('>>> Entering mapStateToProps()');
	logger.debug('<<< Exiting mapStateToProps()');
    return {};
}

export default connect(mapStateToProps)(ApiTokenRow);
