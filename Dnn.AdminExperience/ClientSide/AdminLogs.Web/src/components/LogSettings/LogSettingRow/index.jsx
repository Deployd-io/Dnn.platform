import PropTypes from "prop-types";
import React, { Component } from "react";
import { GridCell, Collapsible as Collapse, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";
const logger = console;


class LogSettingRow extends Component {
    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.handleClick = this.handleClick.bind(this);
		logger.debug('<<< Exiting constructor()');
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
    UNSAFE_componentWillMount() {
		logger.debug('>>> Entering UNSAFE_componentWillMount()');
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
        this.setState({
            opened
        });
		logger.debug('<<< Exiting UNSAFE_componentWillMount()');
    }

    handleClick(event) {
		logger.debug(`>>> Entering handleClick(event=${event})`);
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted) { return; }

        if (!this.node.contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)) {

            this.timeout = 475;
            if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
                this.props.Collapse();
            }
        } else {

            this.timeout = 0;
			logger.debug(`<<< Exiting handleClick(event=${event})`);
        }
    }
    toggle() {
		logger.debug('>>> Entering toggle()');
        if ((this.props.openId !== "" && this.props.id === this.props.openId)) {
            this.props.Collapse();
        } else {
            this.props.OpenCollapse(this.props.id);
			logger.debug('<<< Exiting toggle()');
        }
    }
     
    render() {
		logger.debug('>>> Entering render()');
        const {props} = this;
        let opened = (this.props.openId !== "" && this.props.id === this.props.openId);
		logger.debug(`render(): opened → ${opened}`);
        let uniqueId = "settingrow-" + Math.random() + Date.now();
		logger.debug(`render(): uniqueId → ${uniqueId}`);
        
		logger.debug('<<< Exiting render()');
        return (
            <div ref={node => this.node = node} className={"collapsible-component-log" + (opened ? " row-opened" : "") } id={uniqueId}>
                {props.visible && <div className={"collapsible-header-log " + !opened} >
                    <GridCell title={props.typeName} columnSize={40} >
                        {props.typeName}</GridCell>
                    <GridCell columnSize={20} >
                        {props.website}</GridCell>
                    <GridCell columnSize={15} >
                        {props.activeStatus}</GridCell>
                    <GridCell columnSize={20} >
                        {props.fileName}&nbsp; </GridCell>
                    {!props.readOnly &&
                        <GridCell columnSize={5} >
                            <div className={"edit-icon " + !opened} onClick={this.toggle.bind(this) }>
                                <SvgIcons.EditIcon />
                            </div>
                        </GridCell>
                    }
                </div>
                }
                <Collapse isOpened={opened} className="collapsible-body-log">{opened && props.children}</Collapse>
            </div>
        );
    }
}

LogSettingRow.propTypes = {
    cssClass: PropTypes.string,
    typeName: PropTypes.string,
    website: PropTypes.string,
    activeStatus: PropTypes.string,
    fileName: PropTypes.string,
    className: PropTypes.string,
    OpenCollapse: PropTypes.func,
    Collapse: PropTypes.func,
    id: PropTypes.string,
    openId: PropTypes.string,
    visible: PropTypes.bool,
    readOnly: PropTypes.bool,
    children: PropTypes.bool,
};

LogSettingRow.defaultProps = {
    collapsed: true,
    visible: true,
    readOnly: false
};
export default (LogSettingRow);
