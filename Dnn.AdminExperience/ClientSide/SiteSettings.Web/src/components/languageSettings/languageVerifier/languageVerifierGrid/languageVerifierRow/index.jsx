import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;

class LanguageVerifierRow extends Component {

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
		logger.debug('<<< Exiting componentDidMount()');
    }

    toggle() {
		logger.debug('>>> Entering toggle()');
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        }
        else {
            this.props.OpenCollapse(this.props.id);
			logger.debug('<<< Exiting toggle()');
        }
    }    

     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.debug('<<< Exiting render()');
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
