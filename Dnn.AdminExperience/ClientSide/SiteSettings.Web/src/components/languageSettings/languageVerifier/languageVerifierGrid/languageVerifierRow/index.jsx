import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class LanguageVerifierRow extends Component {

    componentDidMount() {
		logger.error('>>> Entering componentDidMount()', { "event.action": "componentDidMount" });
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
		logger.warn('<<< Exiting componentDidMount()', { "event.action": "componentDidMount" });
    }

    toggle() {
		logger.error('>>> Entering toggle()', { "event.action": "toggle" });
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        }
        else {
            this.props.OpenCollapse(this.props.id);
			logger.warn('<<< Exiting toggle()', { "event.action": "toggle" });
        }
    }    

     
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div className={"collapsible-component-verifier" + (opened ? " row-opened" : "")}>
                <div className={"collapsible-header-verifier " + !opened} >
                    <div className={"row"}>
                        <div className="verifier-item item-row-name">
                            {props.text}
                        </div>     
                        <div className="arrow-icon" onClick={this.toggle.bind(this)}><SvgIcons.ArrowDownIcon /></div>
                    </div>
                </div>
                <Collapsible isOpened={opened} className="collapsible-body-verifier">{opened && props.children}</Collapsible>
            </div>
        );
    }
}

LanguageVerifierRow.propTypes = {
    text: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    children: PropTypes.node
};

LanguageVerifierRow.defaultProps = {
    collapsed: true
};
export default (LanguageVerifierRow);
