import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible as Collapse, Checkbox, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class ProviderRow extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
		logger.debug('<<< Exiting constructor()');
    }

    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        let opened = (this.props.openId !== "" && this.props.name === this.props.openId);
        this.setState({
            opened
        });
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    toggle() {
		logger.debug('>>> Entering toggle()');
        if ((this.props.openId !== "" && this.props.name === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.name);
			logger.debug('<<< Exiting toggle()');
        }
    }

    updateStatus(event) {
		logger.debug(`>>> Entering updateStatus(event=${event})`);
        const {props} = this;
        this.props.onUpdateStatus(props.providerId, event);
		logger.debug(`<<< Exiting updateStatus(event=${event})`);
    }

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.name === this.props.openId);
        if (props.visible) {
            return (
                <div className={"collapsible-component-extention-url-providers" + (opened ? " row-opened" : "")}>
                    <div className={"collapsible-providers " + !opened} >
                        <div className={"row"}>
                            <div className="provider-item item-row-name">
                                {props.name}
                            </div>
                            <div className="provider-item item-row-enabled">
                                <Checkbox value={props.enabled} onChange={this.updateStatus.bind(this) } />
                            </div>
                            <div className="provider-item item-row-editButton">
                                <div className={opened ? "edit-icon-active" : "edit-icon"}onClick={this.toggle.bind(this)}><SvgIcons.EditIcon /></div>
                            </div>
                        </div>
                    </div>
                    <Collapse accordion={true} isOpened={opened} className="collapsible-providers-body">{opened && props.children}</Collapse>
                </div>
            );
        }
        else {
			logger.debug('<<< Exiting render()');
            return <div />;
        }
    }
}

ProviderRow.propTypes = {
    providerId: PropTypes.number,
    name: PropTypes.string,
    enabled: PropTypes.bool,    
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool,
    onUpdateStatus: PropTypes.func,
    children: PropTypes.node,
};

ProviderRow.defaultProps = {
    collapsed: true,
    visible: true
};

export default (ProviderRow);
